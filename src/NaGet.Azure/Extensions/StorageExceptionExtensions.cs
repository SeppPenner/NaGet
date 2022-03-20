// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StorageExceptionExtensions.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The Azure storage extensions class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.Azure.Extensions;

using StorageException = Microsoft.WindowsAzure.Storage.StorageException;
using TableStorageException = Microsoft.Azure.Cosmos.Table.StorageException;

/// <summary>
/// The Azure storage extensions class.
/// </summary>
internal static class StorageExceptionExtensions
{
    /// <summary>
    /// Checks whether the exception indicates an "already exists" error or not.
    /// </summary>
    /// <param name="e">The storage exception.</param>
    /// <returns>A value indicating whether the exception indicates an "already exists" error or not.</returns>
    public static bool IsAlreadyExistsException(this StorageException e)
    {
        return e?.RequestInformation?.HttpStatusCode == (int?)HttpStatusCode.Conflict;
    }

    /// <summary>
    /// Checks whether the exception indicates a "not found" error or not.
    /// </summary>
    /// <param name="e">The storage exception.</param>
    /// <returns>A value indicating whether the exception indicates a "not found" error or not.</returns>
    public static bool IsNotFoundException(this TableStorageException e)
    {
        return e?.RequestInformation?.HttpStatusCode == (int?)HttpStatusCode.NotFound;
    }

    /// <summary>
    /// Checks whether the exception indicates a "conflict" error or not.
    /// </summary>
    /// <param name="e">The storage exception.</param>
    /// <returns>A value indicating whether the exception indicates a "conflict" error or not.</returns>
    public static bool IsAlreadyExistsException(this TableStorageException e)
    {
        return e?.RequestInformation?.HttpStatusCode == (int?)HttpStatusCode.Conflict;
    }

    /// <summary>
    /// Checks whether the exception indicates a "precondition failed" error or not.
    /// </summary>
    /// <param name="e">The storage exception.</param>
    /// <returns>A value indicating whether the exception indicates a "precondition failed" error or not.</returns>
    public static bool IsPreconditionFailedException(this TableStorageException e)
    {
        return e?.RequestInformation?.HttpStatusCode == (int?)HttpStatusCode.PreconditionFailed;
    }
}
