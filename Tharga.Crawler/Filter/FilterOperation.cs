namespace Tharga.Crawler.Filter;

public enum FilterOperation
{
    Exclude,
    Include
}

public record StringFilter
{
    public string Expression { get; init; }
    public FilterOperation Operation { get; init; }
}