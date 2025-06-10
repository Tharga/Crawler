using Tharga.Crawler.Downloader;
using Tharga.Crawler.Scheduler;

namespace Tharga.Crawler;

public record CrawlerOptions
{
    public TimeSpan? MaxCrawlTime { get; init; }
    public int NumberOfProcessors { get; init; } = 3;
    public DownloadOptions DownloadOptions { get; init; } = new();
    public SchedulerOptions SchedulerOptions { get; init; }
}