using Tharga.Crawler.Entity;

namespace Tharga.Crawler;

public class PageCompleteEventArgs : EventArgs
{
    internal PageCompleteEventArgs(CrawlContent crawlContent)
    {
        CrawlContent = crawlContent;
    }

    public CrawlContent CrawlContent { get; }
}