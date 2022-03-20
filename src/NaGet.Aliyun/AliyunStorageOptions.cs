// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AliyunStorageOptions.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The Aliyun storage options class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.Aliyun;

/// <summary>
/// The Aliyun storage options class.
/// </summary>
public class AliyunStorageOptions
{
    /// <summary>
    /// Gets or sets the access key.
    /// </summary>
    [Required]
    public string AccessKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the access key secret.
    /// </summary>
    [Required]
    public string AccessKeySecret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the endpoint.
    /// </summary>
    [Required]
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the bucket.
    /// </summary>
    [Required]
    public string Bucket { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the prefix.
    /// </summary>
    public string Prefix { get; set; } = string.Empty;
}
