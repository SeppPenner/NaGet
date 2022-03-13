namespace NaGet.Core;

public class PackageDeletionService : IPackageDeletionService
{
    private readonly IPackageDatabase packages;
    private readonly IPackageStorageService storage;
    private readonly NaGetOptions options;
    private readonly ILogger<PackageDeletionService> logger;

    public PackageDeletionService(
        IPackageDatabase packages,
        IPackageStorageService storage,
        IOptionsSnapshot<NaGetOptions> options,
        ILogger<PackageDeletionService> logger)
    {
        this.packages = packages ?? throw new ArgumentNullException(nameof(packages));
        this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
        this.options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> TryDeletePackageAsync(string id, NuGetVersion version, CancellationToken cancellationToken)
    {
        switch (options.PackageDeletionBehavior)
        {
            case PackageDeletionBehavior.Unlist:
                return await TryUnlistPackageAsync(id, version, cancellationToken);

            case PackageDeletionBehavior.HardDelete:
                return await TryHardDeletePackageAsync(id, version, cancellationToken);

            default:
                throw new InvalidOperationException($"Unknown deletion behavior '{options.PackageDeletionBehavior}'");
        }
    }

    private async Task<bool> TryUnlistPackageAsync(string id, NuGetVersion version, CancellationToken cancellationToken)
    {
        logger.LogInformation("Unlisting package {PackageId} {PackageVersion}...", id, version);

        if (!await packages.UnlistPackageAsync(id, version, cancellationToken))
        {
            logger.LogWarning("Could not find package {PackageId} {PackageVersion}", id, version);

            return false;
        }

        logger.LogInformation("Unlisted package {PackageId} {PackageVersion}", id, version);

        return true;
    }

    private async Task<bool> TryHardDeletePackageAsync(string id, NuGetVersion version, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Hard deleting package {PackageId} {PackageVersion} from the database...",
            id,
            version);

        var found = await packages.HardDeletePackageAsync(id, version, cancellationToken);
        if (!found)
        {
            logger.LogWarning(
                "Could not find package {PackageId} {PackageVersion} in the database",
                id,
                version);
        }

        // Delete the package from storage. This is necessary even if the package isn't
        // in the database to ensure that the storage is consistent with the database.
        logger.LogInformation("Hard deleting package {PackageId} {PackageVersion} from storage...",
            id,
            version);

        await storage.DeleteAsync(id, version, cancellationToken);

        logger.LogInformation(
            "Hard deleted package {PackageId} {PackageVersion} from storage",
            id,
            version);

        return found;
    }
}
