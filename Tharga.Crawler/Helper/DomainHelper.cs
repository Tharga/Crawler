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

    public static string GetDomain(this Uri uri)
    {
        return uri.Host;
    }

    public static bool HaveSameRootDomain(this Uri uri, Uri otherUri)
    {
        return uri.GetRootDomain().Equals(otherUri.GetRootDomain(), StringComparison.OrdinalIgnoreCase);
    }

    public static bool HaveSameDomain(this Uri uri, Uri otherUri)
    {
        return uri.GetDomain().Equals(otherUri.GetDomain(), StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsBasedOn(this Uri baseUri, Uri candidateUri)
    {
        if (baseUri == null || candidateUri == null)
            return false;

        var baseHost = baseUri.Host;
        var candidateHost = candidateUri.Host;

        // Normalize both to lowercase
        baseHost = baseHost.ToLowerInvariant();
        candidateHost = candidateHost.ToLowerInvariant();

        // Exact match or subdomain match
        return candidateHost == baseHost || candidateHost.EndsWith("." + baseHost);
    }
}