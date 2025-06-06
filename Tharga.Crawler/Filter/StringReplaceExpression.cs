namespace Tharga.Crawler.Filter;

public record StringReplaceExpression
{
    public string Expression { get; init; }
    public string Replacement { get; init; }
}