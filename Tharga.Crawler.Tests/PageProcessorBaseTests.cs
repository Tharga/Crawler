using FluentAssertions;
using Moq;
using Tharga.Crawler.Entity;
using Tharga.Crawler.PageProcessor;
using Xunit;

namespace Tharga.Crawler.Tests;

public class PageProcessorBaseTests
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
}