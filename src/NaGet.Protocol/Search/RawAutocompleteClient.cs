namespace NaGet.Protocol.Internal;

/// <summary>
/// The client used to search for packages.
/// 
/// See https://docs.microsoft.com/en-us/nuget/api/search-autocomplete-service-resource
/// </summary>
public class RawAutocompleteClient : IAutocompleteClient
{
    private readonly HttpClient httpClient;
    private readonly string autocompleteUrl;

    /// <summary>
    /// Create a new Search client.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to send requests.</param>
    /// <param name="autocompleteUrl">The NuGet server's autocomplete URL.</param>
    public RawAutocompleteClient(HttpClient httpClient, string autocompleteUrl)
    {
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        this.autocompleteUrl = autocompleteUrl ?? throw new ArgumentNullException(nameof(autocompleteUrl));
    }

    public async Task<AutocompleteResponse> AutocompleteAsync(
        string query = null,
        int skip = 0,
        int take = 20,
        bool includePrerelease = true,
        bool includeSemVer2 = true,
        CancellationToken cancellationToken = default)
    {
        var url = RawSearchClient.AddSearchQueryString(
            autocompleteUrl,
            query,
            skip,
            take,
            includePrerelease,
            includeSemVer2,
            "q");

        return await httpClient.GetFromJsonAsync<AutocompleteResponse>(url, cancellationToken);
    }

    public async Task<AutocompleteResponse> ListPackageVersionsAsync(
        string packageId,
        bool includePrerelease = true,
        bool includeSemVer2 = true,
        CancellationToken cancellationToken = default)
    {
        var url = RawSearchClient.AddSearchQueryString(
            autocompleteUrl,
            packageId,
            skip: null,
            take: null,
            includePrerelease,
            includeSemVer2,
            "id");

        return await httpClient.GetFromJsonAsync<AutocompleteResponse>(url, cancellationToken);
    }
}
