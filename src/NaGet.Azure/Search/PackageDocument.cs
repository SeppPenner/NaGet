// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageDocument.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The Azure package document class. See: https://docs.microsoft.com/en-us/nuget/api/search-query-service-resource#search-for-packages.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.Azure.Search;

/// <inheritdoc cref="KeyedDocument"/>
/// <summary>
/// The Azure package document class. See: https://docs.microsoft.com/en-us/nuget/api/search-query-service-resource#search-for-packages.
/// </summary>
[SerializePropertyNamesAsCamelCase]
public class PackageDocument : KeyedDocument
{
    /// <summary>
    /// The index name.
    /// </summary>
    public const string IndexName = "packages";

    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    [IsSearchable, IsFilterable, IsSortable]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the package's full version after normalization, including any SemVer 2.0.0 build metadata.
    /// </summary>
    [IsSearchable, IsFilterable, IsSortable]
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    [IsSearchable]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the authors.
    /// </summary>
    public string[] Authors { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets a value indicating whether the package has an embedded icon or not.
    /// </summary>
    public bool HasEmbeddedIcon { get; set; }

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
    /// Gets or sets the pubkish date.
    /// </summary>
    public DateTimeOffset Published { get; set; }

    /// <summary>
    /// Gets or sets the summary.
    /// </summary>
    [IsSearchable]
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tags.
    /// </summary>
    [IsSearchable, IsFilterable, IsFacetable]
    public string[] Tags { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    [IsSearchable]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total downloads.
    /// </summary>
    [IsFilterable, IsSortable]
    public long TotalDownloads { get; set; }

    /// <summary>
    /// Gets or sets the downloads magnitude.
    /// </summary>
    [IsFilterable, IsSortable]
    public int DownloadsMagnitude { get; set; }

    /// <summary>
    /// Gets or sets the package's full versions after normalization, including any SemVer 2.0.0 build metadata.
    /// </summary>
    public string[] Versions { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the version downloads.
    /// </summary>
    public string[] VersionDownloads { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the dependencies.
    /// </summary>
    [IsSearchable]
    [Analyzer(ExactMatchCustomAnalyzer.Name)]
    public string[] Dependencies { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the package types.
    /// </summary>
    [IsSearchable]
    [Analyzer(ExactMatchCustomAnalyzer.Name)]
    public string[] PackageTypes { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the frameworks.
    /// </summary>
    [IsSearchable]
    [Analyzer(ExactMatchCustomAnalyzer.Name)]
    public string[] Frameworks { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the search filters.
    /// </summary>
    [IsFilterable]
    public string SearchFilters { get; set; } = string.Empty;
}
