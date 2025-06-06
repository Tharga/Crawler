using System.Diagnostics;
using Tharga.Crawler.Entity;

namespace Tharga.Crawler.SampleConsole;

public class CustomPageProcessor : PageProcessor.BasicPageProcessor
{
    public override IAsyncEnumerable<ToCrawl> ProcessAsync(CrawlContent page, CrawlerOptions options, CancellationToken cancellationToken)
    {
        //yield return new ToCrawl();
        throw new NotImplementedException();
        Debugger.Break();
    }
}