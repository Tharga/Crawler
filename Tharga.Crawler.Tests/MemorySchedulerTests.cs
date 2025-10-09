using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Tharga.Crawler.Entity;
using Tharga.Crawler.Scheduler;
using Xunit;

namespace Tharga.Crawler.Tests;

public class MemorySchedulerTests
{
    [Fact]
    public async Task Basic()
    {
        //Arrange
        var toCrawl = new ToCrawl { RequestUri = new Uri("https://some.domain") };
        var options = new SchedulerOptions();
        var logger = new Mock<ILogger<MemoryScheduler>>(MockBehavior.Loose);
        var uriService = new Mock<IUriService>(MockBehavior.Strict);
        uriService.Setup(x => x.ShouldEnqueueAsync(It.IsAny<Uri>(), It.IsAny<Uri>())).ReturnsAsync(true);
        uriService.Setup(x => x.MutateUriAsync(It.IsAny<Uri>())).ReturnsAsync((Uri uri) => uri);
        var sut = new MemoryScheduler(uriService.Object, logger.Object);

        //Act
        await sut.EnqueueAsync(toCrawl, options);

        //Assert
        (await sut.GetQueued().ToArrayAsync()).Should().HaveCount(1);
        uriService.Verify(x => x.ShouldEnqueueAsync(It.IsAny<Uri>(), It.IsAny<Uri>()), Times.Once);
        uriService.Verify(x => x.MutateUriAsync(It.IsAny<Uri>()), Times.Once);
    }

    [Fact]
    public async Task SameUriTwice()
    {
        //Arrange
        var toCrawl = new ToCrawl { RequestUri = new Uri("https://some.domain") };
        var options = new SchedulerOptions();
        var logger = new Mock<ILogger<MemoryScheduler>>(MockBehavior.Loose);
        var uriService = new Mock<IUriService>(MockBehavior.Strict);
        uriService.Setup(x => x.ShouldEnqueueAsync(It.IsAny<Uri>(), It.IsAny<Uri>())).ReturnsAsync(true);
        uriService.Setup(x => x.MutateUriAsync(It.IsAny<Uri>())).ReturnsAsync((Uri uri) => uri);
        var sut = new MemoryScheduler(uriService.Object, logger.Object);
        await sut.EnqueueAsync(toCrawl, options);

        //Act
        await sut.EnqueueAsync(toCrawl, options);

        //Assert
        (await sut.GetQueued().ToArrayAsync()).Should().HaveCount(1);
        uriService.Verify(x => x.ShouldEnqueueAsync(It.IsAny<Uri>(), It.IsAny<Uri>()), Times.Exactly(2));
        uriService.Verify(x => x.MutateUriAsync(It.IsAny<Uri>()), Times.Exactly(2));
    }
}