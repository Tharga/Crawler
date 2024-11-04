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
    public required TimeSpan? DownloadTime { get; init; }
    public required string Title { get; init; }
    public required string Message { get; init; }
}