// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageDownloadsEntity.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The Azure package downloads entity class to update the <see cref="Package.Downloads"/> column.
//    The <see cref="TableEntity.PartitionKey"/> is the <see cref="Package.Id"/> and
//    the <see cref="TableEntity.RowKey"/> is the <see cref="Package.Version"/>.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.Azure.Table;

/// <summary>
/// The Azure package downloads entity class to update the <see cref="Package.Downloads"/> column.
/// The <see cref="TableEntity.PartitionKey"/> is the <see cref="Package.Id"/> and
/// the <see cref="TableEntity.RowKey"/> is the <see cref="Package.Version"/>.
/// </summary>
public class PackageDownloadsEntity : TableEntity, IDownloadCount
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PackageDownloadsEntity"/> class.
    /// </summary>
    public PackageDownloadsEntity()
    {
    }

    /// <summary>
    /// Gets or sets the downloads.
    /// </summary>
    public long Downloads { get; set; }
}
