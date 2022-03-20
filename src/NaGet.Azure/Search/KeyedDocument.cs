// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KeyedDocument.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The Azure keyed document class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.Azure.Search;

/// <inheritdoc cref="IKeyedDocument"/>
/// <summary>
/// The Azure keyed document class.
/// </summary>
[SerializePropertyNamesAsCamelCase]
public class KeyedDocument : IKeyedDocument
{
    /// <inheritdoc cref="IKeyedDocument"/>
    [Key]
    public string Key { get; set; } = string.Empty;
}
