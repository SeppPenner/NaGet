// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AliyunStorageService.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The Aliyun storage service class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.Aliyun;

/// <inheritdoc cref="IStorageService"/>
/// <summary>
/// The Aliyun storage service class.
/// </summary>
public class AliyunStorageService : IStorageService
{
    /// <summary>
    /// The separator.
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
    /// The Aliyun client.
    /// </summary>
    private readonly OssClient client;

    /// <summary>
    /// Initializes a new instance of the <see cref="AliyunStorageService"/> class.
    /// </summary>
    /// <param name="options">The storage options.</param>
    /// <param name="client">The Aliyun client.</param>
    /// <exception cref="ArgumentNullException">Thrown if the options or the client is null.</exception>
    public AliyunStorageService(IOptionsSnapshot<AliyunStorageOptions> options, OssClient client)
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

    /// <inheritdoc cref="IStorageService"/>
    public async Task<Stream?> Get(string path, CancellationToken cancellationToken = default)
    {
        try
        {
            var ossObject = await Task.Factory.FromAsync(client.BeginGetObject, client.EndGetObject, bucket, PrepareKey(path), null);
            return ossObject.ResponseStream;
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
        var uri = client.GeneratePresignedUri(bucket, PrepareKey(path));
        return Task.FromResult<Uri?>(uri);
    }

    /// <inheritdoc cref="IStorageService"/>
    public async Task<StoragePutResult> Put(string path, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        // TODO: Uploads should be idempotent. This should fail if and only if the blob
        // already exists but has different content.

        var metadata = new ObjectMetadata
        {
            ContentType = contentType,
        };

        var putResult = await Task<PutObjectResult>.Factory.FromAsync(client.BeginPutObject, client.EndPutObject, bucket, PrepareKey(path), content, metadata);

        return putResult.HttpStatusCode switch
        {
            HttpStatusCode.OK => StoragePutResult.Success,
            // TODO: check sdk documents
            //case System.Net.HttpStatusCode.Conflict:
            //    return StoragePutResult.Conflict;
            //case System.Net.HttpStatusCode.Found:
            //    return StoragePutResult.AlreadyExists;
            _ => StoragePutResult.Success
        };
    }

    /// <inheritdoc cref="IStorageService"/>
    public Task Delete(string path, CancellationToken cancellationToken = default)
    {
        client.DeleteObject(bucket, PrepareKey(path));
        return Task.CompletedTask;
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
}
