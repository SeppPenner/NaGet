// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BlobStorageService.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The Azure blob storage service class.
//    See: https://github.com/NuGet/NuGetGallery/blob/master/src/NuGetGallery.Core/Services/CloudBlobCoreFileStorageService.cs
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.Azure.Storage;

using CosmosStorageException = Microsoft.Azure.Cosmos.Table.StorageException;
using StorageAccessCondition = Microsoft.WindowsAzure.Storage.AccessCondition;

/// <inheritdoc cref="IStorageService"/>
/// <summary>
/// The Azure blob storage service class.
/// See: https://github.com/NuGet/NuGetGallery/blob/master/src/NuGetGallery.Core/Services/CloudBlobCoreFileStorageService.cs
/// </summary>
public class BlobStorageService : IStorageService
{
    /// <summary>
    /// The cloud blob container.
    /// </summary>
    private readonly CloudBlobContainer container;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlobStorageService"/> class.
    /// </summary>
    /// <param name="container">The container.</param>
    /// <exception cref="ArgumentNullException">Thrown if the container is null.</exception>
    public BlobStorageService(CloudBlobContainer container)
    {
        this.container = container ?? throw new ArgumentNullException(nameof(container));
    }

    /// <inheritdoc cref="IStorageService"/>
    public async Task<Stream?> Get(string path, CancellationToken cancellationToken)
    {
        return await container
            .GetBlockBlobReference(path)
            .OpenReadAsync(cancellationToken);
    }

    /// <inheritdoc cref="IStorageService"/>
    public Task<Uri?> GetDownloadUri(string path, CancellationToken cancellationToken)
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
        return Task.FromResult<Uri?>(result);
    }

    /// <inheritdoc cref="IStorageService"/>
    public async Task<StoragePutResult> Put(
        string path,
        Stream content,
        string contentType,
        CancellationToken cancellationToken)
    {
        var blob = container.GetBlockBlobReference(path);
        var condition = StorageAccessCondition.GenerateIfNotExistsCondition();

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
        catch (CosmosStorageException e) when (e.IsAlreadyExistsException())
        {
            using var targetStream = await blob.OpenReadAsync(cancellationToken);
            content.Position = 0;
            return content.Matches(targetStream)
                ? StoragePutResult.AlreadyExists
                : StoragePutResult.Conflict;
        }
    }

    /// <inheritdoc cref="IStorageService"/>
    public async Task Delete(string path, CancellationToken cancellationToken)
    {
        await container
            .GetBlockBlobReference(path)
            .DeleteIfExistsAsync(cancellationToken);
    }
}
