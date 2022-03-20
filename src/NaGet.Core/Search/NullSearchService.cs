namespace NaGet.Core;

/// <summary>
/// A minimal search service implementation, used for advanced scenarios.
/// </summary>
public class NullSearchService : ISearchService
{
    private static readonly IReadOnlyList<string> EmptyStringList = new List<string>();

    private static readonly Task<AutocompleteResponse> EmptyAutocompleteResponseTask =
        Task.FromResult(new AutocompleteResponse
        {
            TotalHits = 0,
            Data = EmptyStringList,
            Context = AutocompleteContext.Default
        });

    private static readonly Task<DependentsResponse> EmptyDependentsResponseTask =
        Task.FromResult(new DependentsResponse
        {
            TotalHits = 0,
            Data = new List<PackageDependent>()
        });

    private static readonly Task<SearchResponse> EmptySearchResponseTask =
        Task.FromResult(new SearchResponse
        {
            TotalHits = 0,
            Data = new List<SearchResult>()
        });

    public Task<AutocompleteResponse> Autocomplete(
        AutocompleteRequest request,
        CancellationToken cancellationToken)
    {
        return EmptyAutocompleteResponseTask;
    }

    public Task<AutocompleteResponse> ListPackageVersions(
        VersionsRequest request,
        CancellationToken cancellationToken)
    {
        return EmptyAutocompleteResponseTask;
    }

    public Task<DependentsResponse> FindDependents(string packageId, CancellationToken cancellationToken)
    {
        return EmptyDependentsResponseTask;
    }

    public Task<SearchResponse> Search(
        SearchRequest request,
        CancellationToken cancellationToken)
    {
        return EmptySearchResponseTask;
    }
}
