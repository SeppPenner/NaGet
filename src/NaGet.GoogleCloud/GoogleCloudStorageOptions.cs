// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GoogleCloudStorageOptions.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The Google cloud storage options class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.GoogleCloud;

/// <inheritdoc cref="StorageOptions"/>
/// <summary>
/// The Google cloud storage options class.
/// </summary>
public class GoogleCloudStorageOptions : StorageOptions
{
    /// <summary>
    /// Gets or sets the bucket name.
    /// </summary>
    [Required]
    public string BucketName { get; set; } = string.Empty;
}
