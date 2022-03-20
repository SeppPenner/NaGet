// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AliyunStorageService.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The Amazon S3 storage service class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.AmazonS3;

/// <inheritdoc cref="IStorageService"/>
/// <summary>
/// The Amazon S3 storage service class.
/// </summary>
public class S3StorageService : IStorageService
{
    /// <summary>
    /// The seperator.
    /// </summary>
    private const string Separator = "/";

    /// <summary>
    /// The bucket.
    /// </summary>
    private readonly string bucket;

    /// <summary>
    /// The prefix.
    /// </summary>
    private readonly string prefix;

    /// <summary>
    /// The Amazon S3 client.
    /// </summary>
    private readonly AmazonS3Client client;

    /// <summary>
    /// Initializes a new instance of the <see cref="S3StorageService"/> class.
    /// </summary>
    /// <param name="options">The storage options.</param>
    /// <param name="client">The Amazon S3 client.</param>
    /// <exception cref="ArgumentNullException">Thrown if the options or the client is null.</exception>
    public S3StorageService(IOptionsSnapshot<S3StorageOptions> options, AmazonS3Client client)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        bucket = options.Value.Bucket;
        prefix = options.Value.Prefix;
        this.client = client ?? throw new ArgumentNullException(nameof(client));

        if (!string.IsNullOrWhiteSpace(prefix) && !prefix.EndsWith(Separator))
        {
            prefix += Separator;
        }
    }

    /// <summary>
    /// Prepares the key.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The key as <see cref="string"/>.</returns>
    private string PrepareKey(string path)
    {
        return prefix + path.Replace("\\", Separator);
    }

    /// <inheritdoc cref="IStorageService"/>
    public async Task<Stream?> Get(string path, CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = await client.GetObjectAsync(bucket, PrepareKey(path), cancellationToken);
            return request.ResponseStream;
        }
        catch (Exception)
        {
            // TODO
            throw;
        }
    }

    /// <inheritdoc cref="IStorageService"/>
    public Task<Uri?> GetDownloadUri(string path, CancellationToken cancellationToken = default)
    {
        var url = client.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = bucket,
            Key = PrepareKey(path)
        });

        return Task.FromResult<Uri?>(new Uri(url));
    }

    /// <inheritdoc cref="IStorageService"/>
    public async Task<StoragePutResult> Put(string path, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        // TODO: Uploads should be idempotent. This should fail if and only if the blob
        // already exists but has different content.

        using (var seekableContent = new MemoryStream())
        {
            await content.CopyToAsync(seekableContent, 4096, cancellationToken);

            seekableContent.Seek(0, SeekOrigin.Begin);

            await client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = bucket,
                Key = PrepareKey(path),
                InputStream = seekableContent,
                ContentType = contentType,
                AutoResetStreamPosition = false,
                AutoCloseStream = false
            }, cancellationToken);
        }

        return StoragePutResult.Success;
    }

    /// <inheritdoc cref="IStorageService"/>
    public async Task Delete(string path, CancellationToken cancellationToken = default)
    {
        await client.DeleteObjectAsync(bucket, PrepareKey(path), cancellationToken);
    }
}
