using Tharga.Console.Commands.Base;

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

        _crawler.Scheduler.SchedulerEvent += (s, e) => { OutputInformation($"Q: {e.QueueCount}, C: {e.CrawlingCount}, H: {e.CrawledCount}"); };

        var options = new CrawlerOptions();
        var result = await _crawler.StartAsync(uri, options, CancellationToken.None);

        var title = new[] { "Status", "Content", "Uri" };
        var data = result.Pages.Select(x => new[] { $"{x.StatusCode}", $"{x.ContentType}", x.FinalUri.AbsoluteUri });
        OutputTable(title, data);
    }
}