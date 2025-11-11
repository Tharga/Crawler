using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tharga.Crawler.Downloader;
using Tharga.Crawler.PageProcessor;
using Tharga.Crawler.Scheduler;

namespace Tharga.Crawler;

public static class Registration
{
    [Obsolete($"Use {nameof(AddCrawler)} instead.")]
    public static void RegisterCrawler(this IServiceCollection services, Action<CrawlerRegistrationOptions> options = null)
    {
        AddCrawler(services, options);
    }

    public static void AddCrawler(this IServiceCollection services, Action<CrawlerRegistrationOptions> options = null)
    {
        var o = new CrawlerRegistrationOptions
        {
            Crawler = provider => new Crawler(provider.GetService<IScheduler>(), provider.GetService<IPageProcessor>(), provider.GetService<IDownloader>(), provider.GetService<ILoggerFactory>().CreateLogger<Crawler>()),
            Scheduler = provider => new MemoryScheduler(provider.GetService<IUriService>(), provider.GetService<ILogger<MemoryScheduler>>()),
            PageProcessor = provider => new BasicPageProcessor(provider.GetService<ILogger<BasicPageProcessor>>()),
            Downloader = provider => new HttpClientDownloader(provider.GetService<ILogger<HttpClientDownloader>>()),
            UriService = provider => new UriService(provider.GetService<ILogger<UriService>>())
        };
        options?.Invoke(o);

        services.AddTransient<ICrawlerProvider, CrawlerProvider>();
        services.AddTransient(o.Crawler);
        services.AddTransient(o.Scheduler);
        services.AddTransient(o.PageProcessor);
        services.AddTransient(o.Downloader);
        services.AddTransient(o.UriService);
    }
}