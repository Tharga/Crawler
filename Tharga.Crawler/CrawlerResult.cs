using Tharga.Crawler.Entity;

namespace Tharga.Crawler;

public record CrawlerResult
{
    internal Crawled[] Pages { get; init; }
    public required bool IsCancelled { get; init; }
}