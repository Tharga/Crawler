using Tharga.Crawler.Entity;

namespace Tharga.Crawler;

public static class CrawlerResultExtensions
{
    public static IEnumerable<Crawled> GetRequestedPages(this CrawlerResult item)
    {
        return item.RequestedPages.Where(x => x != null);
    }

    public static IEnumerable<Crawled> GetFinalPages(this CrawlerResult item)
    {
        return item.RequestedPages.Where(x => x != null).OrderBy(x => x.Redirects.Length).DistinctBy(x => x.FinalUri);
    }
}