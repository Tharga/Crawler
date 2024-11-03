using Microsoft.Extensions.DependencyInjection;
using Tharga.Crawler.Downloader;
using Tharga.Crawler.PageProcessor;
using Tharga.Crawler.Scheduler;

namespace Tharga.Crawler;

public static class Registration
{
    public static void RegisterCrawler(this IServiceCollection services,
        Func<IServiceProvider, ICrawler>? customCrawler = null,
        Func<IServiceProvider, IScheduler>? customScheduler = null,
        Func<IServiceProvider, IPageProcessor>? customPageProcessor = null,
        Func<IServiceProvider, IDownloader>? customDownloader = null)
    {
        services.AddTransient<ICrawler, Crawler>();
        services.AddTransient<IScheduler, MemoryScheduler>();

        if (customPageProcessor != null)
        {
            services.AddTransient(customPageProcessor);
        }
        else
        {
            services.AddTransient<IPageProcessor, PageProcessorBase>();
        }

        services.AddTransient<IDownloader, HttpClientDownloader>();
    }
}