// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageListingEntity.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The Azure package listing entity class to update the <see cref="Package.Listed"/> column.
//    The <see cref="TableEntity.PartitionKey"/> is the <see cref="Package.Id"/> and
//    the <see cref="TableEntity.RowKey"/> is the <see cref="Package.Version"/>.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.Azure.Table;

/// <summary>
/// The Azure package listing entity class to update the <see cref="Package.Listed"/> column.
/// The <see cref="TableEntity.PartitionKey"/> is the <see cref="Package.Id"/> and
/// the <see cref="TableEntity.RowKey"/> is the <see cref="Package.Version"/>.
/// </summary>
public class PackageListingEntity : TableEntity, IListed
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PackageListingEntity"/>.
    /// </summary>
    public PackageListingEntity()
    {
    }

    /// <summary>
    /// Gets or sets a value indicating whether the package is listed or not.
    /// </summary>
    public bool Listed { get; set; }
}
