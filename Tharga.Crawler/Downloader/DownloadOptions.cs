namespace Tharga.Crawler.Downloader;

public record DownloadOptions
{
    public TimeSpan? Timeout { get; init; }
    public int RetryCount { get; init; } = 3;
}