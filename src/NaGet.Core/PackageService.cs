namespace NaGet.Core;

public class PackageService : IPackageService
{
    private readonly IPackageDatabase database;
    private readonly IUpstreamClient upstream;
    private readonly IPackageIndexingService indexer;
    private readonly ILogger<PackageService> logger;

    public PackageService(
        IPackageDatabase db,
        IUpstreamClient upstream,
        IPackageIndexingService indexer,
        ILogger<PackageService> logger)
    {
        database = db ?? throw new ArgumentNullException(nameof(db));
        this.upstream = upstream ?? throw new ArgumentNullException(nameof(upstream));
        this.indexer = indexer ?? throw new ArgumentNullException(nameof(indexer));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<NuGetVersion>> FindPackageVersionsAsync(
        string id,
        CancellationToken cancellationToken)
    {
        var upstreamVersions = await upstream.ListPackageVersionsAsync(id, cancellationToken);

        // Merge the local package versions into the upstream package versions.
        var localPackages = await database.Find(id, includeUnlisted: true, cancellationToken);
        var localVersions = localPackages.Select(p => p.Version);

        if (!upstreamVersions.Any())
        {
            return localVersions.ToList();
        }

        if (!localPackages.Any())
        {
            return upstreamVersions;
        }

        return upstreamVersions.Concat(localVersions).Distinct().ToList();
    }

    public async Task<IReadOnlyList<Package>> FindPackagesAsync(string id, CancellationToken cancellationToken)
    {
        var upstreamPackages = await upstream.ListPackagesAsync(id, cancellationToken);
        var localPackages = await database.Find(id, includeUnlisted: true, cancellationToken);

        if (!upstreamPackages.Any())
        {
            return localPackages;
        }

        if (!localPackages.Any())
        {
            return upstreamPackages;
        }

        // Merge the local packages into the upstream packages.
        var result = upstreamPackages.ToDictionary(p => p.Version);
        var local = localPackages.ToDictionary(p => p.Version);

        foreach (var localPackage in local)
        {
            result[localPackage.Key] = localPackage.Value;
        }

        return result.Values.ToList();
    }

    public async Task<Package?> FindPackageOrNullAsync(
        string id,
        NuGetVersion version,
        CancellationToken cancellationToken)
    {
        if (!await MirrorAsync(id, version, cancellationToken))
        {
            return null;
        }

        return await database.FindOrNull(id, version, includeUnlisted: true, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string id, NuGetVersion version, CancellationToken cancellationToken)
    {
        return await MirrorAsync(id, version, cancellationToken);
    }

    public async Task AddDownloadAsync(string packageId, NuGetVersion version, CancellationToken cancellationToken)
    {
        await database.AddDownload(packageId, version, cancellationToken);
    }

    /// <summary>
    /// Index the package from an upstream if it does not exist locally.
    /// </summary>
    /// <param name="id">The package ID to index from an upstream.</param>
    /// <param name="version">The package version to index from an upstream.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>True if the package exists locally or was indexed from an upstream source.</returns>
    private async Task<bool> MirrorAsync(string id, NuGetVersion version, CancellationToken cancellationToken)
    {
        if (await database.Exists(id, version, cancellationToken))
        {
            return true;
        }

        logger.LogInformation(
            "Package {PackageId} {PackageVersion} does not exist locally. Checking upstream feed...",
            id,
            version);

        try
        {
            using var packageStream = await upstream.DownloadPackageOrNullAsync(id, version, cancellationToken);

            if (packageStream is null)
            {
                logger.LogWarning(
                    "Upstream feed does not have package {PackageId} {PackageVersion}",
                    id,
                    version);
                return false;
            }

            logger.LogInformation(
                "Downloaded package {PackageId} {PackageVersion}, indexing...",
                id,
                version);

            var result = await indexer.IndexAsync(packageStream, cancellationToken);

            logger.LogInformation(
                "Finished indexing package {PackageId} {PackageVersion} from upstream feed with result {Result}",
                id,
                version,
                result);

            return result == PackageIndexingResult.Success;
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to index package {PackageId} {PackageVersion} from upstream",
                id,
                version);

            return false;
        }
    }
}
