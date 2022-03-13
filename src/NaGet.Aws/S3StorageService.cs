namespace NaGet.Aws;

public class S3StorageService : IStorageService
{
    private const string Separator = "/";
    private readonly string bucket;
    private readonly string prefix;
    private readonly AmazonS3Client client;

    public S3StorageService(IOptionsSnapshot<S3StorageOptions> options, AmazonS3Client client)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        bucket = options.Value.Bucket;
        prefix = options.Value.Prefix;
        this.client = client ?? throw new ArgumentNullException(nameof(client));

        if (!string.IsNullOrEmpty(prefix) && !prefix.EndsWith(Separator))
        {
            prefix += Separator;
        }
    }

    private string PrepareKey(string path)
    {
        return prefix + path.Replace("\\", Separator);
    }

    public async Task<Stream> GetAsync(string path, CancellationToken cancellationToken = default)
    {
        var stream = new MemoryStream();

        try
        {
            using (var request = await client.GetObjectAsync(bucket, PrepareKey(path), cancellationToken))
            {
                await request.ResponseStream.CopyToAsync(stream);
            }

            stream.Seek(0, SeekOrigin.Begin);
        }
        catch (Exception)
        {
            stream.Dispose();

            // TODO
            throw;
        }

        return stream;
    }

    public Task<Uri> GetDownloadUriAsync(string path, CancellationToken cancellationToken = default)
    {
        var url = client.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = bucket,
            Key = PrepareKey(path)
        });

        return Task.FromResult(new Uri(url));
    }

    public async Task<StoragePutResult> PutAsync(string path, Stream content, string contentType, CancellationToken cancellationToken = default)
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

    public async Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        await client.DeleteObjectAsync(bucket, PrepareKey(path), cancellationToken);
    }
}
