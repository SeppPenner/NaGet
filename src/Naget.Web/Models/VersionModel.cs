namespace NaGet.Web.Models;

/// <summary>
/// The version model class.
/// </summary>
public class VersionModel
{
    /// <summary>
    /// Gets or sets the version.
    /// </summary>
    public NuGetVersion? Version { get; set; }

    /// <summary>
    /// Gets or sets the downloads count.
    /// </summary>
    public long Downloads { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the version is selected or not.
    /// </summary>
    public bool Selected { get; set; }

    /// <summary>
    /// Gets or sets the last updated date time.
    /// </summary>
    public DateTime LastUpdated { get; set; }
}
