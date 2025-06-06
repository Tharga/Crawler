using Tharga.Crawler.Filter;

namespace Tharga.Crawler.Scheduler;

public record SchedulerOptions
{
    public int? MaxQueueCount { get; init; }
    public StringFilter[] UrlFilters { get; init; }
    public StringReplaceExpression[] UrlReplaceExpressions { get; init; }

}