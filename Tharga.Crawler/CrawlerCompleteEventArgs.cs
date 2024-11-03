namespace Tharga.Crawler;

public class CrawlerCompleteEventArgs : EventArgs
{
    public CrawlerCompleteEventArgs(bool isCancelled)
    {
        IsCancelled = isCancelled;
    }

    public bool IsCancelled { get; }
}