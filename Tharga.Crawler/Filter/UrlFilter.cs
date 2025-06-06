namespace Tharga.Crawler.Filter;

public record UrlFilter
{
    /// <summary>
    /// Expression as text or regex.
    /// </summary>
    public required string Expression { get; init; }

    public required FilterOperation Operation { get; init; }
}