using System.Text.RegularExpressions;
using Tharga.Crawler.Filter;

namespace Tharga.Crawler.Helper;

public static class UriHelper
{
    public static Uri TrimFragment(Uri uri)
    {
        if (uri == null) throw new ArgumentNullException(nameof(uri));
        return new Uri($"{uri.GetLeftPart(UriPartial.Path)}{uri.Query}");
    }

    public static Uri ApplyUrlReplacements(this Uri uri, UrlReplaceExpression[] urlReplaceExpressions)
    {
        var uriString = uri.AbsoluteUri;
        return new Uri(uriString.ApplyUrlReplacements(urlReplaceExpressions));
    }

    public static string ApplyUrlReplacements(this string uriString, UrlReplaceExpression[] urlReplaceExpressions)
    {
        if (urlReplaceExpressions.Length == 0) return uriString;

        foreach (var expression in urlReplaceExpressions)
        {
            if (string.IsNullOrEmpty(expression.Pattern)) continue;

            try
            {
                uriString = Regex.Replace(uriString, expression.Pattern, expression.Replacement ?? string.Empty);
            }
            catch (ArgumentException)
            {
                uriString = uriString.Replace(expression.Pattern, expression.Replacement, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        return uriString;
    }
}