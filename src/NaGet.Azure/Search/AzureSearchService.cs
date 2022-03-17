namespace NaGet.Azure;

using QueryType = Microsoft.Azure.Search.Models.QueryType;
using SearchParameters = Microsoft.Azure.Search.Models.SearchParameters;
using SearchResult = Protocol.Models.SearchResult;

public class AzureSearchService : ISearchService
{
    private readonly SearchIndexClient searchClient;
    private readonly IUrlGenerator url;
    private readonly IFrameworkCompatibilityService frameworks;

    public AzureSearchService(
        SearchIndexClient searchClient,
        IUrlGenerator url,
        IFrameworkCompatibilityService frameworks)
    {
        this.searchClient = searchClient ?? throw new ArgumentNullException(nameof(searchClient));
        this.url = url ?? throw new ArgumentNullException(nameof(url));
        this.frameworks = frameworks ?? throw new ArgumentNullException(nameof(frameworks));
    }

    public async Task<SearchResponse> SearchAsync(
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
                    RegistrationLeafUrl = url.GetRegistrationLeafUrl(document.Id, version) ?? string.Empty,
                    Version = document.Versions[i],
                    Downloads = long.Parse(document.VersionDownloads[i]),
                });
            }

            var iconUrl = document.HasEmbeddedIcon
                ? url.GetPackageIconDownloadUrl(document.Id, NuGetVersion.Parse(document.Version))
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
                RegistrationIndexUrl = url.GetRegistrationIndexUrl(document.Id) ?? string.Empty,
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
            Context = SearchContext.Default(url.GetPackageMetadataResourceUrl() ?? string.Empty)
        };
    }

    public async Task<AutocompleteResponse> AutocompleteAsync(
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
            cancellationToken: cancellationToken);

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

    public Task<AutocompleteResponse> ListPackageVersionsAsync(
        VersionsRequest request,
        CancellationToken cancellationToken)
    {
        // TODO: Support versions autocomplete.
        // See: https://github.com/loic-sharma/NaGet/issues/291
        throw new NotImplementedException();
    }

    public async Task<DependentsResponse> FindDependentsAsync(
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
            var frameworks = this.frameworks.FindAllCompatibleFrameworks(framework);

            queryBuilder.Append(" +frameworks:(");
            queryBuilder.Append(string.Join(" ", frameworks));
            queryBuilder.Append(')');
        }

        return queryBuilder.ToString();
    }

    private string BuildSearchFilter(bool includePrerelease, bool includeSemVer2)
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
