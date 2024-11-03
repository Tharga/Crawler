namespace Tharga.Crawler.Scheduler;

public record SchedulerOptions
{
    public int? MaxQueueCount { get; init; }
}