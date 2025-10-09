using FluentAssertions;
using Xunit;

namespace Tharga.Crawler.Tests;

public class UriServiceTest
{
    [Theory]
    [InlineData(null, "http://some.site/page", true)]
    [InlineData("http://some.site", "http://some.site/page", true)]
    [InlineData("http://some.site", "http://other.site/page", false)]
    [InlineData("http://sub.some.site", "http://sub.some.site/page", true)]
    [InlineData("http://sub.some.site", "http://sub.other.site/page", false)]
    [InlineData("http://sub.some.site", "http://some.site/page", false)]
    [InlineData("http://some.site", "http://sub.some.site/page", false)]
    [InlineData("http://some.site", "http://other.some.site/page", false)]
    public async Task Default(string page, string link, bool enqueue)
    {
        //Arrange
        var parentUri = page == null ? null : new Uri(page);
        var uri = new Uri(link);
        var sut = new UriService();

        //Act
        var result = await sut.ShouldEnqueueAsync(parentUri, uri);

        //Assert
        result.Should().Be(enqueue);
    }
}