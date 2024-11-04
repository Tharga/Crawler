using Tharga.Crawler.Helper;

namespace Tharga.Crawler.Entity;

public record ToCrawlScope : IDisposable
{
    private readonly Action<ToCrawl, ScheduleItemState> _release;
    private bool _released;

    internal ToCrawlScope(ToCrawl toCrawl, Action<ToCrawl, ScheduleItemState> release)
    {
        ToCrawl = toCrawl;
        _release = release;
    }

    public ToCrawl ToCrawl { get; }

    public void Dispose()
    {
        if (!_released)
        {
            _release.Invoke(ToCrawl, ScheduleItemState.Queued);
        }
    }

    public void Retry()
    {
        _release.Invoke(ToCrawl with { RetryCount = ToCrawl.RetryCount + 1 }, ScheduleItemState.Queued);
        _released = true;
    }

    public void Commit(CrawlContent crawlContent)
    {
        _release.Invoke(crawlContent.RemoveContent(), ScheduleItemState.Complete);
        _released = true;
    }
}