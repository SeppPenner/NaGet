namespace NaGet.Web;

public class SearchController : Controller
{
    private readonly ISearchService searchService;

    public SearchController(ISearchService searchService)
    {
        this.searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
    }

    public async Task<ActionResult<SearchResponse>> SearchAsync(
        [FromQuery(Name = "q")] string? query = null,
        [FromQuery]int skip = 0,
        [FromQuery]int take = 20,
        [FromQuery]bool prerelease = false,
        [FromQuery]string? semVerLevel = null,

        // These are unofficial parameters
        [FromQuery]string? packageType = null,
        [FromQuery]string? framework = null,
        CancellationToken cancellationToken = default)
    {
        var request = new SearchRequest
        {
            Skip = skip,
            Take = take,
            IncludePrerelease = prerelease,
            IncludeSemVer2 = semVerLevel == "2.0.0",
            PackageType = packageType,
            Framework = framework,
            Query = query ?? string.Empty,
        };

        return await searchService.Search(request, cancellationToken);
    }

    public async Task<ActionResult<AutocompleteResponse>> AutocompleteAsync(
        [FromQuery(Name = "q")] string? autocompleteQuery = null,
        [FromQuery(Name = "id")] string? versionsQuery = null,
        [FromQuery]bool prerelease = false,
        [FromQuery]string? semVerLevel = null,
        [FromQuery]int skip = 0,
        [FromQuery]int take = 20,

        // These are unofficial parameters
        [FromQuery]string? packageType = null,
        CancellationToken cancellationToken = default)
    {
        // If only "id" is provided, find package versions. Otherwise, find package IDs.
        if (versionsQuery is not null && autocompleteQuery is null)
        {
            var request = new VersionsRequest
            {
                IncludePrerelease = prerelease,
                IncludeSemVer2 = semVerLevel == "2.0.0",
                PackageId = versionsQuery
            };

            return await searchService.ListPackageVersions(request, cancellationToken);
        }
        else
        {
            var request = new AutocompleteRequest
            {
                IncludePrerelease = prerelease,
                IncludeSemVer2 = semVerLevel == "2.0.0",
                PackageType = packageType,
                Skip = skip,
                Take = take,
                Query = autocompleteQuery ?? string.Empty
            };

            return await searchService.Autocomplete(request, cancellationToken);
        }
    }

    public async Task<ActionResult<DependentsResponse>> DependentsAsync(
        [FromQuery] string? packageId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(packageId))
        {
            return BadRequest();
        }

        return await searchService.FindDependents(packageId, cancellationToken);
    }
}
