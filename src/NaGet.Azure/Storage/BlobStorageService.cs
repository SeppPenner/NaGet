namespace NaGet.Azure;

// See: https://github.com/NuGet/NuGetGallery/blob/master/src/NuGetGallery.Core/Services/CloudBlobCoreFileStorageService.cs
public class BlobStorageService : IStorageService
{
    private readonly CloudBlobContainer container;

    public BlobStorageService(CloudBlobContainer container)
    {
        this.container = container ?? throw new ArgumentNullException(nameof(container));
    }

    public async Task<Stream> GetAsync(string path, CancellationToken cancellationToken)
    {
        return await container
            .GetBlockBlobReference(path)
            .OpenReadAsync(cancellationToken);
    }

    public Task<Uri> GetDownloadUriAsync(string path, CancellationToken cancellationToken)
    {
        // TODO: Make expiry time configurable.
        var blob = container.GetBlockBlobReference(path);
        var accessPolicy = new SharedAccessBlobPolicy
        {
            SharedAccessExpiryTime = DateTimeOffset.Now.Add(TimeSpan.FromMinutes(10)),
            Permissions = SharedAccessBlobPermissions.Read
        };

        var signature = blob.GetSharedAccessSignature(accessPolicy);
        var result = new Uri(blob.Uri, signature);

        return Task.FromResult(result);
    }

    public async Task<StoragePutResult> PutAsync(
        string path,
        Stream content,
        string contentType,
        CancellationToken cancellationToken)
    {
        var blob = container.GetBlockBlobReference(path);
        var condition = AccessCondition.GenerateIfNotExistsCondition();

        blob.Properties.ContentType = contentType;

        try
        {
            await blob.UploadFromStreamAsync(
                content,
                condition,
                options: null,
                operationContext: null,
                cancellationToken: cancellationToken);

            return StoragePutResult.Success;
        }
        catch (StorageException e) when (e.IsAlreadyExistsException())
        {
            using var targetStream = await blob.OpenReadAsync(cancellationToken);
            content.Position = 0;
            return content.Matches(targetStream)
                ? StoragePutResult.AlreadyExists
                : StoragePutResult.Conflict;
        }
    }

    public async Task DeleteAsync(string path, CancellationToken cancellationToken)
    {
        await container
            .GetBlockBlobReference(path)
            .DeleteIfExistsAsync(cancellationToken);
    }
}