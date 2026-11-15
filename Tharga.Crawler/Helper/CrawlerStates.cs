using System.Collections.Concurrent;

namespace Tharga.Crawler.Helper;

internal record CrawlerStates
{
    //NOTE: Written by every worker at the same time, so it must be thread-safe.
    public readonly ConcurrentDictionary<int, bool> Crawlers = new();
}
