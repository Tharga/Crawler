namespace Tharga.Crawler;

public class CrawlerCompleteEventArgs : EventArgs
{
    public CrawlerCompleteEventArgs(CrawlerResult crawlerResult)
    {
        CrawlerResult = crawlerResult;
    }

    public CrawlerResult CrawlerResult { get; }
}