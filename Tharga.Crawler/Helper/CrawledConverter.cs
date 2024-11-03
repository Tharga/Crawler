namespace Tharga.Crawler.Helper;

internal static class CrawledConverter
{
    public static Crawled ToCrawled(this CrawlContent crawlContent)
    {
        return new Crawled
        {
            RequestUri = crawlContent.RequestUri,
            StatusCode = crawlContent.StatusCode,
            Redirects = crawlContent.Redirects,
            ContentType = crawlContent.ContentType,
        };
    }
}