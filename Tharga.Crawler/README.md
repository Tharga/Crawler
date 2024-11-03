# Tharga Crawler

Register crawler in your service collection by calling
`services.RegisterCrawler();`.

There are two options for crawling, either by awaiting the `StartAsync` method or by listening for the event `CrawlerCompleteEvent`.

#### Example 1.
```
    var options = new CrawlerOptions();
    var result = await _crawler.StartAsync(uri, options, CancellationToken.None);
```

#### Example 2.
```
    _crawler.CrawlerCompleteEvent += (s, e) => { Debug.Write("Crawl completed."); };
    var options = new CrawlerOptions();
    _crawler.StartAsync(uri, options, CancellationToken.None);
```