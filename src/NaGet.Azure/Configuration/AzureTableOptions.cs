// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AzureTableOptions.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The Azure table options class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.Azure.Configuration;

/// <summary>
/// The Azure table options class.
/// </summary>
public class AzureTableOptions
{
    /// <summary>
    /// Gets or sets the connection string.
    /// </summary>
    [Required]
    public string ConnectionString { get; set; } = string.Empty;
}
