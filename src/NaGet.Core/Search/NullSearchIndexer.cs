namespace NaGet.Core;

/// <summary>
/// A no-op indexer, used when search does not need to index packages.
/// </summary>
public class NullSearchIndexer : ISearchIndexer
{
    public Task Index(Package package, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
