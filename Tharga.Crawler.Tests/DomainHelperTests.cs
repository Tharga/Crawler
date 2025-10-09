using FluentAssertions;
using Tharga.Crawler.Helper;
using Xunit;

namespace Tharga.Crawler.Tests;

public class DomainHelperTests
{
    [Theory]
    [InlineData("http://some.site", "http://some.site/page", true)]
    [InlineData("http://sub.some.site", "http://sub.some.site/page", true)]
    [InlineData("http://sub.some.site", "http://site/page", true)]
    [InlineData("http://top.sub.some.site", "http://site/page", true)]
    [InlineData("http://top.sub.some.site", "http://some.site/page", true)]
    [InlineData("http://top.sub.some.site", "http://sub.some.site/page", true)]
    [InlineData("http://top.sub.some.site", "http://top.sub.some.site/page", true)]
    [InlineData("http://site", "http://other/page", false)]
    [InlineData("http://some.site", "http://other.site/page", false)]
    [InlineData("http://sub.some.site", "http://other.some.site/page", false)]
    [InlineData("http://site/page", "http://top.sub.some.site", false)]
    [InlineData("http://some.site/page", "http://top.sub.some.site", false)]
    [InlineData("http://sub.some.site/page", "http://top.sub.some.site", false)]
    public async Task IsBasedOn(string page, string link, bool follow)
    {
        new Uri(link).IsBasedOn(new Uri(page)).Should().Be(follow);
    }
}