namespace Tharga.Crawler.Entity;

public record ToCrawl
{
    internal ToCrawl()
    {
    }

    public static ToCrawl Build(Uri requestUri, Crawled parent)
    {
        return new ToCrawl
        {
            RequestUri = requestUri,
            RetryCount = 0,
            Parent = parent
        };
    }

    public required Uri RequestUri { get; init; }
    public required int RetryCount { get; init; }
    public required Crawled Parent { get; init; }
    public int Level => Parent == null ? 0 : Parent.Level + 1;
}