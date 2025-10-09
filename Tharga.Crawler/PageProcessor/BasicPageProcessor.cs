using System.Runtime.CompilerServices;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System.Web;
using Tharga.Crawler.Entity;
using Tharga.Crawler.Helper;

namespace Tharga.Crawler.PageProcessor;

public class BasicPageProcessor : IPageProcessor
{
    private readonly ILogger<BasicPageProcessor> _logger;

    public BasicPageProcessor(ILogger<BasicPageProcessor> logger = null)
    {
        _logger = logger;
    }

    public virtual async IAsyncEnumerable<ToCrawl> ProcessAsync(CrawlContent page, CrawlerOptions options, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (!page.RequestUri.HaveSameRootDomain(page.FinalUri))
        {
            _logger?.LogTrace("Skipping link search on {uri} because it was redirected to the domain {domain}.", page.RequestUri, page.FinalUri.GetRootDomain());
            yield break;
        }

        var count = 0;
        await foreach (var link in GetLinks(page).WithCancellation(cancellationToken))
        {
            count++;
            yield return link;
        }

        _logger?.LogInformation("Found {linkCount} links on page {uri}.", count, page.FinalUri);
    }

    private async IAsyncEnumerable<ToCrawl> GetLinks(CrawlContent page)
    {
        var htmlContent = page.Content.ToStringContent(page.ContentType);

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlContent);

        var htmlNodeCollection = htmlDoc.DocumentNode.SelectNodes("//a[@href]");
        if (htmlNodeCollection == null) yield break;

        foreach (var link in htmlNodeCollection)
        {
            var hrefValueRaw = link.GetAttributeValue("href", string.Empty);
            var hrefValue = HttpUtility.HtmlDecode(hrefValueRaw);
            if (!string.IsNullOrEmpty(hrefValue) && Uri.TryCreate(page.FinalUri, hrefValue, out var absoluteUri))
            {
                yield return new ToCrawl
                {
                    RequestUri = UriHelper.TrimFragment(absoluteUri),
                    Parent = page,
                    RetryCount = 0
                };
            }
        }

        await Task.Delay(0);
    }
}