namespace Tharga.Crawler.Helper;

internal record CrawlerStates
{
    public readonly Dictionary<int, bool> Crawlers = new();
}