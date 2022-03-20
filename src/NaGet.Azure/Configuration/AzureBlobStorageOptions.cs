// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AzureBlobStorageOptions.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The Azure blob storage options class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.Azure.Configuration;

/// <summary>
/// The Azure blob storage options class.
/// NaGet's configurations to use Azure blob storage to store packages.
/// </summary>
public class AzureBlobStorageOptions : IValidatableObject
{
    /// <summary>
    /// Gets or sets the Azure blob storage connection string.
    /// If provided, ignores <see cref="AccountName"/> and <see cref="AccessKey"/>.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Azure blob storage account name. Ignored if <see cref="ConnectionString"/> is provided.
    /// </summary>
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Azure blob storage access key. Ignored if <see cref="ConnectionString"/> is provided.
    /// </summary>        
    public string AccessKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Azure blob storage container name.
    /// </summary>
    public string Container { get; set; } = string.Empty;

    /// <inheritdoc cref="IValidatableObject"/>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
        {
            if (string.IsNullOrWhiteSpace(AccountName))
            {
                yield return new ValidationResult(
                    $"The {nameof(AccountName)} configuration is required.",
                    new[] { nameof(AccountName) });
            }

            if (string.IsNullOrWhiteSpace(AccessKey))
            {
                yield return new ValidationResult(
                    $"The {nameof(AccessKey)} configuration is required.",
                    new[] { nameof(AccessKey) });
            }
        }

        if (string.IsNullOrWhiteSpace(Container))
        {
            yield return new ValidationResult(
                $"The {nameof(Container)} configuration is required.",
                new[] { nameof(Container) });
        }
    }
}
