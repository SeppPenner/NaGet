// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AzureSearchService.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The Azure search service class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.Azure.Search;

using QueryType = Microsoft.Azure.Search.Models.QueryType;
using SearchParameters = Microsoft.Azure.Search.Models.SearchParameters;
using SearchResult = Protocol.Models.SearchResult;

/// <summary>
/// The Azure search service class.
/// </summary>
public class AzureSearchService : ISearchService
{
    /// <summary>
    /// The search client.
    /// </summary>
    private readonly SearchIndexClient searchClient;

    /// <summary>
    /// The url generator.
    /// </summary>
    private readonly IUrlGenerator urlGenerator;

    /// <summary>
    /// The framework compabitility service.
    /// </summary>
    private readonly IFrameworkCompatibilityService frameworkCompatibilityService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureSearchService"/> class.
    /// </summary>
    /// <param name="searchClient">The search client.</param>
    /// <param name="urlGenerator">The url generator.</param>
    /// <param name="frameworkCompatibilityService">The framework compabitility service.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters is null.</exception>
    public AzureSearchService(
        SearchIndexClient searchClient,
        IUrlGenerator urlGenerator,
        IFrameworkCompatibilityService frameworkCompatibilityService)
    {
        this.searchClient = searchClient ?? throw new ArgumentNullException(nameof(searchClient));
        this.urlGenerator = urlGenerator ?? throw new ArgumentNullException(nameof(urlGenerator));
        this.frameworkCompatibilityService = frameworkCompatibilityService ?? throw new ArgumentNullException(nameof(frameworkCompatibilityService));
    }

    /// <inheritdoc cref="ISearchService"/>
    public async Task<SearchResponse> Search(
        SearchRequest request,
        CancellationToken cancellationToken)
    {
        var searchText = BuildSeachQuery(request.Query, request.PackageType, request.Framework);
        var filter = BuildSearchFilter(request.IncludePrerelease, request.IncludeSemVer2);
        var parameters = new SearchParameters
        {
            IncludeTotalResultCount = true,
            QueryType = QueryType.Full,
            Skip = request.Skip,
            Top = request.Take,
            Filter = filter
        };

        var response = await searchClient.Documents.SearchAsync<PackageDocument>(
            searchText,
            parameters,
            cancellationToken: cancellationToken);

        var results = new List<SearchResult>();

        foreach (var result in response.Results)
        {
            var document = result.Document;
            var versions = new List<SearchResultVersion>();

            if (document.Versions.Length != document.VersionDownloads.Length)
            {
                throw new InvalidOperationException($"Invalid document {document.Key} with mismatched versions");
            }

            for (var i = 0; i < document.Versions.Length; i++)
            {
                var version = NuGetVersion.Parse(document.Versions[i]);

                versions.Add(new SearchResultVersion
                {
                    RegistrationLeafUrl = urlGenerator.GetRegistrationLeafUrl(document.Id, version) ?? string.Empty,
                    Version = document.Versions[i],
                    Downloads = long.Parse(document.VersionDownloads[i]),
                });
            }

            var iconUrl = document.HasEmbeddedIcon
                ? urlGenerator.GetPackageIconDownloadUrl(document.Id, NuGetVersion.Parse(document.Version))
                : document.IconUrl;

            results.Add(new SearchResult
            {
                PackageId = document.Id,
                Version = document.Version,
                Description = document.Description,
                Authors = document.Authors,
                IconUrl = iconUrl ?? string.Empty,
                LicenseUrl = document.LicenseUrl,
                ProjectUrl = document.ProjectUrl,
                RegistrationIndexUrl = urlGenerator.GetRegistrationIndexUrl(document.Id) ?? string.Empty,
                Summary = document.Summary,
                Tags = document.Tags,
                Title = document.Title,
                TotalDownloads = document.TotalDownloads,
                Versions = versions
            });
        }

        return new SearchResponse
        {
            TotalHits = response?.Count ?? 0L,
            Data = results,
            Context = SearchContext.Default(urlGenerator.GetPackageMetadataResourceUrl() ?? string.Empty)
        };
    }

