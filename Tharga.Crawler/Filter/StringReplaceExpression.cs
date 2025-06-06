namespace Tharga.Crawler.Filter;

public record StringReplaceExpression
{
    public string Pattern { get; init; }
    public string Replacement { get; init; }
}