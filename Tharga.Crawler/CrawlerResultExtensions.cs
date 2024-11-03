using Tharga.Crawler.Entity;

namespace Tharga.Crawler;

public static class CrawlerResultExtensions
{
    public static IEnumerable<Crawled> GetRequestedPages(this CrawlerResult item)
    {
        return item.RequestedPages;
    }

    public static IEnumerable<Crawled> GetFinalPages(this CrawlerResult item)
    {
        return item.RequestedPages.OrderBy(x => x.Redirects.Length).DistinctBy(x => x.FinalUri);
    }
}