    /// <inheritdoc cref="ISearchService"/>
    public async Task<AutocompleteResponse> Autocomplete(
        AutocompleteRequest request,
        CancellationToken cancellationToken)
    {
        // TODO: Do a prefix search on the package id field.
        // TODO: Support prerelease, semver2, and package type filters.
        // See: https://github.com/loic-sharma/NaGet/issues/291
        var parameters = new SearchParameters
        {
            IncludeTotalResultCount = true,
            Skip = request.Skip,
            Top = request.Take,
        };

        var response = await searchClient.Documents.SearchAsync<PackageDocument>(
            request.Query,
            parameters,
            cancellationToken : cancellationToken);

        var results = response.Results
            .Select(r => r.Document.Id)
            .ToList()
            .AsReadOnly();

        return new AutocompleteResponse
        {
            TotalHits = response?.Count ?? 0L,
            Data = results,
            Context = AutocompleteContext.Default
        };
    }

    /// <inheritdoc cref="ISearchService"/>
    public Task<AutocompleteResponse> ListPackageVersions(
        VersionsRequest request,
        CancellationToken cancellationToken)
    {
        // TODO: Support versions autocomplete.
        // See: https://github.com/loic-sharma/NaGet/issues/291
        throw new NotImplementedException();
    }

    /// <inheritdoc cref="ISearchService"/>
    public async Task<DependentsResponse> FindDependents(
        string packageId,
        CancellationToken cancellationToken)
    {
        // TODO: Escape packageId.
        var query = $"dependencies:{packageId.ToLowerInvariant()}";
        var parameters = new SearchParameters
        {
            IncludeTotalResultCount = true,
            QueryType = QueryType.Full,
            Skip = 0,
            Top = 20,
        };

        var response = await searchClient.Documents.SearchAsync<PackageDocument>(query, parameters, cancellationToken: cancellationToken);
        var results = response.Results
            .Select(r => new PackageDependent
            {
                Id = r.Document.Id,
                Description = r.Document.Description,
                TotalDownloads = r.Document.TotalDownloads
            })
            .ToList()
            .AsReadOnly();

        return new DependentsResponse
        {
            TotalHits = response?.Count ?? 0L,
            Data = results
        };
    }

    /// <summary>
    /// Builds the search query.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="packageType">The package type.</param>
    /// <param name="framework">The framework.</param>
    /// <returns>The query string.</returns>
    private string BuildSeachQuery(string? query, string? packageType, string? framework)
    {
        var queryBuilder = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(query))
        {
            queryBuilder.Append(query.TrimEnd().TrimEnd('*'));
            queryBuilder.Append('*');
        }

        if (!string.IsNullOrWhiteSpace(packageType))
        {
            queryBuilder.Append(" +packageTypes:");
            queryBuilder.Append(packageType);
        }

        if (!string.IsNullOrWhiteSpace(framework))
        {
            var frameworks = this.frameworkCompatibilityService.FindAllCompatibleFrameworks(framework);

            queryBuilder.Append(" +frameworks:(");
            queryBuilder.Append(string.Join(" ", frameworks));
            queryBuilder.Append(')');
        }

        return queryBuilder.ToString();
    }

    /// <summary>
    /// Builds the search filter.
    /// </summary>
    /// <param name="includePrerelease">A value indicating whether prereleases should be included or not.</param>
    /// <param name="includeSemVer2">A value indicating whether SemVer2 packages should be included or not.</param>
    /// <returns>The search filter.</returns>
    private static string BuildSearchFilter(bool includePrerelease, bool includeSemVer2)
    {
        var searchFilters = SearchFilters.Default;

        if (includePrerelease)
        {
            searchFilters |= SearchFilters.IncludePrerelease;
        }

        if (includeSemVer2)
        {
            searchFilters |= SearchFilters.IncludeSemVer2;
        }

        return $"searchFilters eq '{searchFilters}'";
    }
}
