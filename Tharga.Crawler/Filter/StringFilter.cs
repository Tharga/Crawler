namespace Tharga.Crawler.Filter;

public record StringFilter
{
    public string Expression { get; init; }
    public FilterOperation Operation { get; init; }
}