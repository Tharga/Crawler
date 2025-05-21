namespace Tharga.Crawler.Entity;

public record ToCrawl
{
    public required Uri RequestUri { get; init; }
    public int RetryCount { get; init; }
    public Crawled Parent { get; init; }
    public int Level => Parent == null ? 0 : Parent.Level + 1;

    public static implicit operator ToCrawl(Uri uri) => new() { RequestUri = uri };
}