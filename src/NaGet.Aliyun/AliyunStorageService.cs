namespace NaGet.Aliyun;

public class AliyunStorageService : IStorageService
{
    private const string Separator = "/";
    private readonly string bucket;
    private readonly string prefix;
    private readonly OssClient client;

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

    private string PrepareKey(string path)
    {
        return prefix + path.Replace("\\", Separator);
    }

    public async Task<Stream?> GetAsync(string path, CancellationToken cancellationToken = default)
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

    public Task<Uri?> GetDownloadUriAsync(string path, CancellationToken cancellationToken = default)
    {
        var uri = client.GeneratePresignedUri(bucket, PrepareKey(path));

        return Task.FromResult<Uri?>(uri);
    }

    public async Task<StoragePutResult> PutAsync(string path, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        // TODO: Uploads should be idempotent. This should fail if and only if the blob
        // already exists but has different content.

        var metadata = new ObjectMetadata
        {
            ContentType = contentType,
        };

        var putResult = await Task<PutObjectResult>.Factory.FromAsync(client.BeginPutObject, client.EndPutObject, bucket, PrepareKey(path), content, metadata);

        switch (putResult.HttpStatusCode)
        {
            case System.Net.HttpStatusCode.OK:
                return StoragePutResult.Success;

            // TODO: check sdk documents
            //case System.Net.HttpStatusCode.Conflict:
            //    return StoragePutResult.Conflict;

            //case System.Net.HttpStatusCode.Found:
            //    return StoragePutResult.AlreadyExists;

            default:
                return StoragePutResult.Success;
        }
    }

    public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        client.DeleteObject(bucket, PrepareKey(path));
        return Task.CompletedTask;
    }
}
