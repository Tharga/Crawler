using Tharga.Crawler.Entity;

namespace Tharga.Crawler.Downloader;

public interface IDownloader
{
    Task<CrawlContent> GetAsync(ToCrawl toCrawl, DownloadOptions downloadOptions, CancellationToken cancellationToken);
}