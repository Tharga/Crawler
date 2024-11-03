using Microsoft.Extensions.DependencyInjection;
using Tharga.Crawler.Downloader;
using Tharga.Crawler.PageProcessor;
using Tharga.Crawler.Processor;
using Tharga.Crawler.Scheduler;

namespace Tharga.Crawler;

public static class Registration
{
    public static void RegisterCrawler(this IServiceCollection services)
    {
        services.AddTransient<ICrawler, Crawler>();
        services.AddTransient<IScheduler, MemoryScheduler>();
        services.AddTransient<IPageProcessor, BasicPageProcessor>();
        services.AddTransient<IDownloader, HttpClientDownloader>();
    }
}