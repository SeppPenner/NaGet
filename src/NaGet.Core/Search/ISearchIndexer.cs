namespace NaGet.Core;

public interface ISearchIndexer
{
    /// <summary>
    /// Adds a package to the search index.
    /// </summary>
    /// <param name="package">The package to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task Index(Package package, CancellationToken cancellationToken);
}
