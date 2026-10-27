using System.Net;
using System.Net.Sockets;
using AwesomeAssertions;
using Tharga.Crawler.Downloader;
using Tharga.Crawler.Entity;
using Xunit;

namespace Tharga.Crawler.Tests;

public class HttpClientDownloaderTests
{
    [Fact]
    public async Task Request_that_times_out_returns_a_failed_download()
    {
        //Arrange
        using var server = new SilentServer();
        var sut = new HttpClientDownloader();
        var options = new DownloadOptions { Timeout = TimeSpan.FromMilliseconds(300) };

        //Act
        var result = await sut.GetAsync(new ToCrawl { RequestUri = server.Uri }, options, CancellationToken.None);

        //Assert
        result.StatusCode.Should().Be(599);
        result.RequestUri.Should().Be(server.Uri);
        result.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Cancelling_the_token_still_throws()
    {
        //Arrange
        using var server = new SilentServer();
        var sut = new HttpClientDownloader();
        var options = new DownloadOptions { Timeout = TimeSpan.FromSeconds(30) };
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));

        //Act
        var act = () => sut.GetAsync(new ToCrawl { RequestUri = server.Uri }, options, cts.Token);

        //Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    /// <summary>
    /// A local TCP server that accepts connections and never answers, so every request to it waits until it is timed out or cancelled.
    /// </summary>
    private sealed class SilentServer : IDisposable
    {
        private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
        private readonly List<TcpClient> _clients = [];

        public SilentServer()
        {
            _listener.Start();
            Uri = new Uri($"http://127.0.0.1:{((IPEndPoint)_listener.LocalEndpoint).Port}/");
            _ = AcceptAsync();
        }

        public Uri Uri { get; }

        private async Task AcceptAsync()
        {
            try
            {
                while (true)
                {
                    var client = await _listener.AcceptTcpClientAsync();
                    lock (_clients) _clients.Add(client);
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (SocketException)
            {
            }
        }

        public void Dispose()
        {
            _listener.Stop();
            lock (_clients)
            {
                foreach (var client in _clients) client.Dispose();
            }
        }
    }
}
