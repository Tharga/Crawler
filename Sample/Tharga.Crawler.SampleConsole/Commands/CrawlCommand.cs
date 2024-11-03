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

        _crawler.Scheduler.SchedulerEvent += (_, e) => { OutputInformation($"Q: {e.QueueCount}, C: {e.CrawlingCount}, H: {e.CrawledCount}"); };
        //_crawler.CrawlerCompleteEvent += (_, _) => { OutputInformation("Crawl completed."); };
        //_crawler.PageCompleteEvent += (_, e) => { OutputInformation($"Page '{e.CrawlContent.FinalUri}' completed."); };

        var options = new CrawlerOptions
        {
            SchedulerOptions = new SchedulerOptions
            {
                MaxQueueCount = 30
            }
        };
        var result = await _crawler.StartAsync(uri, options, CancellationToken.None);

        var title = new[] { "Level", "Status", "Content", "Uri" };
        var data = result.Pages.OrderBy(x => x.FinalUri.AbsoluteUri).Select(x => new[] { $"{x.Level}", $"{x.StatusCode}", $"{x.ContentType}", x.FinalUri.AbsoluteUri });
        OutputTable(title, data);
    }
}