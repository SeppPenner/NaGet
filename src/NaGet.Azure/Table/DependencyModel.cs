// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DependencyModel.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The Azure dependency model class.
//    A single item in <see cref="PackageEntity.Dependencies"/>.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.Azure.Table;

/// <summary>
/// The Azure dependency model class.
/// A single item in <see cref="PackageEntity.Dependencies"/>.
/// </summary>
public class DependencyModel
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version range.
    /// </summary>
    public string VersionRange { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target framework.
    /// </summary>
    public string TargetFramework { get; set; } = string.Empty;
}
