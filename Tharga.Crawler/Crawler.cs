using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Tharga.Crawler.Downloader;
using Tharga.Crawler.Entity;
using Tharga.Crawler.Helper;
using Tharga.Crawler.PageProcessor;
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
    public event EventHandler<PageCompleteEventArgs> PageCompleteEvent;

    public async Task<CrawlerResult> StartAsync(Uri uri, CrawlerOptions options, CancellationToken cancellationToken)
    {
        if (options.NumberOfCrawlers <= 0) throw new InvalidOperationException("Need at least one crawler.");

        using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        await _scheduler.EnqueueAsync(new ToCrawl { RequestUri = uri, RetryCount = 0, Parent = null }, options.SchedulerOptions);

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

        var crawlerResult = new CrawlerResult
        {
            RequestedPages = await _scheduler.GetAllCrawled().ToArrayAsync(CancellationToken.None),
            IsCancelled = linkedTokenSource.IsCancellationRequested,
        };

        CrawlerCompleteEvent?.Invoke(this, new CrawlerCompleteEventArgs(crawlerResult));

        return crawlerResult;
    }

    async Task RunCrawlers(IScheduler scheduler, IPageProcessor pageProcessor, IDownloader downloader, CrawlerOptions options, CancellationToken cancellationToken)
    {
        var crawlerStates = new CrawlerStates();

        var tasks = new List<Task>();
        for (var i = 0; i < options.NumberOfCrawlers; i++)
        {
            var crawlerNo = i;
            crawlerStates.Crawlers.Add(crawlerNo, true);
            tasks.Add(Task.Run(() => CrawlAsync(crawlerNo, scheduler, pageProcessor, downloader, crawlerStates, options, cancellationToken), CancellationToken.None));
        }

        await Task.WhenAll(tasks);
    }

    async Task CrawlAsync(int crawlerNo, IScheduler scheduler, IPageProcessor pageProcessor, IDownloader downloader, CrawlerStates crawlerStates, CrawlerOptions options, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Take the next URL to crawl from the scheduler.
                using var scope = await scheduler.GetQueuedItemScope(cancellationToken);

                if (scope == null)
                {
                    crawlerStates.Crawlers[crawlerNo] = false;
                    _logger.LogTrace("Crawler no {crawlerNo} is waiting for more work. There are {workerCount} other workers still working.", crawlerNo, crawlerStates.Crawlers.Count(x => x.Value));
                    await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
                    if (crawlerStates.Crawlers.Count(x => x.Value) == 0) return; //No workers are working, end the crawl
                }
                else
                {
                    crawlerStates.Crawlers[crawlerNo] = true;

                    var result = await downloader.GetAsync(scope.ToCrawl, options.DownloadOptions, cancellationToken);
                    if (result == null) throw new NullReferenceException($"Downloader did not return anything for '{scope.ToCrawl.RequestUri.AbsoluteUri}'.");

                    var responseCodeCategory = result.GetResponseCodeCategory();
                    switch (responseCodeCategory)
                    {
                        case ResponseCodeCategory.ServerError:
                        {
                            if ((options.DownloadOptions?.RetryCount ?? 0) < result.RetryCount)
                            {
                                //var crawl = new ToCrawl { RequestUri = result.RequestUri, RetryCount = result.RetryCount + 1, Parent = result.Parent };
                                //_logger.LogInformation("Crawler {crawlerNo} failed to processed {uri} with status code {statusCode}. Retry no {retryCount}.", crawlerNo, result.RequestUri, result.StatusCode, crawl.RetryCount);
                                //await _scheduler.EnqueueAsync(crawl, options.SchedulerOptions);
                                Debugger.Break();
                                throw new InvalidOperationException("Retry docment has not yet been implemented.");
                            }
                            else
                            {
                                //await scheduler.AddAsync(result.ToCrawled());
                                //PageCompleteEvent?.Invoke(this, new PageCompleteEventArgs(result));
                                //_logger.LogWarning("Crawler {crawlerNo} failed to processed {uri} with status code {statusCode}. Giving up after {retryCount} retries.", crawlerNo, result.RequestUri, result.StatusCode, result.RetryCount);
                                Debugger.Break();
                                throw new InvalidOperationException("Retry docment depleted has not yet been implemented.");
                            }

                            break;
                        }
                        case ResponseCodeCategory.Redirection:
                        case ResponseCodeCategory.Information:
                        case ResponseCodeCategory.ClientError:
                            _logger.LogWarning("Crawler {crawlerNo} failed to processed {uri} with status code {statusCode}.", crawlerNo, result.RequestUri, result.StatusCode);
                            scope.Commit(result);
                            break;
                        case ResponseCodeCategory.Success:
                            _logger.LogInformation("Crawler {crawlerNo} processed {uri} with success.", crawlerNo, result.RequestUri);
                            PageCompleteEvent?.Invoke(this, new PageCompleteEventArgs(result));
                            await foreach (var item in pageProcessor.ProcessAsync(result).WithCancellation(cancellationToken))
                            {
                                await scheduler.EnqueueAsync(item, options.SchedulerOptions);
                            }
                            scope.Commit(result);

                            break;
                        default:
                            throw new ArgumentOutOfRangeException($"Unknown response category '{responseCodeCategory}'.");
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