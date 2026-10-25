using System.Net.Mime;

namespace Tharga.Crawler.Entity;

public record Crawled : ToCrawl
{
    internal Crawled()
    {
    }

    public required int StatusCode { get; init; }
    // Record equality on crawl results is not relied on; pages are deduplicated by FinalUri.
    // ReSharper disable once TypeWithSuspiciousEqualityIsUsedInRecord.Global
    public required Uri[] Redirects { get; init; }
    public Uri FinalUri => Redirects?.LastOrDefault() ?? RequestUri;
    public required ContentType ContentType { get; init; }
    public required TimeSpan? DownloadTime { get; init; }
    public required string Title { get; init; }
    public required string Message { get; init; }
}