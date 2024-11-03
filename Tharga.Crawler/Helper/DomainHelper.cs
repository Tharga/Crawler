namespace Tharga.Crawler.Helper;

internal static class DomainHelper
{
    public static string GetRootDomain(this Uri uri)
    {
        var hostParts = uri.Host.Split('.');

        // Ensure there are at least two parts (e.g., root.com)
        if (hostParts.Length < 2)
        {
            return uri.Host;
        }

        // Take the last two segments for the root domain
        return string.Join(".", hostParts.TakeLast(2));
    }

    public static bool HaveSameRootDomain(this Uri uri, Uri otherUri)
    {
        return uri.GetRootDomain().Equals(otherUri.GetRootDomain(), StringComparison.OrdinalIgnoreCase);
    }
}