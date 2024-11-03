using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Tharga.Crawler.Entity;

namespace Tharga.Crawler.Scheduler;

internal class MemoryScheduler : IScheduler
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly ILogger<MemoryScheduler> _logger;
    private readonly ConcurrentDictionary<Uri, ScheduleItem> _schedule = new();

    public MemoryScheduler(ILogger<MemoryScheduler> logger)
    {
        _logger = logger;
    }

    public event EventHandler<SchedulerEventArgs> SchedulerEvent;

    public Task EnqueueAsync(ToCrawl toCrawl, SchedulerOptions options)
    {
        if (_schedule.Count >= options?.MaxQueueCount)
        {
            _logger.LogWarning("Queue has {queueCount} items and is full.", _schedule.Count);
            return Task.CompletedTask;
        }

        var scheduleItem = new ScheduleItem { ToCrawl = toCrawl, State = ScheduleItemState.Queued };
        if (_schedule.TryAdd(toCrawl.RequestUri, scheduleItem))
        {
            SchedulerEvent?.Invoke(this, new SchedulerEventArgs(Action.Enqueue, _schedule.Values.Count(x => x.State == ScheduleItemState.Queued), _schedule.Values.Count(x => x.State == ScheduleItemState.Crawling), _schedule.Values.Count(x => x.State == ScheduleItemState.Complete)));
        }

        return Task.CompletedTask;
    }

    public async Task<ToCrawlScope> GetQueuedItemScope(CancellationToken cancellationToken)
    {
        try
        {
            await _lock.WaitAsync(cancellationToken);

            var item = _schedule.Values.FirstOrDefault(x => x.State == ScheduleItemState.Queued);
            if (item == null) return null;

            var scheduleToQueue = item with { State = ScheduleItemState.Crawling };
            var queueUpdate = _schedule.TryUpdate(item.ToCrawl.RequestUri, scheduleToQueue, item);
            if (!queueUpdate) throw new InvalidOperationException("Unable to update queue item.");

            SchedulerEvent?.Invoke(this, new SchedulerEventArgs(Action.Crawl, _schedule.Values.Count(x => x.State == ScheduleItemState.Queued), _schedule.Values.Count(x => x.State == ScheduleItemState.Crawling), _schedule.Values.Count(x => x.State == ScheduleItemState.Complete)));

            return new ToCrawlScope(item.ToCrawl, e =>
            {
                var scheduleComplete = new ScheduleItem { ToCrawl = e, State = ScheduleItemState.Complete };
                var completeUpdate = _schedule.TryUpdate(item.ToCrawl.RequestUri, scheduleComplete, scheduleToQueue);
                if (!completeUpdate) throw new InvalidOperationException("Unable to update completed item.");
                SchedulerEvent?.Invoke(this, new SchedulerEventArgs(Action.Complete, _schedule.Values.Count(x => x.State == ScheduleItemState.Queued), _schedule.Values.Count(x => x.State == ScheduleItemState.Crawling), _schedule.Values.Count(x => x.State == ScheduleItemState.Complete)));
            });
        }
        finally
        {
            _lock.Release();
        }
    }

    public IAsyncEnumerable<Crawled> GetAllCrawled()
    {
        return _schedule.Values.Where(x => x.State == ScheduleItemState.Complete).Select(x => x.ToCrawl as Crawled).ToAsyncEnumerable();
    }
}