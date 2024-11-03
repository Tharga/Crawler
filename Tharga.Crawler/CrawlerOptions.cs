using Tharga.Crawler.Downloader;
using Tharga.Crawler.Scheduler;

namespace Tharga.Crawler;

public record CrawlerOptions
{
    public TimeSpan? MaxCrawlTime { get; init; }
    public int NumberOfCrawlers { get; init; } = 3;
    public DownloadOptions DownloadOptions { get; init; }
    public SchedulerOptions SchedulerOptions { get; init; }
}