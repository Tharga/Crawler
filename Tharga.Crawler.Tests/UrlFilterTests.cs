using FluentAssertions;
using Tharga.Crawler.Filter;
using Tharga.Crawler.Helper;
using Xunit;

namespace Tharga.Crawler.Tests;

public class UrlFilterTests
{
    [Theory]
    [InlineData("https://aaa.com", "aaa", true)]
    [InlineData("https://aaa.com", "aaa,bbb", true)]
    [InlineData("https://aaa.com", "abc", false)]
    [InlineData("https://BBB.com", "aaa,bbb", true)]
    [InlineData("https://ccc.com", "aaa,bbb", false)]
    [InlineData("https://ccc.com", "", false)]
    [InlineData("https://www.ccc.com", "^https://.*\\.com", true)]
    [InlineData("https://www.ccc.se", "^https://.*\\.com", false)]
    [InlineData("http://www.ccc.com", "^https://.*\\.com", false)]
    public void ExcludeUrlsThatMatches(string url, string filter, bool expectedToBeExcluded)
    {
        //Arrange
        var uri = new Uri(url);
        var urlFilters = filter.Split(",").Where(x => !string.IsNullOrEmpty(x)).Select(x => new UrlFilter { Expression = x, Operation = FilterOperation.Exclude }).ToArray();

        //Act
        var excluded = uri.Filter(urlFilters);

        //Assert
        excluded.Should().Be(expectedToBeExcluded);
    }

    [Theory]
    [InlineData("https://aaa.com", "aaa", true)]
    [InlineData("https://aaa.com", "aaa,bbb", true)]
    [InlineData("https://aaa.com", "abc", false)]
    [InlineData("https://BBB.com", "aaa,bbb", true)]
    [InlineData("https://ccc.com", "aaa,bbb", false)]
    [InlineData("https://ccc.com", "", true)]
    [InlineData("https://www.ccc.com", "^https://.*\\.com", true)]
    [InlineData("https://www.ccc.se", "^https://.*\\.com", false)]
    [InlineData("http://www.ccc.com", "^https://.*\\.com", false)]
    public void IncludeUrlsThatContains(string url, string filter, bool expectedToBeIncluded)
    {
        //Arrange
        var uri = new Uri(url);
        var urlFilters = filter.Split(",").Where(x => !string.IsNullOrEmpty(x)).Select(x => new UrlFilter { Expression = x, Operation = FilterOperation.Include }).ToArray();

        //Act
        var excluded = uri.Filter(urlFilters);

        //Assert
        excluded.Should().Be(!expectedToBeIncluded);
    }
}