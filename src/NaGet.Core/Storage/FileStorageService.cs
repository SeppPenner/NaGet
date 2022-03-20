namespace NaGet.Core;

/// <summary>
/// Stores content on disk.
/// </summary>
public class FileStorageService : IStorageService
{
    // See: https://github.com/dotnet/corefx/blob/master/src/Common/src/CoreLib/System/IO/Stream.cs#L35
    private const int DefaultCopyBufferSize = 81920;

    private readonly string storePath;

    public FileStorageService(IOptionsSnapshot<FileSystemStorageOptions> options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        // Resolve relative path components ('.'/'..') and ensure there is a trailing slash.
        storePath = Path.GetFullPath(options.Value.Path);

        if (!storePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
        {
            storePath += Path.DirectorySeparatorChar;
        }
    }

    public Task<Stream?> Get(string path, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        path = GetFullPath(path);
        var content = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);

        return Task.FromResult<Stream?>(content);
    }

    public Task<Uri?> GetDownloadUri(string path, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = new Uri(GetFullPath(path));

        return Task.FromResult<Uri?>(result);
    }

    public async Task<StoragePutResult> Put(
        string path,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new ArgumentException("Content type is required", nameof(contentType));
        }

        cancellationToken.ThrowIfCancellationRequested();

        path = GetFullPath(path);

        // Ensure that the path exists.
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);

        try
        {
            using var fileStream = File.Open(path, FileMode.CreateNew);
            await content.CopyToAsync(fileStream, DefaultCopyBufferSize, cancellationToken);
            return StoragePutResult.Success;
        }
        catch (IOException) when (File.Exists(path))
        {
            using var targetStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            content.Position = 0;
            return content.Matches(targetStream)
                ? StoragePutResult.AlreadyExists
                : StoragePutResult.Conflict;
        }
    }

    public Task Delete(string path, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            File.Delete(GetFullPath(path));
        }
        catch (DirectoryNotFoundException)
        {
        }

        return Task.CompletedTask;
    }

    private string GetFullPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path is required", nameof(path));
        }

        var fullPath = Path.GetFullPath(Path.Combine(storePath, path));

        // Verify path is under the _storePath.
        if (!fullPath.StartsWith(storePath, StringComparison.Ordinal) ||
            fullPath.Length == storePath.Length)
        {
            throw new ArgumentException("Path resolves outside store path", nameof(path));
        }

        return fullPath;
    }
}
