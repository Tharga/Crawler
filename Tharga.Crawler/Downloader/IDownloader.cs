namespace Tharga.Crawler.Downloader;

public interface IDownloader
{
    Task<CrawlContent> GetAsync(ToCrawl toCrawl, CancellationToken cancellationToken);
}