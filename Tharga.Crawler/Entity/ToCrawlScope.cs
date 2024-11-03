using Tharga.Crawler.Helper;

namespace Tharga.Crawler.Entity;

public record ToCrawlScope : IDisposable
{
    private readonly Action<ToCrawl> _release;
    private bool _released;

    internal ToCrawlScope(ToCrawl toCrawl, Action<ToCrawl> release)
    {
        ToCrawl = toCrawl;
        _release = release;
    }

    public ToCrawl ToCrawl { get; }

    public void Dispose()
    {
        if (!_released)
        {
            _release.Invoke(ToCrawl);
        }
    }

    public void Commit(CrawlContent crawlContent)
    {
        _release.Invoke(crawlContent.RemoveContent());
        _released = true;
    }
}