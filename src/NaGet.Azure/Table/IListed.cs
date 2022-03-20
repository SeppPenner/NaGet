// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IListed.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The Azure IListed interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.Azure.Table;

/// <summary>
/// The Azure IListed interface.
/// </summary>
internal interface IListed
{
    /// <summary>
    /// Gets or sets a value indicating whether the item is listed or not.
    /// </summary>
    bool Listed { get; set; }
}
