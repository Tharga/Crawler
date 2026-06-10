# Custom services

The crawler is four pluggable components. Each lives behind an interface — implement the interface, register your implementation, and the crawler will use it.

## IDownloader

Handles HTTP fetching. The built-in `HttpClientDownloader` uses `HttpClient` and follows 301/302/303/307/308 redirects. Replace it if you need a headless browser (Playwright, Puppeteer) to evaluate JavaScript, or a custom transport.

```csharp
public class MyDownloader : IDownloader
{
    public Task<CrawlContent> GetAsync(ToCrawl toCrawl, DownloadOptions options, CancellationToken ct)
    {
        // ... render via a headless browser, return CrawlContent
    }
}
```

## IPageProcessor

Decides which URIs to enqueue from a downloaded page. The built-in `BasicPageProcessor` parses HTML with HtmlAgilityPack and emits all `<a href="...">` targets that resolve to the same root domain. Replace it to process non-HTML content (sitemaps, JSON, RSS) or to apply custom link-extraction rules.

```csharp
public class SitemapPageProcessor : IPageProcessor
{
    public async IAsyncEnumerable<ToCrawl> ProcessAsync(
        CrawlContent page, CrawlerOptions options,
        [EnumeratorCancellation] CancellationToken ct)
    {
        // ... yield ToCrawl per URL in the sitemap
    }
}
```

## IScheduler

Manages the work queue and the set of crawled pages. The built-in `MemoryScheduler` is an in-memory breadth-first queue with retry tracking. Replace it with a database-backed implementation for resumable crawls that survive process restarts.

```csharp
public class RedisScheduler : IScheduler
{
    public Task EnqueueAsync(ToCrawl toCrawl, SchedulerOptions options) { /* ... */ }
    public Task EnqueueAsync(ToCrawl[] items, SchedulerOptions options) { /* ... */ }
    public Task<ToCrawlScope> GetQueuedItemScope(CancellationToken ct) { /* ... */ }
    public IAsyncEnumerable<ToCrawl> GetQueued() { /* ... */ }
    public IAsyncEnumerable<Crawled> GetAllCrawled() { /* ... */ }

    public event EventHandler<SchedulerEventArgs> SchedulerEvent;
    public event EventHandler<EnqueuedEventArgs> EnqueuedEvent;
}
```

## IUriService

Decides whether a discovered URI gets enqueued, and optionally rewrites it. The default `UriService` allows only same-domain, HTTP/HTTPS URIs. Override `ShouldEnqueueAsync` to filter (e.g. skip `/api`, drop URLs matching a pattern) or `MutateUriAsync` to normalize (e.g. strip query parameters, lowercase the host).

```csharp
public class MyUriService : IUriService
{
    public Task<bool> ShouldEnqueueAsync(Uri parentUri, Uri uri)
    {
        if (uri.AbsolutePath.StartsWith("/api")) return Task.FromResult(false);
        return Task.FromResult(true);
    }

    public Task<Uri> MutateUriAsync(Uri uri)
    {
        var builder = new UriBuilder(uri) { Query = string.Empty };
        return Task.FromResult(builder.Uri);
    }
}
```

## Registration

Pass `CrawlerRegistrationOptions` to `AddCrawler` to wire up your replacements:

```csharp
services.AddCrawler(options =>
{
    options.Downloader = provider => new MyDownloader();
    options.PageProcessor = provider => new SitemapPageProcessor();
    options.Scheduler = provider => new RedisScheduler();
    options.UriService = provider => new MyUriService();
});
```

You can also pass instances per-crawl through `ICrawlerProvider.GetCrawlerInstance(...)` when you need different behavior for different crawls.
