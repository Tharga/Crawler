# Tharga Crawler

[![NuGet](https://img.shields.io/nuget/v/Tharga.Crawler)](https://www.nuget.org/packages/Tharga.Crawler)
![Nuget](https://img.shields.io/nuget/dt/Tharga.Crawler)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![GitHub repo Issues](https://img.shields.io/github/issues/Tharga/Crawler?style=flat&logo=github&logoColor=red&label=Issues)](https://github.com/Tharga/Crawler/issues?q=is%3Aopen)

A customizable web crawler framework for .NET written in C#. Supports .NET 8, .NET 9, and .NET 10.

## Installation

```bash
dotnet add package Tharga.Crawler
```

## Quick Start

The simplest way to crawl a site:

```csharp
var crawler = new Crawler();
var result = await crawler.StartAsync(new Uri("https://example.com/"));
```

You can also crawl multiple starting points at once:

```csharp
var crawler = new Crawler();
var uris = new[] { new Uri("https://example.com/"), new Uri("https://example.com/blog") };
var result = await crawler.StartAsync(uris);
```

## Events

There are three events available for monitoring crawl progress:

### CrawlerCompleteEvent

Fires when the entire crawl has finished. Useful for background crawling without awaiting the result.

```csharp
var crawler = new Crawler();
crawler.CrawlerCompleteEvent += (s, e) =>
{
    var result = e.CrawlerResult;
    Console.WriteLine($"Completed with {result.GetRequestedPages().Count()} requests " +
                      $"and {result.GetFinalPages().Count()} final pages.");
};
await crawler.StartAsync(new Uri("https://example.com/"));
```

### PageCompleteEvent

Fires each time a page is successfully downloaded (HTTP 2xx).

```csharp
crawler.PageCompleteEvent += (s, e) =>
{
    Console.WriteLine($"Downloaded: {e.CrawlContent.FinalUri} ({e.CrawlContent.StatusCode})");
};
```

### PageFailedEvent

Fires when a page download fails (non-2xx status or exception).

```csharp
crawler.PageFailedEvent += (s, e) =>
{
    Console.WriteLine($"Failed: {e.CrawlContent.RequestUri} - {e.CrawlContent.StatusCode}");
};
```

## Dependency Injection

Register the crawler in your service collection using `AddCrawler()`. All components are registered as transient, so multiple parallel crawler instances are supported.

```csharp
services.AddCrawler();
```

Then inject `ICrawler` into your services:

```csharp
public class MyService
{
    private readonly ICrawler _crawler;

    public MyService(ICrawler crawler)
    {
        _crawler = crawler;
    }

    public async Task Crawl(Uri uri)
    {
        var result = await _crawler.StartAsync(uri);
    }
}
```

### ICrawlerProvider

For scenarios where you need to create crawler instances with custom components at runtime, inject `ICrawlerProvider`:

```csharp
public class MyService
{
    private readonly ICrawlerProvider _crawlerProvider;

    public MyService(ICrawlerProvider crawlerProvider)
    {
        _crawlerProvider = crawlerProvider;
    }

    public async Task Crawl(Uri uri)
    {
        var crawler = _crawlerProvider.GetCrawlerInstance(scheduler: myCustomScheduler);
        var result = await crawler.StartAsync(uri);
    }
}
```

### Overriding Default Components via DI

You can replace any built-in component by passing `CrawlerRegistrationOptions`:

```csharp
services.AddCrawler(options =>
{
    options.Scheduler = provider => new MyCustomScheduler();
    options.Downloader = provider => new MyCustomDownloader();
});
```

## Options

There are several [options](Tharga.Crawler/CrawlerOptions.cs) that can be configured for each crawl.

| Option | Default | Description |
|---|---|---|
| `MaxCrawlTime` | No limit | Maximum total duration for the crawl |
| `NumberOfProcessors` | 3 | Number of parallel page processors |

### DownloadOptions

| Option | Default | Description |
|---|---|---|
| `RetryCount` | 3 | Number of retries for HTTP 5xx errors |
| `Timeout` | No limit | Timeout per individual page download |
| `UserAgent` | `UserAgentLibrary.Chrome` | User agent string sent with requests |

### SchedulerOptions

| Option | Default | Description |
|---|---|---|
| `MaxQueueCount` | No limit | Maximum items in the queue. New URIs are dropped when the limit is reached |

### Example with Options

```csharp
var crawler = new Crawler();
var options = new CrawlerOptions
{
    MaxCrawlTime = TimeSpan.FromMinutes(10),
    NumberOfProcessors = 5,
    DownloadOptions = new DownloadOptions
    {
        RetryCount = 3,
        Timeout = TimeSpan.FromSeconds(30),
        UserAgent = UserAgentLibrary.Chrome
    },
    SchedulerOptions = new SchedulerOptions
    {
        MaxQueueCount = 1000
    }
};
var result = await crawler.StartAsync(new Uri("https://example.com/"), options);
```

### User Agent Library

The `UserAgentLibrary` class provides built-in user agent strings:

- `UserAgentLibrary.Chrome` (default)
- `UserAgentLibrary.Firefox`
- `UserAgentLibrary.Edge`
- `UserAgentLibrary.Googlebot`
- `UserAgentLibrary.Bingbot`
- `UserAgentLibrary.DuckDuckBot`

## Cancellation

All `StartAsync` overloads accept a `CancellationToken`:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
var result = await crawler.StartAsync(new Uri("https://example.com/"), cancellationToken: cts.Token);
Console.WriteLine($"Cancelled: {result.IsCancelled}, Elapsed: {result.Elapsed}");
```

## Crawl Results

The `CrawlerResult` returned from `StartAsync` provides:

- `IsCancelled` — whether the crawl was cancelled
- `Elapsed` — total crawl duration
- `GetRequestedPages()` — all pages that were requested
- `GetFinalPages()` — distinct final pages (after following redirects), ordered by redirect count

Each crawled page includes the HTTP status code, redirect chain, final URI, content type, download time, and page title.

## Engine Components

The crawler is built from four pluggable components. The `Crawler` orchestrates the overall process, delegating to the `IDownloader`, `IScheduler`, and `IPageProcessor`.

![Process Diagram](Resources/Tharga.Crawler.Process.svg)

## Built-in Components

### HttpClientDownloader

Downloads page content using `HttpClient`. Handles HTTP redirects (301, 302, 303, 307, 308) automatically, tracking the full redirect chain. Extracts the page `<title>` from HTML content.

### BasicPageProcessor

Processes downloaded HTML to extract links. Uses [HtmlAgilityPack](https://html-agility-pack.net/) to parse the DOM and find all `<a href="...">` elements. Resolves relative URLs and stays within the original domain.

### MemoryScheduler

An in-memory queue that uses a breadth-first (shallow) crawl strategy — pages closest to the starting URI are crawled first. Handles retry logic and uses `IUriService` for URI filtering and mutation.

## Customization

All major components can be replaced by implementing the corresponding interface and registering your implementation via DI.

### IDownloader

Handles downloading page content. Override this to use a headless browser (e.g., Playwright, Puppeteer) instead of `HttpClient`.

### IPageProcessor

Controls how HTML is parsed to extract links. Override this to change link extraction logic or to process non-HTML content.

### IScheduler

Manages the crawl queue and tracks what has been crawled. Override this to persist the queue to a database for resumable crawls.

### IUriService

Provides URI filtering and mutation. Called by the scheduler before enqueuing a URI.

- `ShouldEnqueueAsync(Uri parentUri, Uri uri)` — return `false` to skip a URI
- `MutateUriAsync(Uri uri)` — transform a URI before it is enqueued (e.g., strip query parameters)

By default, the crawler stays on the same domain as the starting URI.