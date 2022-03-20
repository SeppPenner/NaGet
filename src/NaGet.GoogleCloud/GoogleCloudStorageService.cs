// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GoogleCloudStorageService.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The Google cloud storage service class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.GoogleCloud;

/// <inheritdoc cref="IStorageService"/>
/// <summary>
/// The Google cloud storage service class.
/// </summary>
public class GoogleCloudStorageService : IStorageService
{
    /// <summary>
    /// The bucket name.
    /// </summary>
    private readonly string bucketName;

    /// <summary>
    /// Initializes a new instance of the <see cref="GoogleCloudStorageService"/> class.
    /// </summary>
    /// <param name="options">The storage options.</param>
    /// <exception cref="ArgumentNullException">Thrown if the options are null.</exception>
    public GoogleCloudStorageService(IOptionsSnapshot<GoogleCloudStorageOptions> options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        bucketName = options.Value.BucketName;
    }

    /// <inheritdoc cref="IStorageService"/>
    public async Task<Stream?> Get(string path, CancellationToken cancellationToken = default)
    {
        using var storage = await StorageClient.CreateAsync();
        var stream = new MemoryStream();
        await storage.DownloadObjectAsync(bucketName, PreparePath(path), stream, cancellationToken: cancellationToken);
        stream.Position = 0;
        return stream;
    }

    /// <inheritdoc cref="IStorageService"/>
    public Task<Uri?> GetDownloadUri(string path, CancellationToken cancellationToken = default)
    {
        // Returns an authenticated browser download url: https://cloud.google.com/storage/docs/request-endpoints#cookieauth.
        return Task.FromResult<Uri?>(new Uri($"https://storage.googleapis.com/{bucketName}/{PreparePath(path).TrimStart('/')}"));
    }

    /// <inheritdoc cref="IStorageService"/>
    public async Task<StoragePutResult> Put(string path, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        using var storage = await StorageClient.CreateAsync();
        using var seekableContent = new MemoryStream();
        await content.CopyToAsync(seekableContent, 65536, cancellationToken);
        seekableContent.Position = 0;

        var objectName = PreparePath(path);

        try
        {
            // attempt to upload, succeeding only if the object doesn't exist
            await storage.UploadObjectAsync(bucketName, objectName, contentType, seekableContent, new UploadObjectOptions { IfGenerationMatch = 0 }, cancellationToken);
            return StoragePutResult.Success;
        }
        catch (GoogleApiException e) when (e.HttpStatusCode == HttpStatusCode.PreconditionFailed)
        {
            // The object already exists; get the hash of its content from its metadata.
            var existingObject = await storage.GetObjectAsync(bucketName, objectName, cancellationToken: cancellationToken);
            var existingHash = Convert.FromBase64String(existingObject.Md5Hash);

            // Hash the content that was uploaded.
            seekableContent.Position = 0;
            byte[] contentHash;
            using (var md5 = MD5.Create())
            contentHash = md5.ComputeHash(seekableContent);

            // Conflict if the two hashes are different.
            return existingHash.SequenceEqual(contentHash) ? StoragePutResult.AlreadyExists : StoragePutResult.Conflict;
        }
    }

    /// <inheritdoc cref="IStorageService"/>
    public async Task Delete(string path, CancellationToken cancellationToken = default)
    {
        using var storage = await StorageClient.CreateAsync();

        try
        {
            var obj = await storage.GetObjectAsync(bucketName, PreparePath(path), cancellationToken: cancellationToken);
            await storage.DeleteObjectAsync(obj, cancellationToken: cancellationToken);
        }
        catch (GoogleApiException e) when (e.HttpStatusCode == HttpStatusCode.NotFound)
        {
        }
    }

    /// <summary>
    /// Prepares the path.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The path as <see cref="string"/>.</returns>
    private static string PreparePath(string path)
    {
        // although Google Cloud Storage objects exist in a flat namespace, using forward slashes allows the objects to
        // be exposed as nested subdirectories, e.g., when browsing via Google Cloud Console
        return path.Replace('\\', '/');
    }
}
