using System.Diagnostics;
using Tharga.Crawler.Downloader;
using Tharga.Crawler.Entity;

namespace Tharga.Crawler.SampleConsole;

public class CustomDownloader : HttpClientDownloader
{
    public override Task<CrawlContent> GetAsync(ToCrawl toCrawl, DownloadOptions downloadOptions, CancellationToken cancellationToken)
    {
        //return new CrawlContent();
        throw new NotImplementedException();
        Debugger.Break();
    }
}