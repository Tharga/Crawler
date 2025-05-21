namespace Tharga.Crawler.Entity;

public record CrawlContent : Crawled
{
    public byte[] Content { get; init; }
}