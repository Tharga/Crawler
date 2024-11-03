namespace Tharga.Crawler.Entity;

public record ToCrawl
{
    internal ToCrawl()
    {
    }

    public required Uri RequestUri { get; init; }
    public required int RetryCount { get; init; }
    public required Crawled Parent { get; init; }
    public int Level => Parent == null ? 0 : Parent.Level + 1;
}
