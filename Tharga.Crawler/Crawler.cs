using System.Diagnostics;
using System.Net;
using Microsoft.Extensions.Logging;
using Tharga.Crawler.Downloader;
using Tharga.Crawler.Entity;
using Tharga.Crawler.Helper;
using Tharga.Crawler.PageProcessor;
using Tharga.Crawler.Scheduler;

namespace Tharga.Crawler;

public class Crawler : ICrawler
{
    private readonly IScheduler _scheduler;
    private readonly IPageProcessor _pageProcessor;
    private readonly IDownloader _downloader;
    private readonly ILogger<Crawler> _logger;

    public Crawler(IScheduler scheduler = default, IPageProcessor pageProcessor = default, IDownloader downloader = default, ILogger<Crawler> logger = default)
    {
        _scheduler = scheduler ?? new MemoryScheduler();
        _pageProcessor = pageProcessor ?? new PageProcessorBase();
        _downloader = downloader ?? new HttpClientDownloader();
        _logger = logger;
    }

    public IScheduler Scheduler => _scheduler;
    public event EventHandler<CrawlerCompleteEventArgs> CrawlerCompleteEvent;
    public event EventHandler<PageCompleteEventArgs> PageCompleteEvent;

    public async Task<CrawlerResult> StartAsync(Uri uri, CrawlerOptions options = default, CancellationToken cancellationToken = default)
    {
        return await StartAsync([uri], options, cancellationToken);
    }

    public async Task<CrawlerResult> StartAsync(Uri[] uris, CrawlerOptions options = default, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        options ??= new CrawlerOptions();
        if (options.NumberOfProcessors <= 0) throw new InvalidOperationException("Need at least one crawler.");

        using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        foreach (var uri in uris)
        {
            await _scheduler.EnqueueAsync(new ToCrawl { RequestUri = uri, RetryCount = 0, Parent = null }, options.SchedulerOptions);
        }

        //NOTE: Set a limitation in time to crawl
        if (options.MaxCrawlTime.HasValue)
        {
            var cts = linkedTokenSource;
            _ = Task.Run(async () =>
            {
                await Task.Delay(options.MaxCrawlTime.Value, cts.Token);
                _logger?.LogInformation("Crawl timeout reached after {timeoutMilliseconds} ms.", options.MaxCrawlTime.Value.TotalMicroseconds);
                await cts.CancelAsync();
            }, linkedTokenSource.Token);
        }

        await RunProcessorsAsync(_scheduler, _pageProcessor, _downloader, options, linkedTokenSource.Token);

        var pages = await _scheduler.GetAllCrawled().ToArrayAsync(CancellationToken.None);
        sw.Stop();

        var crawlerResult = new CrawlerResult
        {
            Pages = pages,
            IsCancelled = linkedTokenSource.IsCancellationRequested,
            Elapsed = sw.Elapsed
        };

        CrawlerCompleteEvent?.Invoke(this, new CrawlerCompleteEventArgs(crawlerResult));

        return crawlerResult;
    }

    async Task RunProcessorsAsync(IScheduler scheduler, IPageProcessor pageProcessor, IDownloader downloader, CrawlerOptions options, CancellationToken cancellationToken)
    {
        var crawlerStates = new CrawlerStates();

        var processorTasks = new List<Task>();
        for (var i = 0; i < options.NumberOfProcessors; i++)
        {
            var crawlerNo = i;
            crawlerStates.Crawlers.Add(crawlerNo, true);
            processorTasks.Add(Task.Run(() => RunProcessorAsync(crawlerNo, scheduler, pageProcessor, downloader, crawlerStates, options, cancellationToken), CancellationToken.None));
        }

        await Task.WhenAll(processorTasks);
    }

    async Task RunProcessorAsync(int crawlerNo, IScheduler scheduler, IPageProcessor pageProcessor, IDownloader downloader, CrawlerStates crawlerStates, CrawlerOptions options, CancellationToken cancellationToken)
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
                    _logger?.LogTrace("Crawler no {crawlerNo} is waiting for more work. There are {workerCount} other workers still working.", crawlerNo, crawlerStates.Crawlers.Count(x => x.Value));
                    await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
                    if (crawlerStates.Crawlers.Count(x => x.Value) == 0) return; //No workers are working, end the crawl
                }
                else
                {
                    crawlerStates.Crawlers[crawlerNo] = true;

                    var result = await downloader.GetAsync(scope.ToCrawl, options.DownloadOptions, cancellationToken);
                    if (result == null) throw new NullReferenceException($"Downloader did not return anything for '{scope.ToCrawl.RequestUri.AbsoluteUri}'.");

                    if (result.StatusCode == 0)
                    {
                        _logger?.LogWarning("Crawler {crawlerNo} failed to processed {uri} with status code {statusCode}. Giving up after {retryCount} retries.", crawlerNo, result.RequestUri, result.StatusCode, result.RetryCount);
                        scope.Commit(result);
                        continue;
                    }

                    try
                    {
                        var responseCodeCategory = result.GetResponseCodeCategory();
                        switch (responseCodeCategory)
                        {
                            case ResponseCodeCategory.ServerError:
                            {
                                if (result.RetryCount < (options.DownloadOptions?.RetryCount ?? 0))
                                {
                                    _logger?.LogInformation("Crawler {crawlerNo} failed to processed {uri} with status code {statusCode}. Retry no {retryCount}.", crawlerNo, result.RequestUri, result.StatusCode, scope.ToCrawl.RetryCount);
                                    scope.Retry();
                                }
                                else
                                {
                                    _logger?.LogWarning("Crawler {crawlerNo} failed to processed {uri} with status code {statusCode}. Giving up after {retryCount} retries.", crawlerNo, result.RequestUri, result.StatusCode, result.RetryCount);
                                    scope.Commit(result);
                                }

                                break;
                            }
                            case ResponseCodeCategory.Redirection:
                            case ResponseCodeCategory.Information:
                            case ResponseCodeCategory.ClientError:
                                _logger?.LogWarning("Crawler {crawlerNo} failed to processed {uri} with status code {statusCode}.", crawlerNo, result.RequestUri, (HttpStatusCode)result.StatusCode);
                                scope.Commit(result);
                                break;
                            case ResponseCodeCategory.Success:
                                _logger?.LogInformation("Crawler {crawlerNo} processed {uri} with success.", crawlerNo, result.RequestUri);
                                PageCompleteEvent?.Invoke(this, new PageCompleteEventArgs(result));
                                await foreach (var item in pageProcessor.ProcessAsync(result, options, cancellationToken))
                                {
                                    await scheduler.EnqueueAsync(item, options.SchedulerOptions);
                                }

                                scope.Commit(result);

                                break;
                            default:
                                throw new ArgumentOutOfRangeException($"Unknown response category '{responseCodeCategory}'.");
                        }
                    }
                    catch (InvalidOperationException e)
                    {
                        _logger?.LogError("Crawler {crawlerNo} failed to processed {uri} with status code {statusCode}. Giving up after {retryCount} retries.", crawlerNo, result.RequestUri, result.StatusCode, result.RetryCount);
                        scope.Commit(result);
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