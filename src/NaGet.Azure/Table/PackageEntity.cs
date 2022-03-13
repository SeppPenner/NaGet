namespace NaGet.Azure;

/// <summary>
/// The Azure Table Storage entity that maps to a <see cref="Package"/>.
/// The <see cref="TableEntity.PartitionKey"/> is the <see cref="Package.Id"/> and
/// the <see cref="TableEntity.RowKey"/> is the <see cref="Package.Version"/>.
/// </summary>
public class PackageEntity : TableEntity, IDownloadCount, IListed
{
    public PackageEntity()
    {
    }

    public string Id { get; set; } = string.Empty;
    public string NormalizedVersion { get; set; } = string.Empty;
    public string OriginalVersion { get; set; } = string.Empty;
    public string Authors { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public long Downloads { get; set; }
    public bool HasReadme { get; set; }
    public bool HasEmbeddedIcon { get; set; }
    public bool IsPrerelease { get; set; }
    public string Language { get; set; } = string.Empty;
    public bool Listed { get; set; }
    public string MinClientVersion { get; set; } = string.Empty;
    public DateTime Published { get; set; }
    public bool RequireLicenseAcceptance { get; set; }
    public int SemVerLevel { get; set; }
    public string ReleaseNotes { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;

    public string IconUrl { get; set; } = string.Empty;
    public string LicenseUrl { get; set; } = string.Empty;
    public string ProjectUrl { get; set; } = string.Empty;

    public string RepositoryUrl { get; set; } = string.Empty;
    public string RepositoryType { get; set; } = string.Empty;

    public string Tags { get; set; } = string.Empty;
    public string Dependencies { get; set; } = string.Empty;
    public string PackageTypes { get; set; } = string.Empty;
    public string TargetFrameworks { get; set; } = string.Empty;
}

/// <summary>
/// A single item in <see cref="PackageEntity.Dependencies"/>.
/// </summary>
public class DependencyModel
{
    public string Id { get; set; } = string.Empty;
    public string VersionRange { get; set; } = string.Empty;
    public string TargetFramework { get; set; } = string.Empty;
}

/// <summary>
/// A single item in <see cref="PackageEntity.PackageTypes"/>.
/// </summary>
public class PackageTypeModel
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}

/// <summary>
/// The Azure Table Storage entity to update the <see cref="Package.Listed"/> column.
/// The <see cref="TableEntity.PartitionKey"/> is the <see cref="Package.Id"/> and
/// the <see cref="TableEntity.RowKey"/> is the <see cref="Package.Version"/>.
/// </summary>
public class PackageListingEntity : TableEntity, IListed
{
    public PackageListingEntity()
    {
    }

    public bool Listed { get; set; }
}

/// <summary>
/// The Azure Table Storage entity to update the <see cref="Package.Downloads"/> column.
/// The <see cref="TableEntity.PartitionKey"/> is the <see cref="Package.Id"/> and
/// the <see cref="TableEntity.RowKey"/> is the <see cref="Package.Version"/>.
/// </summary>
public class PackageDownloadsEntity : TableEntity, IDownloadCount
{
    public PackageDownloadsEntity()
    {
    }

    public long Downloads { get; set; }
}

internal interface IListed
{
    bool Listed { get; set; }
}

public interface IDownloadCount
{
    long Downloads { get; set; }
}
