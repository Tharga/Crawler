using Tharga.Crawler.Entity;

namespace Tharga.Crawler.Helper;

internal static class CrawledConverter
{
    public static Crawled RemoveContent(this CrawlContent crawlContent)
    {
        return new Crawled
        {
            RequestUri = crawlContent.RequestUri,
            StatusCode = crawlContent.StatusCode,
            Redirects = crawlContent.Redirects,
            ContentType = crawlContent.ContentType,
            Parent = crawlContent.Parent,
            RetryCount = crawlContent.RetryCount,
            Message = crawlContent.Message,
        };
    }
}