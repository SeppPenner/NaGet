namespace NaGet.Core;

using PackageMetadata = NaGet.Protocol.Models.PackageMetadata;

/// <summary>
/// The mirroring client for a NuGet server that uses the V3 protocol.
/// </summary>
public class V3UpstreamClient : IUpstreamClient
{
    private readonly NuGetClient client;
    private readonly ILogger<V3UpstreamClient> logger;

    public V3UpstreamClient(NuGetClient client, ILogger<V3UpstreamClient> logger)
    {
        this.client = client ?? throw new ArgumentNullException(nameof(client));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Stream?> DownloadPackageOrNullAsync(
        string id,
        NuGetVersion version,
        CancellationToken cancellationToken)
    {
        try
        {
            using var downloadStream = await client.DownloadPackageAsync(id, version, cancellationToken);

            if (downloadStream is null)
            {
                return null;
            }

            return await downloadStream.AsTemporaryFileStreamAsync(cancellationToken);
        }
        catch (PackageNotFoundException)
        {
            return null;
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to download {PackageId} {PackageVersion} from upstream",
                id,
                version);
            return null;
        }
    }

    public async Task<IReadOnlyList<Package>> ListPackagesAsync(
        string id,
        CancellationToken cancellationToken)
    {
        try
        {
            var packages = await client.GetPackageMetadataAsync(id, cancellationToken);
            return packages.Select(ToPackage).ToList();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to mirror {PackageId}'s upstream metadata", id);
            return new List<Package>();
        }
    }

    public async Task<IReadOnlyList<NuGetVersion>> ListPackageVersionsAsync(
        string id,
        CancellationToken cancellationToken)
    {
        try
        {
            return await client.ListPackageVersionsAsync(id, includeUnlisted: true, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to mirror {PackageId}'s upstream versions", id);
            return new List<NuGetVersion>();
        }
    }

    private Package ToPackage(PackageMetadata metadata)
    {
        var version = metadata.ParseVersion();

        return new Package
        {
            Id = metadata.PackageId,
            Version = version,
            Authors = ParseAuthors(metadata.Authors),
            Description = metadata.Description,
            Downloads = 0,
            HasReadme = false,
            IsPrerelease = version.IsPrerelease,
            Language = metadata.Language,
            Listed = metadata.IsListed(),
            MinClientVersion = metadata.MinClientVersion,
            Published = metadata.Published.UtcDateTime,
            RequireLicenseAcceptance = metadata.RequireLicenseAcceptance,
            Summary = metadata.Summary,
            Title = metadata.Title,
            IconUrl = ParseUri(metadata.IconUrl),
            LicenseUrl = ParseUri(metadata.LicenseUrl),
            ProjectUrl = ParseUri(metadata.ProjectUrl),
            PackageTypes = new List<PackageType>(),
            RepositoryUrl = null,
            RepositoryType = string.Empty,
            SemVerLevel = version.IsSemVer2 ? SemVerLevel.SemVer2 : SemVerLevel.Unknown,
            Tags = metadata.Tags?.ToArray() ?? Array.Empty<string>(),
            Dependencies = ToDependencies(metadata)
        };
    }

    private Uri? ParseUri(string uriString)
    {
        if (uriString is null)
        {
            return null;
        }

        if (!Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
        {
            return null;
        }

        return uri;
    }

    private string[] ParseAuthors(string authors)
    {
        if (string.IsNullOrWhiteSpace(authors)) return Array.Empty<string>();

        return authors
            .Split(new[] { ',', ';', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(a => a.Trim())
            .ToArray();
    }

    private List<PackageDependency> ToDependencies(PackageMetadata package)
    {
        if (package.DependencyGroups.Count == 0)
        {
            return new List<PackageDependency>();
        }

        return package.DependencyGroups
            .SelectMany(ToDependencies)
            .ToList();
    }

    private IEnumerable<PackageDependency> ToDependencies(DependencyGroupItem group)
    {
        // NaGet stores a dependency group with no dependencies as a package dependency
        // with no package id nor package version.
        if ((group.Dependencies?.Count ?? 0) == 0)
        {
            return new[]
            {
                new PackageDependency
                {
                    Id = string.Empty,
                    VersionRange = string.Empty,
                    TargetFramework = group.TargetFramework,
                }
            };
        }

        return group.Dependencies.Select(d => new PackageDependency
        {
            Id = d.Id,
            VersionRange = d.Range,
            TargetFramework = group.TargetFramework,
        });
    }
}
