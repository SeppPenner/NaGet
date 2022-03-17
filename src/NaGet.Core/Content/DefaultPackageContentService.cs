namespace NaGet.Core;

/// <summary>
/// Implements the NuGet Package Content resource in NuGet's V3 protocol.
/// </summary>
public class DefaultPackageContentService : IPackageContentService
{
    private readonly IPackageService packages;
    private readonly IPackageStorageService storage;

    public DefaultPackageContentService(
        IPackageService packages,
        IPackageStorageService storage)
    {
        this.packages = packages ?? throw new ArgumentNullException(nameof(packages));
        this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    public async Task<PackageVersionsResponse?> GetPackageVersionsOrNullAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var versions = await packages.FindPackageVersionsAsync(id, cancellationToken);

        if (!versions.Any())
        {
            return null;
        }

        return new PackageVersionsResponse
        {
            Versions = versions
                .Select(v => v.ToNormalizedString())
                .Select(v => v.ToLowerInvariant())
                .ToList()
        };
    }

    public async Task<Stream?> GetPackageContentStreamOrNullAsync(
        string id,
        NuGetVersion version,
        CancellationToken cancellationToken = default)
    {
        if (!await packages.ExistsAsync(id, version, cancellationToken))
        {
            return null;
        }

        await packages.AddDownloadAsync(id, version, cancellationToken);
        return await storage.GetPackageStreamAsync(id, version, cancellationToken);
    }

    public async Task<Stream?> GetPackageManifestStreamOrNullAsync(string id, NuGetVersion version, CancellationToken cancellationToken = default)
    {
        if (!await packages.ExistsAsync(id, version, cancellationToken))
        {
            return null;
        }

        return await storage.GetNuspecStreamAsync(id, version, cancellationToken);
    }

    public async Task<Stream?> GetPackageReadmeStreamOrNullAsync(string id, NuGetVersion version, CancellationToken cancellationToken = default)
    {
        var package = await packages.FindPackageOrNullAsync(id, version, cancellationToken);

        if (package is null || !package.HasReadme)
        {
            return null;
        }

        return await storage.GetReadmeStreamAsync(id, version, cancellationToken);
    }

    public async Task<Stream?> GetPackageIconStreamOrNullAsync(string id, NuGetVersion version, CancellationToken cancellationToken = default)
    {
        var package = await packages.FindPackageOrNullAsync(id, version, cancellationToken);

        if (package is null || !package.HasEmbeddedIcon)
        {
            return null;
        }

        return await storage.GetIconStreamAsync(id, version, cancellationToken);
    }
}
