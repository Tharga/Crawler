using Tharga.Crawler.Entity;

namespace Tharga.Crawler.PageProcessor;

public interface IPageProcessor
{
    IAsyncEnumerable<ToCrawl> ProcessAsync(CrawlContent page);
}