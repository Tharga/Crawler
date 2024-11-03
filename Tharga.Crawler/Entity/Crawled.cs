using System.Net.Mime;

namespace Tharga.Crawler.Entity;

public record Crawled : ToCrawl
{
    internal Crawled()
    {
    }

    public required int StatusCode { get; init; }
    public required Uri[] Redirects { get; init; }
    public Uri FinalUri => Redirects?.LastOrDefault() ?? RequestUri;
    public required ContentType ContentType { get; init; }
    //public TimeSpan? DownloadTime { get; init; } //TODO: Implement
    //public string Title { get; init; } //TODO: Implement
    public string Message { get; init; }
}