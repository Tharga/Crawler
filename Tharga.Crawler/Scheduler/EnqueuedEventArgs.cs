using Tharga.Crawler.Entity;

namespace Tharga.Crawler.Scheduler;

public class EnqueuedEventArgs : EventArgs
{
    internal EnqueuedEventArgs(ToCrawl toCrawl)
    {
        ToCrawl = toCrawl;
    }

    public ToCrawl ToCrawl { get; }
}