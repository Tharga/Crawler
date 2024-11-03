using Microsoft.Extensions.Logging;
using Tharga.Crawler.Downloader;
using Tharga.Crawler.Helper;
using Tharga.Crawler.Processor;
using Tharga.Crawler.Scheduler;

namespace Tharga.Crawler;

internal class Crawler : ICrawler
{
    private readonly IScheduler _scheduler;
    private readonly IPageProcessor _pageProcessor;
    private readonly IDownloader _downloader;
    private readonly ILogger<Crawler> _logger;

    public Crawler(IScheduler scheduler, IPageProcessor pageProcessor, IDownloader downloader, ILogger<Crawler> logger)
    {
        _scheduler = scheduler;
        _pageProcessor = pageProcessor;
        _downloader = downloader;
        _logger = logger;
    }

    public IScheduler Scheduler => _scheduler;
    public event EventHandler<CrawlerCompleteEventArgs> CrawlerCompleteEvent;

    public async Task<CrawlerResult> StartAsync(Uri uri, CrawlerOptions options, CancellationToken cancellationToken)
    {
        if (options.NumberOfCrawlers <= 0) throw new InvalidOperationException("Need at least one crawler.");

        using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        await _scheduler.Enqueue(new ToCrawl { RequestUri = uri });

        //NOTE: Set a limitation in time to crawl
        if (options.MaxCrawlTime.HasValue)
        {
            var cts = linkedTokenSource;
            _ = Task.Run(async () =>
            {
                await Task.Delay(options.MaxCrawlTime.Value, cts.Token);
                _logger.LogInformation("Crawl timeout reached after {timeoutMilliseconds} ms.", options.MaxCrawlTime.Value.TotalMicroseconds);
                await cts.CancelAsync();
            }, linkedTokenSource.Token);
        }

        await RunCrawlers(_scheduler, _pageProcessor, _downloader, options, linkedTokenSource.Token);

        CrawlerCompleteEvent?.Invoke(this, new CrawlerCompleteEventArgs(linkedTokenSource.IsCancellationRequested));

        return new CrawlerResult
        {
            Pages = await _scheduler.GetAllCrawled().ToArrayAsync(CancellationToken.None),
            IsCancelled = linkedTokenSource.IsCancellationRequested,
        };
    }

    async Task RunCrawlers(IScheduler scheduler, IPageProcessor pageProcessor, IDownloader downloader, CrawlerOptions options, CancellationToken cancellationToken)
    {
        var crawlerStates = new CrawlerStates();

        var tasks = new List<Task>();
        for (var i = 0; i < options.NumberOfCrawlers; i++)
        {
            var crawlerNo = i;
            crawlerStates.Crawlers.Add(crawlerNo, true);
            tasks.Add(Task.Run(() => CrawlAsync(crawlerNo, scheduler, pageProcessor, downloader, crawlerStates, cancellationToken), CancellationToken.None));
        }

        await Task.WhenAll(tasks);
    }

    async Task CrawlAsync(int crawlerNo, IScheduler scheduler, IPageProcessor pageProcessor, IDownloader downloader, CrawlerStates crawlerStates, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Take the next URL to crawl from the scheduler.
                var toCrawl = await scheduler.TakeNextToCrawlAsync(cancellationToken);

                if (toCrawl == null)
                {
                    crawlerStates.Crawlers[crawlerNo] = false;
                    _logger.LogTrace("Crawler no {crawlerNo} is waiting for more work. There are {workerCount} other workers still working.", crawlerNo, crawlerStates.Crawlers.Count(x => x.Value));
                    await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
                    if (crawlerStates.Crawlers.Count(x => x.Value) == 0) return; //No workers are working, end the crawl
                }
                else
                {
                    crawlerStates.Crawlers[crawlerNo] = true;

                    var result = await downloader.GetAsync(toCrawl, cancellationToken);
                    if (result != null)
                    {
                        await scheduler.AddAsync(result.ToCrawled());
                        _logger.LogInformation("Crawler {crawlerNo} processed {uri}.", crawlerNo, result.RequestUri);
                        await foreach (var item in pageProcessor.ProcessAsync(result).WithCancellation(cancellationToken))
                        {
                            await scheduler.Enqueue(item);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}