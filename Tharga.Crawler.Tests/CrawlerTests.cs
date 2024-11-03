using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Tharga.Crawler.Downloader;
using Tharga.Crawler.Entity;
using Tharga.Crawler.PageProcessor;
using Tharga.Crawler.Scheduler;
using Xunit;

namespace Tharga.Crawler.Tests;

public class CrawlerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task Basic()
    {
        //Arrange
        var options = new CrawlerOptions { NumberOfCrawlers = 1 };
        var scheduler = new Mock<IScheduler>(MockBehavior.Strict);
        scheduler.Setup(x => x.GetAllCrawled()).Returns(Array.Empty<Crawled>().ToAsyncEnumerable);
        scheduler.Setup(x => x.EnqueueAsync(It.IsAny<ToCrawl>(), It.IsAny<SchedulerOptions>())).Returns(Task.CompletedTask);
        scheduler.Setup(x => x.TakeNextToCrawlAsync(It.IsAny<CancellationToken>())).ReturnsAsync((ToCrawl)null);
        var pageProcessor = new Mock<IPageProcessor>(MockBehavior.Strict);
        var httpDownloader = new Mock<IDownloader>(MockBehavior.Strict);
        var logger = new Mock<ILogger<Crawler>>(MockBehavior.Loose);
        var sut = new Crawler(scheduler.Object, pageProcessor.Object, httpDownloader.Object, logger.Object);

        //Act
        var result = await sut.StartAsync(new Uri("http://aaa.bbb.ccc"), options, CancellationToken.None);

        //Arrange
        result.Pages.Should().BeEmpty();
    }
}