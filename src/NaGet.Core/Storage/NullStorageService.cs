namespace NaGet.Core;

/// <summary>
/// A minimal storage implementation, used for advanced scenarios.
/// </summary>
public class NullStorageService : IStorageService
{
    public Task Delete(string path, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<Stream?> Get(string path, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Stream?>(null);
    }

    public Task<Uri?> GetDownloadUri(string path, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Uri?>(null);
    }

    public Task<StoragePutResult> Put(
        string path,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(StoragePutResult.Success);
    }
}
