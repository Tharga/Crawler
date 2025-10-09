using FluentAssertions;
using Moq;
using System.Net.Mime;
using Tharga.Crawler.Entity;
using Tharga.Crawler.PageProcessor;
using Xunit;

namespace Tharga.Crawler.Tests;

public class BasicPageProcessorTests
{
    [Fact]
    public async Task Empty()
    {
        //Arrange
        var crawlContent = Mock.Of<CrawlContent>(x => x.RequestUri == new Uri("http://some.site"));
        var sut = new BasicPageProcessor();

        //Act
        var result = await sut.ProcessAsync(crawlContent, new CrawlerOptions(), CancellationToken.None).ToArrayAsync();

        //Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task OneLink()
    {
        //Arrange
        var htmlContent = "<a href='http://some.site/page'>My Page</a>";
        var byteContent = System.Text.Encoding.UTF8.GetBytes(htmlContent);
        var crawlContent = Mock.Of<CrawlContent>(x => x.RequestUri == new Uri("http://some.site") && x.Content == byteContent && x.ContentType == new ContentType("text/html"));
        var sut = new BasicPageProcessor();

        //Act
        var result = await sut.ProcessAsync(crawlContent, new CrawlerOptions(), CancellationToken.None).ToArrayAsync();

        //Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task TwoSameLinks()
    {
        //Arrange
        var htmlContent = "<a href='http://some.site/page'>My Page</a><a href='http://some.site/page'>My Page</a>";
        var byteContent = System.Text.Encoding.UTF8.GetBytes(htmlContent);
        var crawlContent = Mock.Of<CrawlContent>(x => x.RequestUri == new Uri("http://some.site") && x.Content == byteContent && x.ContentType == new ContentType("text/html"));
        var sut = new BasicPageProcessor();

        //Act
        var result = await sut.ProcessAsync(crawlContent, new CrawlerOptions(), CancellationToken.None).ToArrayAsync();

        //Assert
        result.Should().HaveCount(2);
    }
}