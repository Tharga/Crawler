using Tharga.Crawler.Scheduler;

namespace Tharga.Crawler;

public interface ICrawler
{
    IScheduler Scheduler { get; }
    event EventHandler<CrawlerCompleteEventArgs> CrawlerCompleteEvent;
    event EventHandler<PageCompleteEventArgs> PageCompleteEvent;
    Task<CrawlerResult> StartAsync(Uri uri, CrawlerOptions options = default, CancellationToken cancellationToken = default);
    Task<CrawlerResult> StartAsync(Uri[] uris, CrawlerOptions options = default, CancellationToken cancellationToken = default);
}