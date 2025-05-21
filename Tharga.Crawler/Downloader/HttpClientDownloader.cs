using System.Diagnostics;
using System.Net;
using System.Net.Mime;
using System.Web;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Tharga.Crawler.Entity;
using Tharga.Crawler.Helper;

namespace Tharga.Crawler.Downloader;

public class HttpClientDownloader : IDownloader
{
    private readonly ILogger<HttpClientDownloader> _logger;

    public HttpClientDownloader(ILogger<HttpClientDownloader> logger = default)
    {
        _logger = logger;
    }

    public virtual async Task<CrawlContent> GetAsync(ToCrawl toCrawl, DownloadOptions downloadOptions, CancellationToken cancellationToken)
    {
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = false,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };

        (HttpResponseMessage Response, Uri[] Redirects) result = default;
        var sw = new Stopwatch();
        try
        {
            using var httpClient = new HttpClient(handler);
            if (!string.IsNullOrEmpty(downloadOptions?.UserAgent))
            {
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(downloadOptions.UserAgent);
                //httpClient.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                //httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.5");
                //httpClient.DefaultRequestHeaders.Referrer = new Uri("https://www.google.com");
            }
            httpClient.Timeout = downloadOptions?.Timeout ?? TimeSpan.FromSeconds(100);

            sw.Start();

            result = await GetWithRedirectsAsync(httpClient, toCrawl.RequestUri, cancellationToken);
            var content = await result.Response.Content.ReadAsByteArrayAsync(cancellationToken);

            sw.Stop();

            var contentType = new ContentType(result.Response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream");
            var title = GetTitle(content, contentType);

            return new CrawlContent
            {
                RequestUri = toCrawl.RequestUri,
                StatusCode = (int)result.Response.StatusCode,
                Redirects = result.Redirects,
                ContentType = contentType,
                Content = content,
                Parent = toCrawl.Parent,
                RetryCount = toCrawl.RetryCount,
                Message = null,
                DownloadTime = sw.Elapsed,
                Title = title
            };
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger?.LogError(e, e.Message);

            return new CrawlContent
            {
                RequestUri = toCrawl.RequestUri,
                StatusCode = 599,
                Redirects = result.Redirects ?? [],
                ContentType = null,
                Content = null,
                Parent = toCrawl.Parent,
                RetryCount = toCrawl.RetryCount,
                Message = e.Message,
                DownloadTime = sw.Elapsed,
                Title = null
            };
        }
        finally
        {
            result.Response?.Dispose();
        }
    }

    private static string GetTitle(byte[] content, ContentType contentType)
    {
        var htmlContent = content.ToStringContent(contentType);
        if (htmlContent == null) return null;
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlContent);
        var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//head/title");
        var title = HttpUtility.HtmlDecode(titleNode?.InnerText.Trim());
        return title;
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