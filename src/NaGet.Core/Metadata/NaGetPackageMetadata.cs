namespace NaGet.Core;

using PackageMetadata = NaGet.Protocol.Models.PackageMetadata;

/// <summary>
/// NaGet's extensions to the package metadata model. These additions
/// are not part of the official protocol.
/// </summary>
public class NaGetPackageMetadata : PackageMetadata
{
    [JsonPropertyName("downloads")]
    public long Downloads { get; set; }

    [JsonPropertyName("hasReadme")]
    public bool HasReadme { get; set; }

    [JsonPropertyName("packageTypes")]
    public IReadOnlyList<string> PackageTypes { get; set; } = Array.Empty<string>();

    /// <summary>
    /// The package's release notes.
    /// </summary>
    [JsonPropertyName("releaseNotes")]
    public string ReleaseNotes { get; set; } = string.Empty;

    [JsonPropertyName("repositoryUrl")]
    public string RepositoryUrl { get; set; } = string.Empty;

    [JsonPropertyName("repositoryType")]
    public string RepositoryType { get; set; } = string.Empty;
}
