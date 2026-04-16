using System.Net.Mime;
using System.Text;
using Microsoft.Extensions.Logging;
using Tharga.Crawler.Entity;

namespace Tharga.Crawler.Helper;

public static class ByteArrayConverter
{
    public static string ToStringContent(this CrawlContent page, ILogger logger = null)
    {
        if (page == null) return null;
        return page.Content.ToStringContent(page.ContentType, logger);
    }

    public static string ToStringContent(this byte[] data, ContentType contentType, ILogger logger = null)
    {
        if (data == null || data.Length == 0)
            return string.Empty;

        // Check for text-based content types
        if (contentType.MediaType.StartsWith("text/") ||
            contentType.MediaType == "application/json" ||
            contentType.MediaType == "application/xml")
        {
            // Choose encoding based on contentType's charset parameter if provided
            var encoding = Encoding.UTF8; // default to UTF-8
            if (contentType.CharSet != null)
            {
                try
                {
                    encoding = Encoding.GetEncoding(contentType.CharSet);
                }
                catch (ArgumentException)
                {
                    // Fall back to UTF-8 if the charset is unsupported
                    encoding = Encoding.UTF8;
                }
            }
            return encoding.GetString(data);
        }

        if (contentType.MediaType.StartsWith("application/") ||
            contentType.MediaType.StartsWith("image/") ||
            contentType.MediaType.StartsWith("audio/") ||
            contentType.MediaType.StartsWith("video/"))
        {
            // Convert binary content to Base64 for safe representation as a string
            return Convert.ToBase64String(data);
        }

        if (contentType.MediaType.StartsWith("font/"))
        {
            return null;
        }

        logger?.LogWarning("Unsupported content type: {contentType}", contentType.MediaType);
        return null;
    }
}