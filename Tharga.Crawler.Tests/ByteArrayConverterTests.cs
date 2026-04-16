using System.Net.Mime;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Tharga.Crawler.Helper;
using Xunit;

namespace Tharga.Crawler.Tests;

public class ByteArrayConverterTests
{
    [Fact]
    public void TextHtml_ReturnsString()
    {
        var data = "hello"u8.ToArray();
        var result = data.ToStringContent(new ContentType("text/html"));
        result.Should().Be("hello");
    }

    [Fact]
    public void ApplicationJson_ReturnsString()
    {
        var data = "{}"u8.ToArray();
        var result = data.ToStringContent(new ContentType("application/json"));
        result.Should().Be("{}");
    }

    [Fact]
    public void ImagePng_ReturnsBase64()
    {
        var data = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
        var result = data.ToStringContent(new ContentType("image/png"));
        result.Should().Be(Convert.ToBase64String(data));
    }

    [Fact]
    public void Font_ReturnsNull()
    {
        var data = new byte[] { 0x00, 0x01 };
        var result = data.ToStringContent(new ContentType("font/woff2"));
        result.Should().BeNull();
    }

    [Fact]
    public void UnsupportedContentType_ReturnsNull_AndLogsWarning()
    {
        var data = new byte[] { 0x00 };
        var logger = new Mock<ILogger>(MockBehavior.Loose);

        var result = data.ToStringContent(new ContentType("model/gltf+json"), logger.Object);

        result.Should().BeNull();
        logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("model/gltf+json")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void EmptyData_ReturnsEmpty()
    {
        var result = Array.Empty<byte>().ToStringContent(new ContentType("text/html"));
        result.Should().BeEmpty();
    }

    [Fact]
    public void NullData_ReturnsEmpty()
    {
        var result = ((byte[])null).ToStringContent(new ContentType("text/html"));
        result.Should().BeEmpty();
    }
}
