using Tharga.Crawler.Entity;

namespace Tharga.Crawler;

public class PageFailedEventArgs : PageCompleteEventArgs
{
    internal PageFailedEventArgs(CrawlContent crawlContent)
        : base(crawlContent)
    {
    }
}