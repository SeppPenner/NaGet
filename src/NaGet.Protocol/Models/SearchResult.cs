namespace NaGet.Protocol.Models;

/// <summary>
/// A package that matched a search query.
/// 
/// See https://docs.microsoft.com/en-us/nuget/api/search-query-service-resource#search-result
/// </summary>
public class SearchResult
{
    /// <summary>
    /// The ID of the matched package.
    /// </summary>
    [JsonPropertyName("id")]
    public string PackageId { get; set; } = string.Empty;

    /// <summary>
    /// The latest version of the matched pacakge. This is the full NuGet version after normalization,
    /// including any SemVer 2.0.0 build metadata.
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// The description of the matched package.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The authors of the matched package.
    /// </summary>
    [JsonPropertyName("authors")]
    [JsonConverter(typeof(StringOrStringArrayJsonConverter))]
    public IReadOnlyList<string> Authors { get; set; } = new List<string>();

    /// <summary>
    /// The URL of the matched package's icon.
    /// </summary>
    [JsonPropertyName("iconUrl")]
    public string IconUrl { get; set; } = string.Empty;

    /// <summary>
    /// The URL of the matched package's license.
    /// </summary>
    [JsonPropertyName("licenseUrl")]
    public string LicenseUrl { get; set; } = string.Empty;

    /// <summary>
    /// The package types defined by the package author.
    /// </summary>
    [JsonPropertyName("packageTypes")]
    public IReadOnlyList<SearchResultPackageType> PackageTypes { get; set; } = new List<SearchResultPackageType>();

    /// <summary>
    /// The URL of the matched package's homepage.
    /// </summary>
    [JsonPropertyName("projectUrl")]
    public string ProjectUrl { get; set; } = string.Empty;

    /// <summary>
    /// The URL for the matched package's registration index.
    /// </summary>
    [JsonPropertyName("registration")]
    public string RegistrationIndexUrl { get; set; } = string.Empty;

    /// <summary>
    /// The summary of the matched package.
    /// </summary>
    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// The tags of the matched package.
    /// </summary>
    [JsonPropertyName("tags")]
    public IReadOnlyList<string> Tags { get; set; } = new List<string>();

    /// <summary>
    /// The title of the matched package.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The total downloads for all versions of the matched package.
    /// </summary>
    [JsonPropertyName("totalDownloads")]
    public long TotalDownloads { get; set; }

    /// <summary>
    /// The versions of the matched package.
    /// </summary>
    [JsonPropertyName("versions")]
    public IReadOnlyList<SearchResultVersion> Versions { get; set; } = new List<SearchResultVersion>();
}
