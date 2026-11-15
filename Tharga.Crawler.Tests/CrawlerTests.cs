using System.Collections.Concurrent;
using System.Net.Mime;
using AwesomeAssertions;
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
    private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(20);

    private static readonly Uri[] Pages =
    [
        new("http://aaa.bbb.ccc/one"),
        new("http://aaa.bbb.ccc/two"),
        new("http://aaa.bbb.ccc/three")
    ];

    [Fact]
    public async Task Basic()
    {
        //Arrange
        var options = new CrawlerOptions { NumberOfProcessors = 1 };
        var scheduler = new Mock<IScheduler>(MockBehavior.Strict);
        scheduler.Setup(x => x.GetAllCrawled()).Returns(Array.Empty<Crawled>().ToAsyncEnumerable);
        scheduler.Setup(x => x.EnqueueAsync(It.IsAny<ToCrawl[]>(), It.IsAny<SchedulerOptions>())).Returns(Task.CompletedTask);
        scheduler.Setup(x => x.GetQueuedItemScope(It.IsAny<CancellationToken>())).ReturnsAsync((ToCrawlScope)null);
        var pageProcessor = new Mock<IPageProcessor>(MockBehavior.Strict);
        var httpDownloader = new Mock<IDownloader>(MockBehavior.Strict);
        var logger = new Mock<ILogger<Crawler>>(MockBehavior.Loose);
        var sut = new Crawler(scheduler.Object, pageProcessor.Object, httpDownloader.Object, logger.Object);

        //Act
        var result = await sut.StartAsync(new Uri("http://aaa.bbb.ccc"), options, CancellationToken.None);

        //Assert
        result.Pages.Should().BeEmpty();
    }

    [Fact]
    public async Task Page_that_times_out_once_is_retried_and_crawl_completes()
    {
        //Arrange
        var downloader = new FakeDownloader(Pages[1], timeouts: 1);
        var sut = new Crawler(new MemoryScheduler(new UriService()), NoLinksPageProcessor(), downloader);
        using var cts = new CancellationTokenSource(TestTimeout * 2);

        //Act
        var result = await sut.StartAsync(Pages, new CrawlerOptions { NumberOfProcessors = 2 }, cts.Token).WaitAsync(TestTimeout);

        //Assert
        result.IsCancelled.Should().BeFalse();
        result.Pages.Select(x => x.RequestUri).Should().BeEquivalentTo(Pages);
        result.Pages.Should().OnlyContain(x => x.StatusCode == 200);
        result.Pages.Single(x => x.RequestUri == Pages[1]).RetryCount.Should().Be(1);
    }

    [Fact]
    public async Task Page_that_always_times_out_is_reported_as_failed_and_crawl_completes()
    {
        //Arrange
        var downloader = new FakeDownloader(Pages[1], timeouts: int.MaxValue);
        var sut = new Crawler(new MemoryScheduler(new UriService()), NoLinksPageProcessor(), downloader);
        var options = new CrawlerOptions { NumberOfProcessors = 2, DownloadOptions = new DownloadOptions { RetryCount = 2 } };
        using var cts = new CancellationTokenSource(TestTimeout * 2);

        //Act
        var result = await sut.StartAsync(Pages, options, cts.Token).WaitAsync(TestTimeout);

        //Assert
        result.IsCancelled.Should().BeFalse();
        result.Pages.Select(x => x.RequestUri).Should().BeEquivalentTo(Pages);
        var failed = result.Pages.Single(x => x.RequestUri == Pages[1]);
        failed.StatusCode.Should().Be(599);
        failed.RetryCount.Should().Be(2);
    }

    [Fact]
    public async Task Page_that_fails_after_all_retries_raises_PageFailedEvent()
    {
        //Arrange
        var downloader = new FakeDownloader(Pages[1], timeouts: int.MaxValue);
        var sut = new Crawler(new MemoryScheduler(new UriService()), NoLinksPageProcessor(), downloader);
        var failed = new ConcurrentBag<Uri>();
        sut.PageFailedEvent += (_, e) => failed.Add(e.CrawlContent.RequestUri);
        var options = new CrawlerOptions { NumberOfProcessors = 2, DownloadOptions = new DownloadOptions { RetryCount = 2 } };
        using var cts = new CancellationTokenSource(TestTimeout * 2);

        //Act
        await sut.StartAsync(Pages, options, cts.Token).WaitAsync(TestTimeout);

        //Assert
        failed.Should().BeEquivalentTo([Pages[1]]);
    }

    [Fact]
    public async Task Many_workers_crawl_every_discovered_page()
    {
        //Arrange
        const int pageCount = 127;
        Uri PageUri(int n) => new($"http://aaa.bbb.ccc/p{n}");
        var pageProcessor = new Mock<IPageProcessor>(MockBehavior.Strict);
        pageProcessor.Setup(x => x.ProcessAsync(It.IsAny<CrawlContent>(), It.IsAny<CrawlerOptions>(), It.IsAny<CancellationToken>()))
            .Returns((CrawlContent page, CrawlerOptions _, CancellationToken _) =>
            {
                var n = int.Parse(page.RequestUri.AbsolutePath[2..]);
                return new[] { 2 * n + 1, 2 * n + 2 }
                    .Where(x => x < pageCount)
                    .Select(x => new ToCrawl { RequestUri = PageUri(x), RetryCount = 0, Parent = page })
                    .ToAsyncEnumerable();
            });
        var sut = new Crawler(new MemoryScheduler(new UriService()), pageProcessor.Object, new FakeDownloader(null, timeouts: 0));
        using var cts = new CancellationTokenSource(TestTimeout * 2);

        //Act
        var result = await sut.StartAsync(PageUri(0), new CrawlerOptions { NumberOfProcessors = 16 }, cts.Token).WaitAsync(TestTimeout);

        //Assert
        result.IsCancelled.Should().BeFalse();
        result.Pages.Should().HaveCount(pageCount);
    }

    [Fact]
    public async Task Exception_from_PageCompleteEvent_handler_does_not_stop_the_crawl()
    {
        //Arrange
        var sut = new Crawler(new MemoryScheduler(new UriService()), NoLinksPageProcessor(), new FakeDownloader(null, timeouts: 0));
        var thrown = 0;
        sut.PageCompleteEvent += (_, _) =>
        {
            if (Interlocked.Increment(ref thrown) == 1) throw new NotSupportedException("Handler failure.");
        };
        using var cts = new CancellationTokenSource(TestTimeout * 2);

        //Act
        var result = await sut.StartAsync(Pages, new CrawlerOptions { NumberOfProcessors = 2 }, cts.Token).WaitAsync(TestTimeout);

        //Assert
        result.IsCancelled.Should().BeFalse();
        result.Pages.Select(x => x.RequestUri).Should().BeEquivalentTo(Pages);
        result.Pages.Should().OnlyContain(x => x.StatusCode == 200);
    }

    [Fact]
    public async Task Cancelling_the_crawl_stops_it()
    {
        //Arrange
        var downloader = new Mock<IDownloader>(MockBehavior.Strict);
        downloader.Setup(x => x.GetAsync(It.IsAny<ToCrawl>(), It.IsAny<DownloadOptions>(), It.IsAny<CancellationToken>()))
            .Returns(async (ToCrawl _, DownloadOptions _, CancellationToken token) =>
            {
                await Task.Delay(Timeout.Infinite, token);
                return null;
            });
        var sut = new Crawler(new MemoryScheduler(new UriService()), NoLinksPageProcessor(), downloader.Object);
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));

        //Act
        var result = await sut.StartAsync(Pages, new CrawlerOptions { NumberOfProcessors = 2 }, cts.Token).WaitAsync(TestTimeout);

        //Assert
        result.IsCancelled.Should().BeTrue();
        result.Pages.Should().BeEmpty();
    }

    private static IPageProcessor NoLinksPageProcessor()
    {
        var pageProcessor = new Mock<IPageProcessor>(MockBehavior.Strict);
        pageProcessor.Setup(x => x.ProcessAsync(It.IsAny<CrawlContent>(), It.IsAny<CrawlerOptions>(), It.IsAny<CancellationToken>()))
            .Returns(AsyncEnumerable.Empty<ToCrawl>());
        return pageProcessor.Object;
    }

    /// <summary>
    /// Returns 200 for every page, except that <paramref name="timingOutUri"/> fails the way an
    /// <see cref="HttpClient"/> timeout does (a <see cref="TaskCanceledException"/> while the crawl's
    /// own token is not cancelled) for its first <paramref name="timeouts"/> requests.
    /// </summary>
    private class FakeDownloader(Uri timingOutUri, int timeouts) : IDownloader
    {
        private readonly ConcurrentDictionary<Uri, int> _requests = new();

        public Task<CrawlContent> GetAsync(ToCrawl toCrawl, DownloadOptions downloadOptions, CancellationToken cancellationToken)
        {
            var requestNo = _requests.AddOrUpdate(toCrawl.RequestUri, 1, (_, count) => count + 1);
            if (toCrawl.RequestUri == timingOutUri && requestNo <= timeouts)
            {
                throw new TaskCanceledException("The request was canceled due to the configured HttpClient.Timeout elapsing.");
            }

            return Task.FromResult(new CrawlContent
            {
                RequestUri = toCrawl.RequestUri,
                StatusCode = 200,
                Redirects = [],
                ContentType = new ContentType("text/html"),
                Content = [],
                Parent = toCrawl.Parent,
                RetryCount = toCrawl.RetryCount,
                DownloadTime = TimeSpan.Zero,
                Title = null,
                Message = null
            });
        }
    }
}
