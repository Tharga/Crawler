using Tharga.Crawler.Entity;

namespace Tharga.Crawler.Scheduler;

public interface IScheduler
{
    event EventHandler<SchedulerEventArgs> SchedulerEvent;
    event EventHandler<EnqueuedEventArgs> EnqueuedEvent;

    Task EnqueueAsync(ToCrawl toCrawl, SchedulerOptions options);
    Task<ToCrawlScope> GetQueuedItemScope(CancellationToken cancellationToken);
    IAsyncEnumerable<ToCrawl> GetQueued();
    IAsyncEnumerable<Crawled> GetAllCrawled();
}