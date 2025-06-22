namespace Tharga.Crawler.Helper;

internal static class UriHelper
{
    public static Uri TrimFragment(Uri uri)
    {
        if (uri == null) throw new ArgumentNullException(nameof(uri));
        return new Uri($"{uri.GetLeftPart(UriPartial.Path)}{uri.Query}");
    }
}