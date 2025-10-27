using Microsoft.Extensions.Logging;
using Tharga.Crawler.Helper;

namespace Tharga.Crawler;

public class UriService : IUriService
{
    private readonly ILogger<UriService> _logger;

    public UriService(ILogger<UriService> logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// By default, this method checks for the same domain. It alsow skips non http/https schema like mailto-links.
    /// </summary>
    /// <param name="parentUri"></param>
    /// <param name="uri"></param>
    /// <returns></returns>
    public virtual Task<bool> ShouldEnqueueAsync(Uri parentUri, Uri uri)
    {
        if (!uri.Scheme.StartsWith("http"))
        {
            _logger?.LogTrace("Skipping {uri} because not scheme http or https.", uri);
            return Task.FromResult(false);
        }

        if (parentUri == null)
        {
            _logger?.LogTrace("Including {uri} because there is no parent.", uri);
            return Task.FromResult(true);
        }

        if (!uri.HaveSameDomain(parentUri))
        {
            _logger?.LogTrace("Skipping {uri} because not in domain {domain}.", uri, parentUri.GetDomain());
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    /// <summary>
    /// By default, this method does not change the uri.
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    public virtual Task<Uri> MutateUriAsync(Uri uri)
    {
        return Task.FromResult(uri);
    }
}