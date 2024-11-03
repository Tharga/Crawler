using System.Net.Mime;
using System.Text;

namespace Tharga.Crawler.Helper;

internal static class ByteArrayConverter
{
    public static string ConvertByteArrayToString(this byte[] data, ContentType contentType)
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
        else if (contentType.MediaType.StartsWith("application/") ||
                 contentType.MediaType.StartsWith("image/") ||
                 contentType.MediaType.StartsWith("audio/") ||
                 contentType.MediaType.StartsWith("video/"))
        {
            // Convert binary content to Base64 for safe representation as a string
            return Convert.ToBase64String(data);
        }
        else
        {
            throw new NotSupportedException($"Unsupported content type: {contentType.MediaType}");
        }
    }
}