﻿using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Tharga.Crawler.Entity;

namespace Tharga.Crawler.Scheduler;

public class MemoryScheduler : IScheduler
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly IUriService _uriService;
    private readonly ILogger<MemoryScheduler> _logger;
    private readonly ConcurrentDictionary<Uri, ScheduleItem> _schedule = new();

    public MemoryScheduler(IUriService uriService, ILogger<MemoryScheduler> logger = null)
    {
        _uriService = uriService;
        _logger = logger;
    }

    public event EventHandler<SchedulerEventArgs> SchedulerEvent;
    public event EventHandler<EnqueuedEventArgs> EnqueuedEvent;

    public virtual async Task EnqueueAsync(ToCrawl toCrawl, SchedulerOptions options)
    {
        if (_schedule.Count >= options?.MaxQueueCount)
        {
            _logger?.LogWarning("Queue has {queueCount} items and is full.", _schedule.Count);
            return;
        }

        if (!await _uriService.ShouldIncludeAsync(toCrawl.RequestUri)) return;

        toCrawl = toCrawl with { RequestUri = await _uriService.MutateUriAsync(toCrawl.RequestUri) };

        var scheduleItem = new ScheduleItem { ToCrawl = toCrawl, State = ScheduleItemState.Queued };
        if (_schedule.TryAdd(toCrawl.RequestUri, scheduleItem))
        {
            SchedulerEvent?.Invoke(this, new SchedulerEventArgs(Action.Enqueue, _schedule.Values.Count(x => x.State == ScheduleItemState.Queued), _schedule.Values.Count(x => x.State == ScheduleItemState.Crawling), _schedule.Values.Count(x => x.State == ScheduleItemState.Complete)));
            EnqueuedEvent?.Invoke(this, new EnqueuedEventArgs(toCrawl));
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

            SchedulerEvent?.Invoke(this, new SchedulerEventArgs(Action.Crawl, _schedule.Values.Count(x => x.State == ScheduleItemState.Queued), _schedule.Values.Count(x => x.State == ScheduleItemState.Crawling), _schedule.Values.Count(x => x.State == ScheduleItemState.Complete)));

            return new ToCrawlScope(item.ToCrawl, (toCrawl, state) =>
            {
                var scheduleComplete = new ScheduleItem { ToCrawl = toCrawl, State = state };
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

    public virtual IAsyncEnumerable<Crawled> GetAllCrawled()
    {
        return _schedule.Values.Where(x => x.State == ScheduleItemState.Complete).Select(x => x.ToCrawl as Crawled).ToAsyncEnumerable();
    }
}