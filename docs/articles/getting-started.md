# Getting started

## Install

```
dotnet add package Tharga.Crawler
```

Tharga.Crawler targets **.NET 8, .NET 9, and .NET 10**.

## Namespace

The entry-point types live under `Tharga.Crawler`:

```csharp
using Tharga.Crawler;
```

## Minimal example

```csharp
var crawler = new Crawler();
var result = await crawler.StartAsync(new Uri("https://example.com/"));
```

The default `Crawler` constructor wires up an `HttpClientDownloader`, a `BasicPageProcessor`, and a `MemoryScheduler` — enough to crawl a public site without any configuration.

## Multiple starting points

```csharp
var crawler = new Crawler();
var uris = new[] { new Uri("https://example.com/"), new Uri("https://example.com/blog") };
var result = await crawler.StartAsync(uris);
```

## Events

Three events let you observe progress without awaiting the final result:

```csharp
crawler.CrawlerCompleteEvent += (s, e) =>
    Console.WriteLine($"Done: {e.CrawlerResult.GetFinalPages().Count()} pages.");

crawler.PageCompleteEvent += (s, e) =>
    Console.WriteLine($"Downloaded: {e.CrawlContent.FinalUri} ({e.CrawlContent.StatusCode})");

crawler.PageFailedEvent += (s, e) =>
    Console.WriteLine($"Failed: {e.CrawlContent.RequestUri} - {e.CrawlContent.StatusCode}");
```

`PageCompleteEvent` fires on HTTP 2xx responses; `PageFailedEvent` fires on non-2xx or exceptions.

## Dependency injection

Register the crawler in your service collection:

```csharp
services.AddCrawler();
```

All components are registered as transient, so multiple parallel crawler instances are supported. Inject `ICrawler` where you need one:

```csharp
public class MyService(ICrawler crawler)
{
    public Task<CrawlerResult> Crawl(Uri uri) => crawler.StartAsync(uri);
}
```

For scenarios where you need to construct a crawler with custom components at runtime, inject `ICrawlerProvider`:

```csharp
public class MyService(ICrawlerProvider crawlerProvider)
{
    public Task<CrawlerResult> Crawl(Uri uri, IScheduler myScheduler)
    {
        var crawler = crawlerProvider.GetCrawlerInstance(scheduler: myScheduler);
        return crawler.StartAsync(uri);
    }
}
```

You can also replace any default component at registration time:

```csharp
services.AddCrawler(options =>
{
    options.Scheduler = provider => new MyCustomScheduler();
    options.Downloader = provider => new MyCustomDownloader();
});
```

## Next steps

- [Configuration](configuration.md) — `CrawlerOptions`, `DownloadOptions`, `SchedulerOptions`, cancellation, and result shape.
- [Custom services](custom-services.md) — replace any of the four pluggable components.
