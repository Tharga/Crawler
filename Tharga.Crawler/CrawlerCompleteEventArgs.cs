namespace Tharga.Crawler;

public class CrawlerCompleteEventArgs : EventArgs
{
    internal CrawlerCompleteEventArgs(CrawlerResult crawlerResult)
    {
        CrawlerResult = crawlerResult;
    }

    public CrawlerResult CrawlerResult { get; }
}