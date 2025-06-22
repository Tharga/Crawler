using Tharga.Crawler.Downloader;
using Tharga.Crawler.PageProcessor;
using Tharga.Crawler.Scheduler;

namespace Tharga.Crawler;

public interface ICrawlerProvider
{
    ICrawler GetCrawlerInstance(IScheduler scheduler = null, IPageProcessor pageProcessor = null, IDownloader downloader = null);
}