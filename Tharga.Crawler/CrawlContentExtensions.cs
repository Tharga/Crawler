using Tharga.Crawler.Entity;

namespace Tharga.Crawler;

internal static class CrawlContentExtensions
{
    public static ResponseCodeCategory GetResponseCodeCategory(this CrawlContent crawlContent)
    {
        var statusCode = (int)(crawlContent?.StatusCode ?? 0);

        if (statusCode >= 100 && statusCode <= 199) return ResponseCodeCategory.Information;
        if (statusCode >= 200 && statusCode <= 299) return ResponseCodeCategory.Success;
        if (statusCode >= 300 && statusCode <= 399) return ResponseCodeCategory.Redirection;
        if (statusCode >= 400 && statusCode <= 499) return ResponseCodeCategory.ClientError;
        if (statusCode >= 500 && statusCode <= 599) return ResponseCodeCategory.ServerError;
        throw new InvalidOperationException($"Cannot convert '{crawlContent?.StatusCode}' to {nameof(ResponseCodeCategory)}");
    }
}