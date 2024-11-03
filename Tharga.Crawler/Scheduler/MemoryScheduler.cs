using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Tharga.Crawler.Helper;

namespace Tharga.Crawler.Scheduler;

public class MemoryScheduler : IScheduler
{
    private readonly ILogger<MemoryScheduler> _logger;
    private readonly ConcurrentDictionary<Uri, ToCrawl> _queue = new();
    private readonly ConcurrentDictionary<Uri, ToCrawl> _crawling = new();
    private readonly ConcurrentDictionary<Uri, Crawled> _crawled = new();

    public MemoryScheduler(ILogger<MemoryScheduler> logger)
    {
        _logger = logger;
    }

    public event EventHandler<SchedulerEventArgs> SchedulerEvent;

    public Task<ToCrawl> TakeNextToCrawlAsync(CancellationToken cancellationToken)
    {
        var toTake = _queue.FirstOrDefault();
        if (toTake.Key == null) return Task.FromResult<ToCrawl>(null);
        if (_queue.TryRemove(toTake.Key, out var item))
        {
            if (!_crawling.TryAdd(item.RequestUri, item))
            {
                _logger.LogWarning("Another thread already took this one, take another.");
                Debugger.Break();
            }
            SchedulerEvent?.Invoke(this, new SchedulerEventArgs(_queue.Count, _crawling.Count, _crawled.Count));
            return Task.FromResult(item);
        }
        return Task.FromResult<ToCrawl>(null);
    }

    public Task AddAsync(Crawled result)
    {
        if (result is CrawlContent) throw new InvalidOperationException($"Adding result of type '{nameof(CrawlContent)}' is not allowed since it will take too much memory. Use the '{nameof(CrawledConverter.ToCrawled)}' method to convert.");

        if (result.FinalUri != result.RequestUri)
        {
            //TODO: Assure that the final and request uri belongs to the same root-domain.
            //TODO: Add to a block-list so that the request domain is not crawled again.
            Debugger.Break();
            throw new NotImplementedException();
        }

        if (_crawled.TryAdd(result.FinalUri, result))
        {
            //_logger.LogInformation($"Completed {result.FinalUri.AbsoluteUri}.");
            if (_queue.TryRemove(result.FinalUri, out _))
            {
                //Another crawler added an url that is now completed, and it has been removed.
            }
        }
        else
        {
            //TODO: How do we avoid this?
            _logger.LogWarning("Already completed '{uri}'.", result.FinalUri.AbsoluteUri);
        }

        if (!_crawling.TryRemove(result.RequestUri, out _))
        {
        }

        SchedulerEvent?.Invoke(this, new SchedulerEventArgs(_queue.Count, _crawling.Count, _crawled.Count));

        return Task.CompletedTask;
    }

    public Task Enqueue(ToCrawl toCrawl)
    {
        if (_crawled.ContainsKey(toCrawl.RequestUri)) return Task.CompletedTask;
        if (_crawling.ContainsKey(toCrawl.RequestUri)) return Task.CompletedTask;

        _queue.TryAdd(toCrawl.RequestUri, toCrawl);
        SchedulerEvent?.Invoke(this, new SchedulerEventArgs(_queue.Count, _crawling.Count, _crawled.Count));
        return Task.CompletedTask;
    }

    public IAsyncEnumerable<Crawled> GetAllCrawled()
    {
        return _crawled.Values.ToAsyncEnumerable();
    }
}