using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tharga.Crawler.Downloader;
using Tharga.Crawler.PageProcessor;
using Tharga.Crawler.Scheduler;

namespace Tharga.Crawler;

public static class Registration
{
    public static void RegisterCrawler(this IServiceCollection services, Action<CrawlerRegistrationOptions> options = default)
    {
        var o = new CrawlerRegistrationOptions
        {
            Crawler = provider => new Crawler(provider.GetService<IScheduler>(), provider.GetService<IPageProcessor>(), provider.GetService<IDownloader>(), provider.GetService<ILogger<Crawler>>()),
            Scheduler = provider => new MemoryScheduler(provider.GetService<ILogger<MemoryScheduler>>()),
            PageProcessor = provider => new PageProcessorBase(provider.GetService<ILogger<PageProcessorBase>>()),
            Downloader = provider => new HttpClientDownloader(provider.GetService<ILogger<HttpClientDownloader>>())
        };
        options?.Invoke(o);

        services.AddTransient(o.Crawler);
        services.AddTransient(o.Scheduler);
        services.AddTransient(o.PageProcessor);
        services.AddTransient(o.Downloader);
    }
}