# Tharga Crawler

[![NuGet](https://img.shields.io/nuget/v/Tharga.Crawler)](https://www.nuget.org/packages/Tharga.Crawler)
![Downloads](https://img.shields.io/nuget/dt/Tharga.Crawler)

A customizable web crawler framework for .NET. Drop it into your project and start crawling in two lines of code.

```csharp
var crawler = new Crawler();
var result = await crawler.StartAsync(new Uri("https://example.com/"));
```

## Why Tharga Crawler?

- **Minimal setup** — works out of the box with sensible defaults
- **Parallel processing** — configurable number of concurrent page processors
- **Breadth-first crawling** — pages closest to the root are crawled first
- **Redirect tracking** — follows and records full redirect chains
- **Automatic retries** — retries failed requests on server errors (5xx)
- **Event-driven** — subscribe to page-level and crawl-level events for real-time progress
- **Fully pluggable** — replace the downloader, page processor, scheduler, or URI service with your own implementations
- **DI-ready** — integrates with `Microsoft.Extensions.DependencyInjection` via a single `AddCrawler()` call
- **Multi-target** — supports .NET 8, .NET 9, and .NET 10

## Get Started

```bash
dotnet add package Tharga.Crawler
```

For full documentation, examples, and customization guides, see the [GitHub repository](https://github.com/Tharga/Crawler).
