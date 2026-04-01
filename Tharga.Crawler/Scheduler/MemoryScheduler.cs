using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Tharga.Crawler.Entity;

namespace Tharga.Crawler.Scheduler;

public class MemoryScheduler : IScheduler
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly IUriService _uriService;
    private readonly ILogger<MemoryScheduler> _logger;
    private readonly ConcurrentDictionary<Uri, ScheduleItem> _schedule = new();
    private int _queuedCount;
    private int _crawlingCount;
    private int _completeCount;

    public MemoryScheduler(IUriService uriService, ILogger<MemoryScheduler> logger = null)
    {
        _uriService = uriService;
        _logger = logger;
    }

    public event EventHandler<SchedulerEventArgs> SchedulerEvent;
    public event EventHandler<EnqueuedEventArgs> EnqueuedEvent;

    public virtual async Task EnqueueAsync(ToCrawl toCrawl, SchedulerOptions options)
    {
        await EnqueueAsync([toCrawl], options);
    }

    public virtual async Task EnqueueAsync(ToCrawl[] items, SchedulerOptions options)
    {
        var enqueued = new List<ToCrawl>();

        foreach (var toCrawl in items)
        {
            if (!await _uriService.ShouldEnqueueAsync(toCrawl.Parent?.RequestUri, toCrawl.RequestUri)) continue;

            if (_schedule.Count >= options?.MaxQueueCount)
            {
                _logger?.LogWarning("Queue has {queueCount} items and is full.", _schedule.Count);
                break;
            }

            var mutated = toCrawl with { RequestUri = await _uriService.MutateUriAsync(toCrawl.RequestUri) };

            var scheduleItem = new ScheduleItem { ToCrawl = mutated, State = ScheduleItemState.Queued };
            if (_schedule.TryAdd(mutated.RequestUri, scheduleItem))
            {
                Interlocked.Increment(ref _queuedCount);
                enqueued.Add(mutated);
            }
        }

        if (enqueued.Count > 0)
        {
            SchedulerEvent?.Invoke(this, new SchedulerEventArgs(Action.Enqueue, _queuedCount, _crawlingCount, _completeCount));
            EnqueuedEvent?.Invoke(this, new EnqueuedEventArgs(enqueued.ToArray()));
        }
    }

    public virtual async Task<ToCrawlScope> GetQueuedItemScope(CancellationToken cancellationToken)
    {
        try
        {
            await _lock.WaitAsync(cancellationToken);

            var item = _schedule.Values
                .OrderBy(x => x.ToCrawl.RetryCount)
                .ThenBy(x => x.ToCrawl.Level) //Shallow crawl
                .FirstOrDefault(x => x.State == ScheduleItemState.Queued);
            if (item == null) return null;

            var scheduleToQueue = item with { State = ScheduleItemState.Crawling };
            var queueUpdate = _schedule.TryUpdate(item.ToCrawl.RequestUri, scheduleToQueue, item);
            if (!queueUpdate) throw new InvalidOperationException("Unable to update queue item.");

            Interlocked.Decrement(ref _queuedCount);
            Interlocked.Increment(ref _crawlingCount);
            SchedulerEvent?.Invoke(this, new SchedulerEventArgs(Action.Crawl, _queuedCount, _crawlingCount, _completeCount));

            return new ToCrawlScope(item.ToCrawl, (toCrawl, state) =>
            {
                var scheduleComplete = new ScheduleItem { ToCrawl = toCrawl, State = state };
                var completeUpdate = _schedule.TryUpdate(item.ToCrawl.RequestUri, scheduleComplete, scheduleToQueue);
                if (!completeUpdate) throw new InvalidOperationException("Unable to update completed item.");

                Interlocked.Decrement(ref _crawlingCount);
                if (state == ScheduleItemState.Complete)
                    Interlocked.Increment(ref _completeCount);
                else
                    Interlocked.Increment(ref _queuedCount);

                SchedulerEvent?.Invoke(this, new SchedulerEventArgs(Action.Complete, _queuedCount, _crawlingCount, _completeCount));
            });
        }
        finally
        {
            _lock.Release();
        }
    }

    public IAsyncEnumerable<ToCrawl> GetQueued()
    {
        return _schedule.Values.Where(x => x.State == ScheduleItemState.Queued).Select(x => x.ToCrawl).ToAsyncEnumerable();
    }

    public virtual IAsyncEnumerable<Crawled> GetAllCrawled()
    {
        return _schedule.Values.Where(x => x.State == ScheduleItemState.Complete).Select(x => x.ToCrawl as Crawled).ToAsyncEnumerable();
    }
}