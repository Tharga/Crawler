using Tharga.Crawler.Entity;

namespace Tharga.Crawler.Scheduler;

public interface IScheduler
{
    event EventHandler<SchedulerEventArgs> SchedulerEvent;

    Task EnqueueAsync(ToCrawl toCrawl, SchedulerOptions options);
    Task<ToCrawlScope> GetQueuedItemScope(CancellationToken cancellationToken);
    IAsyncEnumerable<Crawled> GetAllCrawled();
}