namespace Tharga.Crawler;

public record CrawlerOptions
{
    public TimeSpan? MaxCrawlTime { get; init; }
    public int NumberOfCrawlers { get; init; } = 3;
}