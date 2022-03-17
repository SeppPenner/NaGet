namespace NaGet.Core;

// See NuGetGallery's: https://github.com/NuGet/NuGetGallery/blob/master/src/NuGetGallery.Core/Entities/Package.cs
public class Package
{
    public int Key { get; set; }

    public string Id { get; set; } = string.Empty;

    public NuGetVersion Version
    {
        get
        {
            // Favor the original version string as it contains more information.
            // Packages uploaded with older versions of NaGet may not have the original version string.
            return NuGetVersion.Parse(OriginalVersionString ?? NormalizedVersionString);
        }

        set
        {
            NormalizedVersionString = value.ToNormalizedString().ToLowerInvariant();
            OriginalVersionString = value.OriginalVersion;
        }
    }

    public string[] Authors { get; set; } = Array.Empty<string>();
    public string Description { get; set; } = string.Empty;
    public long Downloads { get; set; }
    public bool HasReadme { get; set; }
    public bool HasEmbeddedIcon { get; set; }
    public bool IsPrerelease { get; set; }
    public string ReleaseNotes { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public bool Listed { get; set; }
    public string MinClientVersion { get; set; } = string.Empty;
    public DateTime Published { get; set; }
    public bool RequireLicenseAcceptance { get; set; }
    public SemVerLevel SemVerLevel { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;

    public Uri IconUrl { get; set; }
    public Uri LicenseUrl { get; set; }
    public Uri ProjectUrl { get; set; }

    public Uri RepositoryUrl { get; set; }
    public string RepositoryType { get; set; } = string.Empty;

    public string[] Tags { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Used for optimistic concurrency.
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public List<PackageDependency> Dependencies { get; set; } = new();
    public List<PackageType> PackageTypes { get; set; } = new();
    public List<TargetFramework> TargetFrameworks { get; set; } = new();

    public string NormalizedVersionString { get; set; } = string.Empty;
    public string OriginalVersionString { get; set; } = string.Empty;


    public string IconUrlString => IconUrl?.AbsoluteUri ?? string.Empty;
    public string LicenseUrlString => LicenseUrl?.AbsoluteUri ?? string.Empty;
    public string ProjectUrlString => ProjectUrl?.AbsoluteUri ?? string.Empty;
    public string RepositoryUrlString => RepositoryUrl?.AbsoluteUri ?? string.Empty;
}
