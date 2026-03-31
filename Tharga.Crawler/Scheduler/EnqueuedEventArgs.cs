using Tharga.Crawler.Entity;

namespace Tharga.Crawler.Scheduler;

public class EnqueuedEventArgs : EventArgs
{
    internal EnqueuedEventArgs(ToCrawl[] items)
    {
        Items = items;
    }

    public ToCrawl[] Items { get; }
}