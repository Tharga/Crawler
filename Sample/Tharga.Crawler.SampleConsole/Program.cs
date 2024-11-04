using Microsoft.Extensions.DependencyInjection;
using Tharga.Console;
using Tharga.Console.Commands;
using Tharga.Console.Consoles;
using Tharga.Crawler;
using Tharga.Crawler.SampleConsole.Commands;

var services = new ServiceCollection();
services.AddTransient<CrawlCommand>();
services.RegisterCrawler();
//services.AddLogging(x =>
//{
//    x.AddConsole();
//    x.SetMinimumLevel(LogLevel.Trace);
//});
var serviceProvider = services.BuildServiceProvider();

using var console = new ClientConsole();
var command = new RootCommand(console, new CommandResolver(type => (ICommand)serviceProvider.GetService(type)));
command.RegisterCommand<CrawlCommand>();
var engine = new CommandEngine(command);
engine.Start(args);