// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TableOperationBuilder.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The table operation builder class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.Azure.Table;

/// <summary>
/// The table operation builder class.
/// </summary>
public class TableOperationBuilder
{
    /// <summary>
    /// Adds the package.
    /// </summary>
    /// <param name="package">The package.</param>
    /// <returns>A <see cref="TableOperation"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the package is null.</exception>
    public TableOperation AddPackage(Package package)
    {
        if (package is null)
        {
            throw new ArgumentNullException(nameof(package));
        }

        var version = package.Version;
        var normalizedVersion = version.ToNormalizedString();

        var entity = new PackageEntity
        {
            PartitionKey = package.Id.ToLowerInvariant(),
            RowKey = normalizedVersion.ToLowerInvariant(),

            Id = package.Id,
            NormalizedVersion = normalizedVersion,
            OriginalVersion = version.ToFullString(),
            Authors = JsonSerializer.Serialize(package.Authors),
            Description = package.Description,
            Downloads = package.Downloads,
            HasReadme = package.HasReadme,
            HasEmbeddedIcon = package.HasEmbeddedIcon,
            IsPrerelease = package.IsPrerelease,
            Language = package.Language,
            Listed = package.Listed,
            MinClientVersion = package.MinClientVersion,
            Published = package.Published,
            RequireLicenseAcceptance = package.RequireLicenseAcceptance,
            SemVerLevel = (int)package.SemVerLevel,
            Summary = package.Summary,
            Title = package.Title,
            IconUrl = package.IconUrlString,
            LicenseUrl = package.LicenseUrlString,
            ReleaseNotes = package.ReleaseNotes,
            ProjectUrl = package.ProjectUrlString,
            RepositoryUrl = package.RepositoryUrlString,
            RepositoryType = package.RepositoryType,
            Tags = JsonSerializer.Serialize(package.Tags),
            Dependencies = SerializeList(package.Dependencies, AsDependencyModel),
            PackageTypes = SerializeList(package.PackageTypes, AsPackageTypeModel),
            TargetFrameworks = SerializeList(package.TargetFrameworks, f => f.Moniker)
        };

        return TableOperation.Insert(entity);
    }

    /// <summary>
    /// Updates the downloads.
    /// </summary>
    /// <param name="packageId">The package identifier.</param>
    /// <param name="packageVersion">The package version.</param>
    /// <param name="downloads">The downloads.</param>
    /// <returns>A <see cref="TableOperation"/>.</returns>
    public TableOperation UpdateDownloads(string packageId, NuGetVersion packageVersion, long downloads)
    {
        var entity = new PackageDownloadsEntity
        {
            PartitionKey = packageId.ToLowerInvariant(),
            RowKey = packageVersion.ToNormalizedString().ToLowerInvariant(),
            Downloads = downloads,
            ETag = "*"
        };

        return TableOperation.Merge(entity);
    }

    /// <summary>
    /// Deletes the package.
    /// </summary>
    /// <param name="packageId">The package identifier.</param>
    /// <param name="packageVersion">The package version.</param>
    /// <returns>A <see cref="TableOperation"/>.</returns>
    public TableOperation HardDeletePackage(string packageId, NuGetVersion packageVersion)
    {
        var entity = new PackageEntity
        {
            PartitionKey = packageId.ToLowerInvariant(),
            RowKey = packageVersion.ToNormalizedString().ToLowerInvariant(),
            ETag = "*"
        };

        return TableOperation.Delete(entity);
    }

    /// <summary>
    /// Unkists the package.
    /// </summary>
    /// <param name="packageId">The package identifier.</param>
    /// <param name="packageVersion">The package version.</param>
    /// <returns>A <see cref="TableOperation"/>.</returns>
    public TableOperation UnlistPackage(string packageId, NuGetVersion packageVersion)
    {
        var entity = new PackageListingEntity
        {
            PartitionKey = packageId.ToLowerInvariant(),
            RowKey = packageVersion.ToNormalizedString().ToLowerInvariant(),
            Listed = false,
            ETag = "*"
        };

        return TableOperation.Merge(entity);
    }

    /// <summary>
    /// Relists the package.
    /// </summary>
    /// <param name="packageId">The package identifier.</param>
    /// <param name="packageVersion">The package version.</param>
    /// <returns>A <see cref="TableOperation"/>.</returns>
    public TableOperation RelistPackage(string packageId, NuGetVersion packageVersion)
    {
        var entity = new PackageListingEntity
        {
            PartitionKey = packageId.ToLowerInvariant(),
            RowKey = packageVersion.ToNormalizedString().ToLowerInvariant(),
            Listed = true,
            ETag = "*"
        };

        return TableOperation.Merge(entity);
    }

    /// <summary>
    /// Serializes the list.
    /// </summary>
    /// <typeparam name="TIn">The in parameter.</typeparam>
    /// <typeparam name="TOut">The out parameter.</typeparam>
    /// <param name="objects">The objects.</param>
    /// <param name="map">The map.</param>
    /// <returns>The serialized list.</returns>
    private static string SerializeList<TIn, TOut>(IReadOnlyList<TIn> objects, Func<TIn, TOut> map)
    {
        var data = objects.Select(map).ToList();
        return JsonSerializer.Serialize(data);
    }

    /// <summary>
    /// Converts the <see cref="PackageDependency"/> to a <see cref="DependencyModel"/>.
    /// </summary>
    /// <param name="packageDependency">The package dependency.</param>
    /// <returns>A <see cref="DependencyModel"/>.</returns>
    public static DependencyModel AsDependencyModel(PackageDependency packageDependency)
    {
        return new DependencyModel
        {
            Id = packageDependency.Id,
            VersionRange = packageDependency.VersionRange,
            TargetFramework = packageDependency.TargetFramework
        };
    }

    /// <summary>
    /// Converts the <see cref="PackageType"/> to a <see cref="PackageTypeModel"/>.
    /// </summary>
    /// <param name="packageType">The package type.</param>
    /// <returns>A <see cref="PackageTypeModel"/>.</returns>
    public static PackageTypeModel AsPackageTypeModel(PackageType packageType)
    {
        return new PackageTypeModel
        {
            Name = packageType.Name,
            Version = packageType.Version
        };
    }
}
