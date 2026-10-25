using System.Diagnostics;
using System.Net.Mime;
using AwesomeAssertions;
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
    public async Task SchedulerEvent_HasCorrectCounts_ThroughLifecycle()
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
        var events = new List<SchedulerEventArgs>();
        sut.SchedulerEvent += (_, e) => events.Add(e);

        //Act - enqueue 3 items
        await sut.EnqueueAsync(items, options);

        //Assert - after enqueue: 3 queued, 0 crawling, 0 complete
        events.Should().HaveCount(1);
        events[0].QueueCount.Should().Be(3);
        events[0].CrawlingCount.Should().Be(0);
        events[0].CrawledCount.Should().Be(0);

        //Act - dequeue one item
        using var scope = await sut.GetQueuedItemScope(CancellationToken.None);

        //Assert - after dequeue: 2 queued, 1 crawling, 0 complete
        events.Should().HaveCount(2);
        events[1].QueueCount.Should().Be(2);
        events[1].CrawlingCount.Should().Be(1);
        events[1].CrawledCount.Should().Be(0);

        //Act - complete the item
        var crawled = new CrawlContent
        {
            RequestUri = scope.ToCrawl.RequestUri,
            StatusCode = 200,
            Redirects = [],
            ContentType = new ContentType("text/html"),
            DownloadTime = TimeSpan.FromMilliseconds(100),
            Title = "Test",
            Message = null,
            Content = []
        };
        scope.Commit(crawled);

        //Assert - after complete: 2 queued, 0 crawling, 1 complete
        events.Should().HaveCount(3);
        events[2].QueueCount.Should().Be(2);
        events[2].CrawlingCount.Should().Be(0);
        events[2].CrawledCount.Should().Be(1);
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

    [Fact]
    public async Task GetQueuedItemScope_ReturnsLowestRetryCountAndLevel()
    {
        //Arrange
        var options = new SchedulerOptions();
        var uriService = new Mock<IUriService>(MockBehavior.Strict);
        uriService.Setup(x => x.ShouldEnqueueAsync(It.IsAny<Uri>(), It.IsAny<Uri>())).ReturnsAsync(true);
        uriService.Setup(x => x.MutateUriAsync(It.IsAny<Uri>())).ReturnsAsync((Uri uri) => uri);
        var sut = new MemoryScheduler(uriService.Object);

        var parent = new CrawlContent
        {
            RequestUri = new Uri("https://example.com/parent"),
            StatusCode = 200, Redirects = [], ContentType = null,
            DownloadTime = null, Title = null, Message = null, Content = []
        };

        var items = new[]
        {
            new ToCrawl { RequestUri = new Uri("https://example.com/retry1"), RetryCount = 1 },
            new ToCrawl { RequestUri = new Uri("https://example.com/level2"), Parent = parent },
            new ToCrawl { RequestUri = new Uri("https://example.com/first"), RetryCount = 0 },
        };
        await sut.EnqueueAsync(items, options);

        //Act
        using var scope = await sut.GetQueuedItemScope(CancellationToken.None);

        //Assert — RetryCount 0 + Level 0 should come first
        scope.ToCrawl.RequestUri.Should().Be(new Uri("https://example.com/first"));
    }

    [Fact]
    public async Task GetQueuedItemScope_AfterRetry_ReturnsWithCorrectPriority()
    {
        //Arrange
        var options = new SchedulerOptions();
        var uriService = new Mock<IUriService>(MockBehavior.Strict);
        uriService.Setup(x => x.ShouldEnqueueAsync(It.IsAny<Uri>(), It.IsAny<Uri>())).ReturnsAsync(true);
        uriService.Setup(x => x.MutateUriAsync(It.IsAny<Uri>())).ReturnsAsync((Uri uri) => uri);
        var sut = new MemoryScheduler(uriService.Object);

        var items = new[]
        {
            new ToCrawl { RequestUri = new Uri("https://example.com/a"), RetryCount = 0 },
            new ToCrawl { RequestUri = new Uri("https://example.com/b"), RetryCount = 0 },
        };
        await sut.EnqueueAsync(items, options);

        //Act — dequeue first item and retry it (bumps RetryCount to 1)
        using var scope1 = await sut.GetQueuedItemScope(CancellationToken.None);
        scope1.Retry();

        //Assert — next dequeue should return /b (RetryCount 0) before /a (RetryCount 1)
        using var scope2 = await sut.GetQueuedItemScope(CancellationToken.None);
        scope2.ToCrawl.RequestUri.Should().Be(new Uri("https://example.com/b"));

        using var scope3 = await sut.GetQueuedItemScope(CancellationToken.None);
        scope3.ToCrawl.RequestUri.Should().Be(new Uri("https://example.com/a"));
    }

    [Fact]
    public async Task GetQueuedItemScope_PerformanceWith45kItems()
    {
        //Arrange
        var options = new SchedulerOptions();
        var uriService = new Mock<IUriService>(MockBehavior.Strict);
        uriService.Setup(x => x.ShouldEnqueueAsync(It.IsAny<Uri>(), It.IsAny<Uri>())).ReturnsAsync(true);
        uriService.Setup(x => x.MutateUriAsync(It.IsAny<Uri>())).ReturnsAsync((Uri uri) => uri);
        var sut = new MemoryScheduler(uriService.Object);

        var items = Enumerable.Range(0, 45_000)
            .Select(i => new ToCrawl { RequestUri = new Uri($"https://example.com/page{i}") })
            .ToArray();
        await sut.EnqueueAsync(items, options);

        //Act
        var sw = Stopwatch.StartNew();
        using var scope = await sut.GetQueuedItemScope(CancellationToken.None);
        sw.Stop();

        //Assert
        scope.Should().NotBeNull();
        sw.ElapsedMilliseconds.Should().BeLessThan(50, "dequeue from 45k items should be fast with PriorityQueue");
    }

    [Fact]
    public async Task EnqueueAsync_ChildLinksIntoLargeQueue_CompletesQuickly()
    {
        //Arrange - a large initial batch, and one page being crawled
        const int initialCount = 45_000;
        var options = new SchedulerOptions();
        var sut = new MemoryScheduler(new UriService());
        sut.SchedulerEvent += (_, _) => { };

        var initialItems = Enumerable.Range(0, initialCount)
            .Select(i => new ToCrawl { RequestUri = new Uri($"https://example.com/page{i}") })
            .ToArray();
        await sut.EnqueueAsync(initialItems, options);

        using var scope = await sut.GetQueuedItemScope(CancellationToken.None);
        scope.Should().NotBeNull();

        var crawlResult = new CrawlContent
        {
            RequestUri = scope.ToCrawl.RequestUri,
            StatusCode = 200,
            Redirects = [],
            ContentType = new ContentType("text/html"),
            DownloadTime = TimeSpan.FromMilliseconds(100),
            Title = "Test Page",
            Message = null,
            Content = "<html><body><a href='/page1'>Link1</a></body></html>"u8.ToArray()
        };

        //Child links as BasicPageProcessor would produce them
        const int childCount = 113;
        var childLinks = Enumerable.Range(1, childCount)
            .Select(i => new ToCrawl
            {
                RequestUri = new Uri($"https://example.com/child-page-{i}/"),
                Parent = crawlResult,
                RetryCount = 0
            })
            .ToArray();

        //Act - enqueue the children one at a time, like the await foreach loop in Crawler, then complete the page
        var sw = Stopwatch.StartNew();
        foreach (var childLink in childLinks)
        {
            await sut.EnqueueAsync(childLink, options);
        }
        sw.Stop();
        scope.Commit(crawlResult);

        //Assert
        sw.ElapsedMilliseconds.Should().BeLessThan(10_000, "enqueuing 113 child links should not take more than 10 seconds");
        (await sut.GetQueued().ToArrayAsync()).Should().HaveCount(initialCount - 1 + childCount);
    }
}