using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tharga.Crawler.Downloader;
using Tharga.Crawler.PageProcessor;
using Tharga.Crawler.Scheduler;

namespace Tharga.Crawler;

internal class CrawlerProvider : ICrawlerProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoggerFactory _loggerFactory;

    public CrawlerProvider(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
    {
        _serviceProvider = serviceProvider;
        _loggerFactory = loggerFactory;
    }

    public ICrawler GetCrawlerInstance(IScheduler scheduler, IPageProcessor pageProcessor, IDownloader downloader)
    {
        scheduler ??= _serviceProvider.GetService<IScheduler>();
        pageProcessor ??= _serviceProvider.GetService<IPageProcessor>();
        downloader ??= _serviceProvider.GetService<IDownloader>();
        var logger = _loggerFactory.CreateLogger<Crawler>();
        return new Crawler(scheduler, pageProcessor, downloader, logger);
    }
}