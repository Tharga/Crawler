namespace Tharga.Crawler;

public interface IUriService
{
    public Task<bool> ShouldIncludeAsync(Uri uri);
    public Task<Uri> MutateUriAsync(Uri uri);
}