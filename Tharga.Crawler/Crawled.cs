using System.Net;
using System.Net.Mime;

namespace Tharga.Crawler;

public record Crawled : ToCrawl
{
    public required HttpStatusCode? StatusCode { get; init; }
    public required Uri[] Redirects { get; init; }
    public Uri FinalUri => Redirects?.LastOrDefault() ?? RequestUri;
    public required ContentType ContentType { get; init; }
    //public TimeSpan? DownloadTime { get; init; } //TODO: Implement

    //public required Uri Parent { get; init; } //TODO: Implement
    //public string Title { get; init; } //TODO: Implement
}