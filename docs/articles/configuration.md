# Configuration

`CrawlerOptions` is the top-level knob bag passed to `StartAsync`. It groups download- and scheduler-level options.

## CrawlerOptions

| Option | Default | Description |
|---|---|---|
| `MaxCrawlTime` | No limit | Maximum total duration for the crawl |
| `NumberOfProcessors` | 3 | Number of parallel page processors |
| `DownloadOptions` | `new DownloadOptions()` | See below |
| `SchedulerOptions` | `new SchedulerOptions()` | See below |

## DownloadOptions

| Option | Default | Description |
|---|---|---|
| `RetryCount` | 3 | Number of retries for HTTP 5xx errors |
| `Timeout` | No limit | Timeout per individual page download |
| `UserAgent` | `UserAgentLibrary.Chrome` | User agent string sent with requests |

## SchedulerOptions

| Option | Default | Description |
|---|---|---|
| `MaxQueueCount` | No limit | Maximum items in the queue. New URIs are dropped when the limit is reached. |

## Full example

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

## User agent library

`UserAgentLibrary` provides built-in user agent strings so you don't have to type them out:

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

When cancelled, `StartAsync` still returns a `CrawlerResult` — pages crawled before cancellation are included and `IsCancelled` is `true`.

## CrawlerResult

The `CrawlerResult` returned from `StartAsync` exposes:

- `IsCancelled` — whether the crawl was cancelled.
- `Elapsed` — total crawl duration.
- `GetRequestedPages()` — every page that was requested.
- `GetFinalPages()` — distinct final pages (after following redirects), ordered by redirect count.

Each crawled page includes the HTTP status code, redirect chain, final URI, content type, download time, and page title.
