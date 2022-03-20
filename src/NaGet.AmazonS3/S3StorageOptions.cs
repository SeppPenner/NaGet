// <copyright file="AliyunStorageOptions.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The Amazon S3 storage options class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.AmazonS3;

/// <summary>
/// The Amazon S3 storage options class.
/// </summary>
public class S3StorageOptions
{
    /// <summary>
    /// Gets or sets the access key.
    /// </summary>
    [RequiredIf(nameof(SecretKey), null, IsInverted = true)]
    public string AccessKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the secret key.
    /// </summary>
    [RequiredIf(nameof(AccessKey), null, IsInverted = true)]
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the region.
    /// </summary>
    [Required]
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the bucket.
    /// </summary>
    [Required]
    public string Bucket { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the prefix.
    /// </summary>
    public string Prefix { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the instance profile is used or not.
    /// </summary>
    public bool UseInstanceProfile { get; set; }

    /// <summary>
    /// Gets or sets the assume ARN role.
    /// </summary>
    public string AssumeRoleArn { get; set; } = string.Empty;
}
