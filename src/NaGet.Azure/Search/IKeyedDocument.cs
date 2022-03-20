// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IKeyedDocument.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The Azure keyed document interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.Azure.Search;

/// <summary>
/// The Azure keyed document interface.
/// </summary>
public interface IKeyedDocument
{
    /// <summary>
    /// Gets or sets the key.
    /// </summary>
    string Key { get; set; }
}
