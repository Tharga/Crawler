namespace Tharga.Crawler;

public record CrawlContent : Crawled
{
    public byte[] Content { get; init; }
}