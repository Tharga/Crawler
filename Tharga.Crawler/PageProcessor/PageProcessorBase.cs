using System.Runtime.CompilerServices;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System.Web;
using Tharga.Crawler.Entity;
using Tharga.Crawler.Helper;

namespace Tharga.Crawler.PageProcessor;

public class PageProcessorBase : IPageProcessor
{
    private readonly ILogger<PageProcessorBase> _logger;

    public PageProcessorBase(ILogger<PageProcessorBase> logger)
    {
        _logger = logger;
    }

    public virtual async IAsyncEnumerable<ToCrawl> ProcessAsync(CrawlContent page, CrawlerOptions options, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var count = 0;
        await foreach (var link in GetLinks(page).Distinct())
        {
            if (!page.RequestUri.HaveSameRootDomain(link.RequestUri) || !page.FinalUri.HaveSameRootDomain(link.RequestUri))
            {
                _logger.LogTrace("Skipping {uri} because not in domain {domain}.", link.RequestUri, page.RequestUri.GetRootDomain());
            }
            else if (!link.RequestUri.Scheme.StartsWith("http"))
            {
                _logger.LogTrace("Skipping {uri} because not scheme http or https.", link.RequestUri);
            }
            else
            {
                count++;
                yield return link;
            }
        }

        _logger.LogInformation("Found {linkCount} on page {uri}.", count, page.FinalUri);
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
    }
}