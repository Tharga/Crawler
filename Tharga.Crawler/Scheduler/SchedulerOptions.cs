using Tharga.Crawler.Filter;

namespace Tharga.Crawler.Scheduler;

public record SchedulerOptions
{
    /// <summary>
    /// Maximum number of pages that will be placed in the queue.
    /// If the queue reaches this limit, any new uris will be dropped.
    /// </summary>
    public int? MaxQueueCount { get; init; }

    /// <summary>
    /// List of filters used to ignre adding urls to the scheduler.
    /// If the filter matches the expression, the page will not be added.
    /// The filter uses the request uri, not the response (if redirected), it is run before url replacement.
    /// </summary>
    public UrlFilter[] UrlFilters { get; init; }

    /// <summary>
    /// List of replacement expressions that makes it possible to mutate uris before they are added to the scheduler.
    /// Url replacement is run after the url filters.
    /// </summary>
    public UrlReplaceExpression[] UrlReplaceExpressions { get; init; }
}