using System.Text.RegularExpressions;
using Tharga.Crawler.Filter;

namespace Tharga.Crawler.Helper;

internal static class FilterHelper
{
    public static bool Filter(this Uri uri, StringFilter[] filters)
    {
        return Filter(uri, filters, FilterOperation.Exclude) || !Filter(uri, filters, FilterOperation.Include);
    }

    private static bool Filter(Uri uri, StringFilter[] filters, FilterOperation operation)
    {
        filters = filters?.Where(x => x.Operation == operation).ToArray() ?? [];

        if (!filters.Any()) return operation == FilterOperation.Include;

        foreach (var filter in filters)
        {
            if (IsMatch(uri, filter)) return true;
        }

        return false;
    }

    private static bool IsMatch(Uri uri, StringFilter filter)
    {
        var uriString = uri.AbsoluteUri;
        try
        {
            return Regex.IsMatch(uriString, filter.Expression, RegexOptions.IgnoreCase);
        }
        catch (ArgumentException)
        {
            // Not a valid regex pattern, treat as literal contains
            return uriString.Contains(filter.Expression, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}