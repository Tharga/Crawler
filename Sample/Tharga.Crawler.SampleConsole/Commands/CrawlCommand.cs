using Tharga.Console.Commands.Base;
using Tharga.Crawler.Downloader;
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
        IEnumerable<Uri> uris =
        [
            new("https://thargelion.se/"),
            new("https://thargelion.com/"),
            new("https://eplicta.se/"),
            new("https://seafun.se/"),
        ];
        var uri = QueryParam("Url", param, uris);

        _crawler.Scheduler.SchedulerEvent += (_, e) => { OutputInformation($"Q: {e.QueueCount}, C: {e.CrawlingCount}, H: {e.CrawledCount}, A: {e.Action}"); };
        //_crawler.CrawlerCompleteEvent += (_, e) => { OutputInformation("Crawl completed."); };
        //_crawler.PageCompleteEvent += (_, e) => { OutputInformation($"Page '{e.CrawlContent.FinalUri}' completed."); };

        var options = new CrawlerOptions
        {
            NumberOfCrawlers = 20,
            SchedulerOptions = new SchedulerOptions
            {
                MaxQueueCount = 30
            },
            DownloadOptions = new DownloadOptions
            {
                UserAgent = UserAgentLibrary.Chrome
            }
        };
        var result = await _crawler.StartAsync(uri, options, CancellationToken.None);

        var title = new[] { "Lvl", "Status", "R", "Content", "Redirects", "Title", "Uri" };

        OutputInformation("Requested");
        var requested = result.GetRequestedPages().OrderBy(x => x.RequestUri.AbsoluteUri).Select(x => new[] { $"{x.Level}", $"{x.StatusCode}", $"{x.RetryCount}", $"{x.ContentType}", $"{x.Redirects.Length}", x.Title, x.RequestUri.AbsoluteUri });
        OutputTable(title, requested);

        OutputInformation("Final");
        var final = result.GetFinalPages().OrderBy(x => x.FinalUri.AbsoluteUri).Select(x => new[] { $"{x.Level}", $"{x.StatusCode}", $"{x.RetryCount}", $"{x.ContentType}", $"{x.Redirects.Length}", x.Title, x.FinalUri.AbsoluteUri });
        OutputTable(title, final);
    }
}