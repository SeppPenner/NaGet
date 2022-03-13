namespace NaGet.Core;

public class PackageIndexingService : IPackageIndexingService
{
    private readonly IPackageDatabase packages;
    private readonly IPackageStorageService storage;
    private readonly ISearchIndexer search;
    private readonly SystemTime time;
    private readonly IOptionsSnapshot<NaGetOptions> options;
    private readonly ILogger<PackageIndexingService> logger;

    public PackageIndexingService(
        IPackageDatabase packages,
        IPackageStorageService storage,
        ISearchIndexer search,
        SystemTime time,
        IOptionsSnapshot<NaGetOptions> options,
        ILogger<PackageIndexingService> logger)
    {
        this.packages = packages ?? throw new ArgumentNullException(nameof(packages));
        this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
        this.search = search ?? throw new ArgumentNullException(nameof(search));
        this.time = time ?? throw new ArgumentNullException(nameof(time));
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PackageIndexingResult> IndexAsync(Stream packageStream, CancellationToken cancellationToken)
    {
        // Try to extract all the necessary information from the package.
        Package package;
        Stream nuspecStream;
        Stream readmeStream;
        Stream iconStream;

        try
        {
            using var packageReader = new PackageArchiveReader(packageStream, leaveStreamOpen: true);
            package = packageReader.GetPackageMetadata();
            package.Published = time.UtcNow;

            nuspecStream = await packageReader.GetNuspecAsync(cancellationToken);
            nuspecStream = await nuspecStream.AsTemporaryFileStreamAsync(cancellationToken);

            if (package.HasReadme)
            {
                readmeStream = await packageReader.GetReadmeAsync(cancellationToken);
                readmeStream = await readmeStream.AsTemporaryFileStreamAsync(cancellationToken);
            }
            else
            {
                readmeStream = null;
            }

            if (package.HasEmbeddedIcon)
            {
                iconStream = await packageReader.GetIconAsync(cancellationToken);
                iconStream = await iconStream.AsTemporaryFileStreamAsync(cancellationToken);
            }
            else
            {
                iconStream = null;
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Uploaded package is invalid");

            return PackageIndexingResult.InvalidPackage;
        }

        // The package is well-formed. Ensure this is a new package.
        if (await packages.ExistsAsync(package.Id, package.Version, cancellationToken))
        {
            if (!options.Value.AllowPackageOverwrites)
            {
                return PackageIndexingResult.PackageAlreadyExists;
            }

            await packages.HardDeletePackageAsync(package.Id, package.Version, cancellationToken);
            await storage.DeleteAsync(package.Id, package.Version, cancellationToken);
        }

        // TODO: Add more package validations
        // TODO: Call PackageArchiveReader.ValidatePackageEntriesAsync
        logger.LogInformation(
            "Validated package {PackageId} {PackageVersion}, persisting content to storage...",
            package.Id,
            package.NormalizedVersionString);

        try
        {
            packageStream.Position = 0;

            await storage.SavePackageContentAsync(
                package,
                packageStream,
                nuspecStream,
                readmeStream,
                iconStream,
                cancellationToken);
        }
        catch (Exception e)
        {
            // This may happen due to concurrent pushes.
            // TODO: Make IPackageStorageService.SavePackageContentAsync return a result enum so this
            // can be properly handled.
            logger.LogError(
                e,
                "Failed to persist package {PackageId} {PackageVersion} content to storage",
                package.Id,
                package.NormalizedVersionString);

            throw;
        }

        logger.LogInformation(
            "Persisted package {Id} {Version} content to storage, saving metadata to database...",
            package.Id,
            package.NormalizedVersionString);

        var result = await packages.AddAsync(package, cancellationToken);
        if (result == PackageAddResult.PackageAlreadyExists)
        {
            logger.LogWarning(
                "Package {Id} {Version} metadata already exists in database",
                package.Id,
                package.NormalizedVersionString);

            return PackageIndexingResult.PackageAlreadyExists;
        }

        if (result != PackageAddResult.Success)
        {
            logger.LogError($"Unknown {nameof(PackageAddResult)} value: {{PackageAddResult}}", result);

            throw new InvalidOperationException($"Unknown {nameof(PackageAddResult)} value: {result}");
        }

        logger.LogInformation(
            "Successfully persisted package {Id} {Version} metadata to database. Indexing in search...",
            package.Id,
            package.NormalizedVersionString);

        await search.IndexAsync(package, cancellationToken);

        logger.LogInformation(
            "Successfully indexed package {Id} {Version} in search",
            package.Id,
            package.NormalizedVersionString);

        return PackageIndexingResult.Success;
    }
}
