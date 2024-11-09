using System.Diagnostics;
using Tharga.Crawler.Downloader;
using Tharga.Crawler.Entity;
using Tharga.Crawler.PageProcessor;
using Tharga.Crawler.Scheduler;

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

public class CustomPageProcessor : PageProcessorBase
{
    public override IAsyncEnumerable<ToCrawl> ProcessAsync(CrawlContent page, CrawlerOptions options, CancellationToken cancellationToken)
    {
        //yield return new ToCrawl();
        throw new NotImplementedException();
        Debugger.Break();
    }
}

public class CustomScheduler : MemoryScheduler
{
    public override Task EnqueueAsync(ToCrawl toCrawl, SchedulerOptions options)
    {
        throw new NotImplementedException();
        Debugger.Break();
    }

    public override IAsyncEnumerable<Crawled> GetAllCrawled()
    {
        //yield return new Crawled();
        throw new NotImplementedException();
        Debugger.Break();
    }

    public override Task<ToCrawlScope> GetQueuedItemScope(CancellationToken cancellationToken)
    {
        //return new ToCrawlScope();
        throw new NotImplementedException();
        Debugger.Break();
    }
}