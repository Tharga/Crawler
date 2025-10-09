using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Tharga.Crawler.Entity;
using Tharga.Crawler.Scheduler;

namespace Tharga.Crawler.SampleConsole;

public class CustomScheduler : MemoryScheduler
{
    public CustomScheduler(IUriService uriService, ILogger<MemoryScheduler> logger = null)
        : base(uriService, logger)
    {
    }

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