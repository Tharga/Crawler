namespace Tharga.Crawler;

public record ToCrawl
{
    public required Uri RequestUri { get; init; }
}