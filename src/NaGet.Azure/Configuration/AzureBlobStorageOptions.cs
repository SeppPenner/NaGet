namespace NaGet.Azure;

/// <summary>
/// NaGet's configurations to use Azure Blob Storage to store packages.
/// See: https://loic-sharma.github.io/NaGet/quickstart/azure/#azure-blob-storage
/// </summary>
public class AzureBlobStorageOptions : IValidatableObject
{
    /// <summary>
    /// The Azure Blob Storage connection string.
    /// If provided, ignores <see cref="AccountName"/> and <see cref="AccessKey"/>.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// The Azure Blob Storage account name. Ignored if <see cref="ConnectionString"/> is provided.
    /// </summary>
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// The Azure Blob Storage access key. Ignored if <see cref="ConnectionString"/> is provided.
    /// </summary>        
    public string AccessKey { get; set; } = string.Empty;

    /// <summary>
    /// The Azure Blob Storage container name.
    /// </summary>
    public string Container { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        const string helpUrl = "https://loic-sharma.github.io/NaGet/quickstart/azure/#azure-blob-storage";

        if (string.IsNullOrWhiteSpace(ConnectionString))
        {
            if (string.IsNullOrWhiteSpace(AccountName))
            {
                yield return new ValidationResult(
                    $"The {nameof(AccountName)} configuration is required. See {helpUrl}",
                    new[] { nameof(AccountName) });
            }

            if (string.IsNullOrWhiteSpace(AccessKey))
            {
                yield return new ValidationResult(
                    $"The {nameof(AccessKey)} configuration is required. See {helpUrl}",
                    new[] { nameof(AccessKey) });
            }
        }

        if (string.IsNullOrWhiteSpace(Container))
        {
            yield return new ValidationResult(
                $"The {nameof(Container)} configuration is required. See {helpUrl}",
                new[] { nameof(Container) });
        }
    }
}
