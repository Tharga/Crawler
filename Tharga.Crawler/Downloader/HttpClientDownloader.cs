using System.Net.Mime;
using Microsoft.Extensions.Logging;

namespace Tharga.Crawler.Downloader;

public class HttpClientDownloader : IDownloader
{
    private readonly ILogger<HttpClientDownloader> _logger;

    public HttpClientDownloader(ILogger<HttpClientDownloader> logger)
    {
        _logger = logger;
    }

    public async Task<CrawlContent> GetAsync(ToCrawl toCrawl, CancellationToken cancellationToken)
    {
        //TODO: Add polly
        //TODO: Handle redirects
        //TODO: Add retry-feature

        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
        };

        try
        {
            using var client = new HttpClient(handler);
            using var result = await client.GetAsync(toCrawl.RequestUri, cancellationToken);
            var content = await result.Content.ReadAsByteArrayAsync(cancellationToken);

            return new CrawlContent
            {
                RequestUri = toCrawl.RequestUri,
                StatusCode = result.StatusCode,
                Redirects = [], //TODO: Implement
                ContentType = new ContentType(result.Content.Headers.ContentType?.MediaType ?? "application/octet-stream"),
                Content = content,
            };
        }
        catch (TaskCanceledException)
        {
            return null;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);

            //TODO: Add another error message to this item.
            return new CrawlContent
            {
                RequestUri = toCrawl.RequestUri,
                StatusCode = null,
                Redirects = [], //TODO: Implement
                ContentType = null,
                Content = null,
            };
        }
    }
}