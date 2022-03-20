// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AliyunStorageOptions.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The Azure search options class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.Azure.Configuration;

/// <summary>
/// The Azure search options class.
/// </summary>
public class AzureSearchOptions
{
    /// <summary>
    /// Gets or sets the account name.
    /// </summary>
    [Required]
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the API key.
    /// </summary>
    [Required]
    public string ApiKey { get; set; } = string.Empty;
}
