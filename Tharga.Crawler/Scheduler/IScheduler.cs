using Tharga.Crawler.Entity;

namespace Tharga.Crawler.Scheduler;

public interface IScheduler
{
    event EventHandler<SchedulerEventArgs> SchedulerEvent;

    Task EnqueueAsync(ToCrawl toCrawl, SchedulerOptions options);
    Task<ToCrawl> TakeNextToCrawlAsync(CancellationToken cancellationToken); //TODO: Should return a scope that can be committed, abandoned, cancelled or set to retry.
    Task AddAsync(Crawled result);
    IAsyncEnumerable<Crawled> GetAllCrawled();
}