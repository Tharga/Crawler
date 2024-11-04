using Tharga.Crawler.Downloader;
using Tharga.Crawler.PageProcessor;
using Tharga.Crawler.Scheduler;

namespace Tharga.Crawler;

public record CrawlerRegistrationOptions
{
    public Func<IServiceProvider, ICrawler> Crawler { get; init; }
    public Func<IServiceProvider, IScheduler> Scheduler { get; init; }
    public Func<IServiceProvider, IPageProcessor> PageProcessor { get; init; }
    public Func<IServiceProvider, IDownloader> Downloader { get; init; }
}