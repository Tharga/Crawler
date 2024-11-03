namespace Tharga.Crawler;

public record CrawlerResult
{
    public required Crawled[] Pages { get; init; }
    public required bool IsCancelled { get; init; }
}