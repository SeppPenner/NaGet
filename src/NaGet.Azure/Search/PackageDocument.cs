namespace NaGet.Azure;

// See: https://docs.microsoft.com/en-us/nuget/api/search-query-service-resource#search-for-packages
[SerializePropertyNamesAsCamelCase]
public class PackageDocument : KeyedDocument
{
    public const string IndexName = "packages";

    [IsSearchable, IsFilterable, IsSortable]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The package's full versions after normalization, including any SemVer 2.0.0 build metadata.
    /// </summary>
    [IsSearchable, IsFilterable, IsSortable]
    public string Version { get; set; } = string.Empty;

    [IsSearchable]
    public string Description { get; set; } = string.Empty;
    public string[] Authors { get; set; } = Array.Empty<string>();
    public bool HasEmbeddedIcon { get; set; }
    public string IconUrl { get; set; } = string.Empty;
    public string LicenseUrl { get; set; } = string.Empty;
    public string ProjectUrl { get; set; } = string.Empty;
    public DateTimeOffset Published { get; set; }

    [IsSearchable]
    public string Summary { get; set; } = string.Empty;

    [IsSearchable, IsFilterable, IsFacetable]
    public string[] Tags { get; set; } = Array.Empty<string>();

    [IsSearchable]
    public string Title { get; set; } = string.Empty;

    [IsFilterable, IsSortable]
    public long TotalDownloads { get; set; }

    [IsFilterable, IsSortable]
    public int DownloadsMagnitude { get; set; }

    /// <summary>
    /// The package's full versions after normalization, including any SemVer 2.0.0 build metadata.
    /// </summary>
    public string[] Versions { get; set; } = Array.Empty<string>();
    public string[] VersionDownloads { get; set; } = Array.Empty<string>();

    [IsSearchable]
    [Analyzer(ExactMatchCustomAnalyzer.Name)]
    public string[] Dependencies { get; set; } = Array.Empty<string>();

    [IsSearchable]
    [Analyzer(ExactMatchCustomAnalyzer.Name)]
    public string[] PackageTypes { get; set; } = Array.Empty<string>();

    [IsSearchable]
    [Analyzer(ExactMatchCustomAnalyzer.Name)]
    public string[] Frameworks { get; set; } = Array.Empty<string>();

    [IsFilterable]
    public string SearchFilters { get; set; } = string.Empty;
}

[SerializePropertyNamesAsCamelCase]
public class KeyedDocument : IKeyedDocument
{
    [Key]
    public string Key { get; set; } = string.Empty;
}

public interface IKeyedDocument
{
    string Key { get; set; }
}
