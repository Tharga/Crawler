using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Tharga.Crawler.Helper;
using Tharga.Crawler.Processor;

namespace Tharga.Crawler.PageProcessor;

public class BasicPageProcessor : IPageProcessor
{
    private readonly ILogger<BasicPageProcessor> _logger;

    public BasicPageProcessor(ILogger<BasicPageProcessor> logger)
    {
        _logger = logger;
    }

    public async IAsyncEnumerable<ToCrawl> ProcessAsync(CrawlContent page)
    {
        await foreach (var link in GetLinks(page))
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
                yield return link;
            }
        }
    }

    private async IAsyncEnumerable<ToCrawl> GetLinks(CrawlContent page)
    {
        var htmlContent = page.Content.ConvertByteArrayToString(page.ContentType);

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlContent);

        var htmlNodeCollection = htmlDoc.DocumentNode.SelectNodes("//a[@href]");
        if (htmlNodeCollection == null) yield break;

        foreach (var link in htmlNodeCollection)
        {
            var hrefValue = link.GetAttributeValue("href", string.Empty);
            if (!string.IsNullOrEmpty(hrefValue) && Uri.TryCreate(page.FinalUri, hrefValue, out var absoluteUri))
            {
                yield return new ToCrawl
                {
                    RequestUri = UriHelper.TrimFragment(absoluteUri)
                };
            }
        }
    }
}