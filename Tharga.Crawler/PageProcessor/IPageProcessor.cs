namespace Tharga.Crawler.Processor;

public interface IPageProcessor
{
    IAsyncEnumerable<ToCrawl> ProcessAsync(CrawlContent page);
}