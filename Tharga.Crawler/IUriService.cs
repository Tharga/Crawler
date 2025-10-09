namespace Tharga.Crawler;

public interface IUriService
{
    public Task<bool> ShouldEnqueueAsync(Uri parentUri, Uri uri);
    public Task<Uri> MutateUriAsync(Uri uri);
}