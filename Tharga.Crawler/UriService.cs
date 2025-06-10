using Microsoft.Extensions.Logging;

namespace Tharga.Crawler;

public class UriService : IUriService
{
    public UriService(ILogger<UriService> logger = default)
    {
    }

    public virtual Task<bool> ShouldIncludeAsync(Uri uri)
    {
        return Task.FromResult(true);
    }

    public virtual Task<Uri> MutateUriAsync(Uri uri)
    {
        return Task.FromResult(uri);
    }
}