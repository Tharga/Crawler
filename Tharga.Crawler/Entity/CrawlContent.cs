namespace Tharga.Crawler.Entity;

public record CrawlContent : Crawled
{
    public CrawlContent()
    {
    }

    public byte[] Content { get; init; }
}