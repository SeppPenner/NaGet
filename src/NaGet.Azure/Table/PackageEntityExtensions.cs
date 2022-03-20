// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageEntityExtensions.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The Azure package entity extensions class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.Azure.Table;

/// <summary>
/// The Azure package entity extensions class.
/// </summary>
public static class PackageEntityExtensions
{
    /// <summary>
    /// Converts the <see cref="PackageEntity"/> to a <see cref="Package"/>.
    /// </summary>
    /// <param name="entity">The <see cref="PackageEntity"/>.</param>
    /// <returns>The <see cref="Package"/>.</returns>
    public static Package AsPackage(this PackageEntity entity)
    {
        return new Package
        {
            Id = entity.Id,
            NormalizedVersionString = entity.NormalizedVersion,
            OriginalVersionString = entity.OriginalVersion,
            Authors = JsonSerializer.Deserialize<string[]>(entity.Authors) ?? Array.Empty<string>(),
            Description = entity.Description,
            Downloads = entity.Downloads,
            HasReadme = entity.HasReadme,
            HasEmbeddedIcon = entity.HasEmbeddedIcon,
            IsPrerelease = entity.IsPrerelease,
            Language = entity.Language,
            Listed = entity.Listed,
            MinClientVersion = entity.MinClientVersion,
            Published = entity.Published,
            RequireLicenseAcceptance = entity.RequireLicenseAcceptance,
            SemVerLevel = (SemVerLevel)entity.SemVerLevel,
            Summary = entity.Summary,
            Title = entity.Title,
            ReleaseNotes = entity.ReleaseNotes,
            IconUrl = ParseUri(entity.IconUrl),
            LicenseUrl = ParseUri(entity.LicenseUrl),
            ProjectUrl = ParseUri(entity.ProjectUrl),
            RepositoryUrl = ParseUri(entity.RepositoryUrl),
            RepositoryType = entity.RepositoryType,
            Tags = JsonSerializer.Deserialize<string[]>(entity.Tags) ?? Array.Empty<string>(),
            Dependencies = ParseDependencies(entity.Dependencies),
            PackageTypes = ParsePackageTypes(entity.PackageTypes),
            TargetFrameworks = ParseTargetFrameworks(entity.TargetFrameworks),
        };
    }

    /// <summary>
    /// Parses the uri.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns>The parsed <see cref="Uri"/>.</returns>
    private static Uri? ParseUri(string? input)
    {
        return string.IsNullOrWhiteSpace(input) ? null : new Uri(input);
    }

    /// <summary>
    /// Parses the package dependencies.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns>A <see cref="List{T}"/> of <see cref="PackageDependency"/>s.</returns>
    private static List<PackageDependency> ParseDependencies(string input)
    {
        var dependencies = JsonSerializer.Deserialize<List<DependencyModel>>(input) ?? new List<DependencyModel>();

        return dependencies
            .Select(e => new PackageDependency
            {
                Id = e.Id,
                VersionRange = e.VersionRange,
                TargetFramework = e.TargetFramework,
            })
            .ToList();
    }

    /// <summary>
    /// Parses the package types.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns>A <see cref="List{T}"/> of <see cref="PackageType"/>s.</returns>
    private static List<PackageType> ParsePackageTypes(string input)
    {
        var packages = JsonSerializer.Deserialize<List<PackageTypeModel>>(input) ?? new List<PackageTypeModel>();

        return packages
            .Select(e => new PackageType
            {
                Name = e.Name,
                Version = e.Version
            })
            .ToList();
    }

    /// <summary>
    /// Parses the target frameworks.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns>A <see cref="List{T}"/> of <see cref="TargetFramework"/>s.</returns>
    private static List<TargetFramework> ParseTargetFrameworks(string targetFrameworks)
    {
        var targetFrameworkList = JsonSerializer.Deserialize<List<string>>(targetFrameworks) ?? new List<string>();

        return targetFrameworkList
            .Select(f => new TargetFramework { Moniker = f })
            .ToList();
    }
}
