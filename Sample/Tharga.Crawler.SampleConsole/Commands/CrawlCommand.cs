using Tharga.Console.Commands.Base;
using Tharga.Crawler.Scheduler;

namespace Tharga.Crawler.SampleConsole.Commands;

internal class CrawlCommand : AsyncActionCommandBase
{
    private readonly ICrawler _crawler;

    public CrawlCommand(ICrawler crawler)
        : base("crawl")
    {
        _crawler = crawler;
    }

    public override async Task InvokeAsync(string[] param)
    {
        var uri = QueryParam("Url", param, [new Uri("https://eplicta.se/")]);

        _crawler.Scheduler.SchedulerEvent += (_, e) => { OutputInformation($"Q: {e.QueueCount}, C: {e.CrawlingCount}, H: {e.CrawledCount}, A: {e.Action}"); };
        //_crawler.CrawlerCompleteEvent += (_, e) => { OutputInformation("Crawl completed."); };
        //_crawler.PageCompleteEvent += (_, e) => { OutputInformation($"Page '{e.CrawlContent.FinalUri}' completed."); };

        var options = new CrawlerOptions
        {
            SchedulerOptions = new SchedulerOptions
            {
                //MaxQueueCount = 20
            }
        };
        var result = await _crawler.StartAsync(uri, options, CancellationToken.None);

        var title = new[] { "Level", "Status", "Content", "Redirects", "Final" };

        OutputInformation("Requested");
        var requested = result.GetRequestedPages().OrderBy(x => x.FinalUri.AbsoluteUri).Select(x => new[] { $"{x.Level}", $"{x.StatusCode}", $"{x.ContentType}", $"{x.Redirects.Length}", x.RequestUri.AbsoluteUri });
        OutputTable(title, requested);

        OutputInformation("Final");
        var final = result.GetFinalPages().OrderBy(x => x.FinalUri.AbsoluteUri).Select(x => new[] { $"{x.Level}", $"{x.StatusCode}", $"{x.ContentType}", $"{x.Redirects.Length}", x.FinalUri.AbsoluteUri });
        OutputTable(title, final);
    }
}