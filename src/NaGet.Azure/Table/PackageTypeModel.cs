// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DependencyModel.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The Azure package type model class.
//    A single item in <see cref="PackageEntity.PackageTypes"/>.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace NaGet.Azure.Table;

/// <summary>
/// The Azure package type model class.
/// A single item in <see cref="PackageEntity.PackageTypes"/>.
/// </summary>
public class PackageTypeModel
{
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version.
    /// </summary>
    public string Version { get; set; } = string.Empty;
}
