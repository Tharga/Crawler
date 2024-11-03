namespace Tharga.Crawler.Downloader;

public record DownloadOptions
{
    public TimeSpan? Timeout { get; init; }
    public int RetryCount { get; init; } = 3;
    public string UserAgent { get; init; } = UserAgentLibrary.Chrome;
}