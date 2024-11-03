using System.Net;
using System.Net.Mime;
using Microsoft.Extensions.Logging;
using Tharga.Crawler.Entity;

namespace Tharga.Crawler.Downloader;

public class HttpClientDownloader : IDownloader
{
    private readonly ILogger<HttpClientDownloader> _logger;

    public HttpClientDownloader(ILogger<HttpClientDownloader> logger)
    {
        _logger = logger;
    }

    public async Task<CrawlContent> GetAsync(ToCrawl toCrawl, DownloadOptions downloadOptions, CancellationToken cancellationToken)
    {
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = false,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };

        (HttpResponseMessage Response, Uri[] Redirects) result = default;
        try
        {
            using var httpClient = new HttpClient(handler);
            if (!string.IsNullOrEmpty(downloadOptions?.UserAgent))
            {
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(downloadOptions.UserAgent);
                httpClient.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.5");
                httpClient.DefaultRequestHeaders.Referrer = new Uri("https://www.google.com");
            }
            httpClient.Timeout = downloadOptions?.Timeout ?? TimeSpan.FromSeconds(100);
            result = await GetWithRedirectsAsync(httpClient, toCrawl.RequestUri, cancellationToken);
            var content = await result.Response.Content.ReadAsByteArrayAsync(cancellationToken);

            return new CrawlContent
            {
                RequestUri = toCrawl.RequestUri,
                StatusCode = (int)result.Response.StatusCode,
                Redirects = result.Redirects,
                ContentType = new ContentType(result.Response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream"),
                Content = content,
                Parent = toCrawl.Parent,
                RetryCount = toCrawl.RetryCount,
                Message = null
            };
        }
        catch (TaskCanceledException)
        {
            return null;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);

            return new CrawlContent
            {
                RequestUri = toCrawl.RequestUri,
                StatusCode = 599,
                Redirects = result.Redirects ?? [],
                ContentType = null,
                Content = null,
                Message = e.Message,
                Parent = toCrawl.Parent,
                RetryCount = toCrawl.RetryCount
            };
        }
        finally
        {
            result.Response?.Dispose();
        }
    }

    private async Task<(HttpResponseMessage Response, Uri[] Redirects)> GetWithRedirectsAsync(HttpClient httpClient, Uri url, CancellationToken cancellationToken)
    {
        var currentUri = url;
        HttpResponseMessage response;

        var redirects = new List<Uri>();
        do
        {
            response = await httpClient.GetAsync(currentUri, cancellationToken);

            if (IsRedirect(response.StatusCode))
            {
                if (redirects.Count >= 20) throw new InvalidOperationException("Exceeded maximum number of redirects.");

                // Get the new location and add it to the redirects list
                if (response.Headers.Location != null)
                {
                    // Resolve the absolute URI in case it's a relative path
                    currentUri = new Uri(currentUri, response.Headers.Location);
                    redirects.Add(currentUri);
                }
            }
            else
            {
                // Break the loop if no more redirects are found
                break;
            }
        } while (IsRedirect(response.StatusCode));

        return (response, redirects.ToArray());
    }

    private bool IsRedirect(HttpStatusCode statusCode)
    {
        return statusCode == HttpStatusCode.MovedPermanently || // 301
               statusCode == HttpStatusCode.Found ||            // 302
               statusCode == HttpStatusCode.SeeOther ||         // 303
               statusCode == HttpStatusCode.TemporaryRedirect || // 307
               statusCode == HttpStatusCode.PermanentRedirect;   // 308
    }
}