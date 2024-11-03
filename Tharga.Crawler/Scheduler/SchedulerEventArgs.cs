namespace Tharga.Crawler.Scheduler;

public class SchedulerEventArgs : EventArgs
{
    public SchedulerEventArgs(int queueCount, int crawlingCount, int crawledCount)
    {
        QueueCount = queueCount;
        CrawlingCount = crawlingCount;
        CrawledCount = crawledCount;
    }

    public int QueueCount { get; }
    public int CrawlingCount { get; }
    public int CrawledCount { get; }
}