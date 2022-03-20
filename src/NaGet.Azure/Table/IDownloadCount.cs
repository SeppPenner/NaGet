// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDownloadCount.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The Azure IDownloadCount interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.Azure.Table;

/// <summary>
/// The Azure IDownloadCount interface.
/// </summary>
public interface IDownloadCount
{
    /// <summary>
    /// Gets or sets the downloads.
    /// </summary>
    long Downloads { get; set; }
}
