namespace NaGet.Core;

using ILogger = ILogger<V2UpstreamClient>;
using INuGetLogger = NuGet.Common.ILogger;

/// <summary>
/// The client to upstream a NuGet server that uses the V2 protocol.
/// </summary>
public class V2UpstreamClient : IUpstreamClient, IDisposable
{
    private readonly SourceCacheContext cache;
    private readonly SourceRepository repository;
    private readonly INuGetLogger ngLogger;
    private readonly ILogger logger;

    public V2UpstreamClient(
        IOptionsSnapshot<MirrorOptions> options,
        ILogger logger)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (options.Value?.PackageSource?.AbsolutePath is null)
        {
            throw new ArgumentException("No mirror package source has been set.");
        }

        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ngLogger = NullLogger.Instance;
        cache = new SourceCacheContext();
        repository = Repository.Factory.GetCoreV2(new PackageSource(options.Value.PackageSource.AbsoluteUri));
    }

    public async Task<IReadOnlyList<NuGetVersion>> ListPackageVersionsAsync(string id, CancellationToken cancellationToken)
    {
        try
        {
            var resource = await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);
            var versions = await resource.GetAllVersionsAsync(id, cache, ngLogger, cancellationToken);

            return versions.ToList();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to mirror {PackageId}'s upstream versions", id);
            return new List<NuGetVersion>();
        }
    }

    public async Task<IReadOnlyList<Package>> ListPackagesAsync(
        string id,
        CancellationToken cancellationToken)
    {
        try
        {
            var resource = await repository.GetResourceAsync<PackageMetadataResource>(cancellationToken);
            var packages = await resource.GetMetadataAsync(
                id,
                includePrerelease: true,
                includeUnlisted: true,
                cache,
                ngLogger,
                cancellationToken);

            return packages.Select(ToPackage).ToList();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to mirror {PackageId}'s upstream versions", id);
            return new List<Package>();
        }
    }

    public async Task<Stream?> DownloadPackageOrNullAsync(
        string id,
        NuGetVersion version,
        CancellationToken cancellationToken)
    {
        var packageStream = new MemoryStream();

        try
        {
            var resource = await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);
            var success = await resource.CopyNupkgToStreamAsync(
                id, version, packageStream, cache, ngLogger,
                cancellationToken);

            if (!success)
            {
                packageStream.Dispose();
                return null;
            }

            packageStream.Position = 0;

            return packageStream;
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to index package {Id} {Version} from upstream",
                id,
                version);

            packageStream.Dispose();
            return null;
        }
    }

    public void Dispose() => cache.Dispose();

    private Package ToPackage(IPackageSearchMetadata package)
    {
        return new Package
        {
            Id = package.Identity.Id,
            Version = package.Identity.Version,
            Authors = ParseAuthors(package.Authors),
            Description = package.Description,
            Downloads = 0,
            HasReadme = false,
            Language = string.Empty,
            Listed = package.IsListed,
            MinClientVersion = string.Empty,
            Published = package.Published?.UtcDateTime ?? DateTime.MinValue,
            RequireLicenseAcceptance = package.RequireLicenseAcceptance,
            Summary = package.Summary,
            Title = package.Title,
            IconUrl = package.IconUrl,
            LicenseUrl = package.LicenseUrl,
            ProjectUrl = package.ProjectUrl,
            PackageTypes = new List<PackageType>(),
            RepositoryUrl = null,
            RepositoryType = string.Empty,
            Tags = package.Tags?.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>(),
            Dependencies = ToDependencies(package)
        };
    }

    private string[] ParseAuthors(string authors)
    {
        if (string.IsNullOrWhiteSpace(authors)) return Array.Empty<string>();

        return authors
            .Split(new[] { ',', ';', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(a => a.Trim())
            .ToArray();
    }

    private List<PackageDependency> ToDependencies(IPackageSearchMetadata package)
    {
        return package
            .DependencySets
            .SelectMany(ToDependencies)
            .ToList();
    }

    private IEnumerable<PackageDependency> ToDependencies(PackageDependencyGroup group)
    {
        var framework = group.TargetFramework.GetShortFolderName();

        // NaGet stores a dependency group with no dependencies as a package dependency
        // with no package id nor package version.
        if (group.Packages.Count() == 0)
        {
            return new[]
            {
                new PackageDependency
                {
                    Id = string.Empty,
                    VersionRange = string.Empty,
                    TargetFramework = framework,
                }
            };
        }

        return group.Packages.Select(d => new PackageDependency
        {
            Id = d.Id,
            VersionRange = d.VersionRange?.ToString() ?? string.Empty,
            TargetFramework = framework,
        });
    }
}
