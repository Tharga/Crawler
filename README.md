# Tharga Crawler
[![NuGet](https://img.shields.io/nuget/v/Tharga.Crawler)](https://www.nuget.org/packages/Tharga.Crawler)
![Nuget](https://img.shields.io/nuget/dt/Tharga.Crawler)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![GitHub repo Issues](https://img.shields.io/github/issues/Tharga/Crawler?style=flat&logo=github&logoColor=red&label=Issues)](https://github.com/Tharga/Crawler/issues?q=is%3Aopen)

Customizable crawler for .NET written in C#.

## Get started
This is the most basic simple 
```
var crawler = new Crawler();
var result = await crawler.StartAsync(new Uri("https://thargelion.se/"));
```

## Background
There are two options for crawling, either by awaiting the `StartAsync` method (like in the example above) or by listening for the event `CrawlerCompleteEvent`.

Crawler that runs in the bacground and fires an event on completion.
```
var crawler = new Crawler();
crawler.CrawlerCompleteEvent += (s, e) => { Console.WriteLine($"Completed with {e.CrawlerResult.GetRequestedPages().Count()} page requests and {e.CrawlerResult.GetFinalPages().Count()} final pages collected."); };
crawler.StartAsync(new Uri("https://thargelion.se/"));
```

## IOC
To use the .NET IOC, register crawler in your service collection by calling
`services.RegisterCrawler();`. This will register the `ICrawler` so it can be injected to your services. The registration is transient so there can be several parallel instances of crawlers.

```
public class MyService
{
    private readonly ICrawler _crawler;

    public MyService(ICrawler crawler)
    {
        _crawler = crawler;
    }

    public async Task Crawl(Uri uri)
    {
        await _crawler.StartAsync(uri);
    }
}
```

## Options
There are a number of [options](Tharga.Crawler\CrawlerOptions.cs) that can be set for the crawl.

### Example with options
```
var crawler = new Crawler();
var options = new CrawlerOptions
{
    MaxCrawlTime = TimeSpan.FromMinutes(10),    //Limit the total crawl time to 10 minutes.
    NumberOfCrawlers = 5,                       //Run 5 parallel crawlers. (Default is 3)
    DownloadOptions = new DownloadOptions
    {
        RetryCount = 3,                         //If a download fails with 5xx, retry 3 times. (Default is 3)
        Timeout = TimeSpan.FromSeconds(30),     //Time out each page crawl after 30 seconds. (Default is 100 seconds)
        UserAgent = UserAgentLibrary.Chrome     //Specify a user agent for the httlClient. (Default is none)
    },
    SchedulerOptions = new SchedulerOptions
    {
        MaxQueueCount = null                    //Number of maximum items to be queued. (Default is no limit)
    }
};
var result = await crawler.StartAsync(new Uri("https://thargelion.se/"), options);```
```

## Engine Components
There are four main components. The `Crawler` handles the overall process of the crawl.
It uses the `IDownloader` to download pages, the `IScheduler` to handle the queue and the result of pages and the `IPageProcessor` to handle the finding of links and crawling rules.

## Customize behaviour
This version is not yet fully customizable.
The currently built in components that are used are..
- `MemoryScheduler`, that uses memory for queue and result.
- `PageProcessorBase`. It stays on the domain on the provided uri.
- `HttpClientDownloader`. It uses a regular ´HttpClient´ to download content.

## Planned
- Persistable Scheduler using MongoDB, so that crawls can be resumed.
- A page processor that uses injectable rules and filters.
- Chromium Downloader to support SPA sites.
