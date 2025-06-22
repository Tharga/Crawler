namespace Tharga.Crawler.Scheduler;

public record SchedulerOptions
{
    /// <summary>
    /// Maximum number of pages that will be placed in the queue.
    /// If the queue reaches this limit, any new uris will be dropped.
    /// </summary>
    public int? MaxQueueCount { get; init; }
}