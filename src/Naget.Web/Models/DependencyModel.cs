namespace NaGet.Web.Models;

/// <summary>
/// The dependency model class.
/// </summary>
public class DependencyModel
{
    /// <summary>
    /// Gets or sets the package identifier.
    /// </summary>
    public string PackageId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version specification.
    /// </summary>
    public string VersionSpec { get; set; } = string.Empty;
}
