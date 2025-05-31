using Tharga.Crawler.Scheduler;

namespace Tharga.Crawler;

public interface ICrawler
{
    IScheduler Scheduler { get; }
    event EventHandler<CrawlerCompleteEventArgs> CrawlerCompleteEvent;
    event EventHandler<PageCompleteEventArgs> PageCompleteEvent;
    event EventHandler<PageFailedEventArgs> PageFailedEvent;
    Task<CrawlerResult> StartAsync(Uri uri, CrawlerOptions options = null, CancellationToken cancellationToken = default);
    Task<CrawlerResult> StartAsync(Uri[] uris, CrawlerOptions options = null, CancellationToken cancellationToken = default);
}