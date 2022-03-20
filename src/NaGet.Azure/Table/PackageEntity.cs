// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageEntity.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The Azure package entity class that maps to a <see cref="Package"/>.
//    The <see cref="TableEntity.PartitionKey"/> is the <see cref="Package.Id"/> and
//    the <see cref="TableEntity.RowKey"/> is the <see cref="Package.Version"/>.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.Azure.Table;

/// <inheritdoc cref="TableEntity"/>
/// <inheritdoc cref="IDownloadCount"/>
/// <inheritdoc cref="IListed"/>
/// <summary>
///The Azure package entity class that maps to a <see cref="Package"/>.
/// The <see cref="TableEntity.PartitionKey"/> is the <see cref="Package.Id"/> and
/// the <see cref="TableEntity.RowKey"/> is the <see cref="Package.Version"/>.
/// </summary>
public class PackageEntity : TableEntity, IDownloadCount, IListed
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PackageEntity"/> class.
    /// </summary>
    public PackageEntity()
    {
    }

    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the normalized version.
    /// </summary>
    public string NormalizedVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the original version.
    /// </summary>
    public string OriginalVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the authors.
    /// </summary>
    public string Authors { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the downloads.
    /// </summary>
    public long Downloads { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the package has a readme or not.
    /// </summary>
    public bool HasReadme { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the package has an embedded icon or not.
    /// </summary>
    public bool HasEmbeddedIcon { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the package is a pre-release or not.
    /// </summary>
    public bool IsPrerelease { get; set; }

    /// <summary>
    /// Gets or sets the language.
    /// </summary>
    public string Language { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the package is a listed or not.
    /// </summary>
    public bool Listed { get; set; }

    /// <summary>
    /// Gets or sets the minimum client version.
    /// </summary>
    public string MinClientVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the published date.
    /// </summary>
    public DateTime Published { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the package requires a license acceptance or not.
    /// </summary>
    public bool RequireLicenseAcceptance { get; set; }

    /// <summary>
    /// Gets or sets the SemVer level.
    /// </summary>
    public int SemVerLevel { get; set; }

    /// <summary>
    /// Gets or sets the release notes.
    /// </summary>
    public string ReleaseNotes { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the summary.
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the icon url.
    /// </summary>
    public string IconUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the license url.
    /// </summary>
    public string LicenseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project url.
    /// </summary>
    public string ProjectUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the repository url.
    /// </summary>
    public string RepositoryUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the repository type.
    /// </summary>
    public string RepositoryType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tags.
    /// </summary>
    public string Tags { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the dependencies.
    /// </summary>
    public string Dependencies { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the package types.
    /// </summary>
    public string PackageTypes { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target frameworks.
    /// </summary>
    public string TargetFrameworks { get; set; } = string.Empty;
}
