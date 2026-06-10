---
_layout: landing
---

# Tharga.Crawler

A customizable web crawler framework for .NET, written in C#. Four pluggable components — downloader, page processor, scheduler, URI service — let you crawl a site with one line or replace any piece for headless browsers, persistent queues, custom link extraction, or non-HTML content. Targets **.NET 8, .NET 9, and .NET 10**.

## Package

| Package | What it does |
|---|---|
| [Tharga.Crawler](https://www.nuget.org/packages/Tharga.Crawler) | `Crawler` + `ICrawler` + `ICrawlerProvider`, the built-in `HttpClientDownloader` / `BasicPageProcessor` / `MemoryScheduler`, plus interfaces (`IDownloader`, `IPageProcessor`, `IScheduler`, `IUriService`) for replacing any component. |

## Quick start

```
dotnet add package Tharga.Crawler
```

```csharp
var crawler = new Crawler();
var result = await crawler.StartAsync(new Uri("https://example.com/"));
```

See [Getting started](articles/getting-started.md) for the full setup walkthrough.

## What's in the box

- **`Crawler`** — orchestrates the crawl, runs page processors in parallel, surfaces events (`CrawlerCompleteEvent`, `PageCompleteEvent`, `PageFailedEvent`). See [Getting started](articles/getting-started.md).
- **`HttpClientDownloader`** — built-in `IDownloader` using `HttpClient`. Follows 301/302/303/307/308 redirects and tracks the full redirect chain. Configurable via `DownloadOptions` (retry, timeout, user agent). See [Configuration](articles/configuration.md#downloadoptions).
- **`BasicPageProcessor`** — built-in `IPageProcessor`. Uses [HtmlAgilityPack](https://html-agility-pack.net/) to extract `<a href="...">` links and stays within the original domain.
- **`MemoryScheduler`** — built-in `IScheduler`. In-memory breadth-first queue with retry tracking. Configurable via `SchedulerOptions` (max queue size). See [Configuration](articles/configuration.md#scheduleroptions).
- **`UriService`** — default `IUriService` that enforces same-domain crawls. Override `ShouldEnqueueAsync` / `MutateUriAsync` to filter or rewrite URIs. See [Custom services](articles/custom-services.md#iuriservice).
- **Pluggable everything** — every component is behind an interface. Swap `IDownloader` for a headless browser, `IScheduler` for a database-backed queue, `IPageProcessor` for non-HTML content. See [Custom services](articles/custom-services.md).

## Repo

[github.com/Tharga/Crawler](https://github.com/Tharga/Crawler) — source, issues, releases.
