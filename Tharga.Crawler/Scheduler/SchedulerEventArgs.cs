namespace Tharga.Crawler.Scheduler;

public class SchedulerEventArgs : EventArgs
{
    internal SchedulerEventArgs(Action action, int queueCount, int crawlingCount, int completeCount)
    {
        Action = action;
        QueueCount = queueCount;
        CrawlingCount = crawlingCount;
        CrawledCount = completeCount;
    }

    public Action Action { get; }
    public int QueueCount { get; }
    public int CrawlingCount { get; }
    public int CrawledCount { get; }
}