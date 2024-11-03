namespace Tharga.Crawler.Entity;

public record CrawlContent : Crawled
{
    internal CrawlContent()
    {
    }

    public byte[] Content { get; init; }
}