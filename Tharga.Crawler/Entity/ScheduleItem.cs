namespace Tharga.Crawler.Entity;

internal record ScheduleItem
{
    public required ToCrawl ToCrawl { get; init; }
    public required ScheduleItemState State { get; init; }
}