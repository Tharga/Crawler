using Tharga.Crawler.Downloader;
using Tharga.Crawler.PageProcessor;
using Tharga.Crawler.Scheduler;

namespace Tharga.Crawler;

public record CrawlerRegistrationOptions
{
    public Func<IServiceProvider, ICrawler> Crawler { get; set; }
    public Func<IServiceProvider, IScheduler> Scheduler { get; set; }
    public Func<IServiceProvider, IPageProcessor> PageProcessor { get; set; }
    public Func<IServiceProvider, IDownloader> Downloader { get; set; }
    public Func<IServiceProvider, IUriService> UriService { get; set; }
}