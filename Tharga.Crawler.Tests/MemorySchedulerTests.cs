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

    [Fact]
    public async Task BatchEnqueue()
    {
        //Arrange
        var items = new[]
        {
            new ToCrawl { RequestUri = new Uri("https://some.domain/a") },
            new ToCrawl { RequestUri = new Uri("https://some.domain/b") },
            new ToCrawl { RequestUri = new Uri("https://some.domain/c") }
        };
        var options = new SchedulerOptions();
        var logger = new Mock<ILogger<MemoryScheduler>>(MockBehavior.Loose);
        var uriService = new Mock<IUriService>(MockBehavior.Strict);
        uriService.Setup(x => x.ShouldEnqueueAsync(It.IsAny<Uri>(), It.IsAny<Uri>())).ReturnsAsync(true);
        uriService.Setup(x => x.MutateUriAsync(It.IsAny<Uri>())).ReturnsAsync((Uri uri) => uri);
        var sut = new MemoryScheduler(uriService.Object, logger.Object);

        //Act
        await sut.EnqueueAsync(items, options);

        //Assert
        (await sut.GetQueued().ToArrayAsync()).Should().HaveCount(3);
    }

    [Fact]
    public async Task BatchEnqueue_FiresSingleSchedulerEvent()
    {
        //Arrange
        var items = new[]
        {
            new ToCrawl { RequestUri = new Uri("https://some.domain/a") },
            new ToCrawl { RequestUri = new Uri("https://some.domain/b") }
        };
        var options = new SchedulerOptions();
        var logger = new Mock<ILogger<MemoryScheduler>>(MockBehavior.Loose);
        var uriService = new Mock<IUriService>(MockBehavior.Strict);
        uriService.Setup(x => x.ShouldEnqueueAsync(It.IsAny<Uri>(), It.IsAny<Uri>())).ReturnsAsync(true);
        uriService.Setup(x => x.MutateUriAsync(It.IsAny<Uri>())).ReturnsAsync((Uri uri) => uri);
        var sut = new MemoryScheduler(uriService.Object, logger.Object);
        var schedulerEventCount = 0;
        sut.SchedulerEvent += (_, _) => schedulerEventCount++;

        //Act
        await sut.EnqueueAsync(items, options);

        //Assert
        schedulerEventCount.Should().Be(1);
    }

    [Fact]
    public async Task BatchEnqueue_FiresSingleEnqueuedEvent_WithAllItems()
    {
        //Arrange
        var items = new[]
        {
            new ToCrawl { RequestUri = new Uri("https://some.domain/a") },
            new ToCrawl { RequestUri = new Uri("https://some.domain/b") },
            new ToCrawl { RequestUri = new Uri("https://some.domain/c") }
        };
        var options = new SchedulerOptions();
        var logger = new Mock<ILogger<MemoryScheduler>>(MockBehavior.Loose);
        var uriService = new Mock<IUriService>(MockBehavior.Strict);
        uriService.Setup(x => x.ShouldEnqueueAsync(It.IsAny<Uri>(), It.IsAny<Uri>())).ReturnsAsync(true);
        uriService.Setup(x => x.MutateUriAsync(It.IsAny<Uri>())).ReturnsAsync((Uri uri) => uri);
        var sut = new MemoryScheduler(uriService.Object, logger.Object);
        EnqueuedEventArgs capturedArgs = null;
        sut.EnqueuedEvent += (_, args) => capturedArgs = args;

        //Act
        await sut.EnqueueAsync(items, options);

        //Assert
        capturedArgs.Should().NotBeNull();
        capturedArgs.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task BatchEnqueue_RespectsMaxQueueCount()
    {
        //Arrange
        var items = new[]
        {
            new ToCrawl { RequestUri = new Uri("https://some.domain/a") },
            new ToCrawl { RequestUri = new Uri("https://some.domain/b") },
            new ToCrawl { RequestUri = new Uri("https://some.domain/c") }
        };
        var options = new SchedulerOptions { MaxQueueCount = 2 };
        var logger = new Mock<ILogger<MemoryScheduler>>(MockBehavior.Loose);
        var uriService = new Mock<IUriService>(MockBehavior.Strict);
        uriService.Setup(x => x.ShouldEnqueueAsync(It.IsAny<Uri>(), It.IsAny<Uri>())).ReturnsAsync(true);
        uriService.Setup(x => x.MutateUriAsync(It.IsAny<Uri>())).ReturnsAsync((Uri uri) => uri);
        var sut = new MemoryScheduler(uriService.Object, logger.Object);

        //Act
        await sut.EnqueueAsync(items, options);

        //Assert
        (await sut.GetQueued().ToArrayAsync()).Should().HaveCount(2);
    }

    [Fact]
    public async Task BatchEnqueue_NoDuplicates()
    {
        //Arrange
        var items = new[]
        {
            new ToCrawl { RequestUri = new Uri("https://some.domain/a") },
            new ToCrawl { RequestUri = new Uri("https://some.domain/a") },
            new ToCrawl { RequestUri = new Uri("https://some.domain/b") }
        };
        var options = new SchedulerOptions();
        var logger = new Mock<ILogger<MemoryScheduler>>(MockBehavior.Loose);
        var uriService = new Mock<IUriService>(MockBehavior.Strict);
        uriService.Setup(x => x.ShouldEnqueueAsync(It.IsAny<Uri>(), It.IsAny<Uri>())).ReturnsAsync(true);
        uriService.Setup(x => x.MutateUriAsync(It.IsAny<Uri>())).ReturnsAsync((Uri uri) => uri);
        var sut = new MemoryScheduler(uriService.Object, logger.Object);

        //Act
        await sut.EnqueueAsync(items, options);

        //Assert
        (await sut.GetQueued().ToArrayAsync()).Should().HaveCount(2);
    }

    [Fact]
    public async Task BatchEnqueue_NoEvents_WhenNothingEnqueued()
    {
        //Arrange
        var items = new[]
        {
            new ToCrawl { RequestUri = new Uri("https://some.domain/a") }
        };
        var options = new SchedulerOptions();
        var logger = new Mock<ILogger<MemoryScheduler>>(MockBehavior.Loose);
        var uriService = new Mock<IUriService>(MockBehavior.Strict);
        uriService.Setup(x => x.ShouldEnqueueAsync(It.IsAny<Uri>(), It.IsAny<Uri>())).ReturnsAsync(false);
        var sut = new MemoryScheduler(uriService.Object, logger.Object);
        var eventCount = 0;
        sut.SchedulerEvent += (_, _) => eventCount++;
        sut.EnqueuedEvent += (_, _) => eventCount++;

        //Act
        await sut.EnqueueAsync(items, options);

        //Assert
        (await sut.GetQueued().ToArrayAsync()).Should().BeEmpty();
        eventCount.Should().Be(0);
    }
}