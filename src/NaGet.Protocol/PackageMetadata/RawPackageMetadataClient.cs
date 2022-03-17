namespace NaGet.PackageMetadata;

/// <summary>
/// The client to interact with an upstream source's Package Metadata resource.
/// </summary>
public class RawPackageMetadataClient : IPackageMetadataClient
{
    private readonly HttpClient httpClient;
    private readonly string packageMetadataUrl;

    /// <summary>
    /// Create a new Package Metadata client.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to send requests.</param>
    /// <param name="registrationBaseUrl">The NuGet server's registration resource URL.</param>
    public RawPackageMetadataClient(HttpClient httpClient, string registrationBaseUrl)
    {
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        packageMetadataUrl = registrationBaseUrl ?? throw new ArgumentNullException(nameof(registrationBaseUrl));
    }

    /// <inheritdoc />
    public async Task<RegistrationIndexResponse?> GetRegistrationIndexOrNullAsync(
        string packageId,
        CancellationToken cancellationToken = default)
    {
        var url = $"{packageMetadataUrl}/{packageId.ToLowerInvariant()}/index.json";
        return await httpClient.GetFromJsonOrDefaultAsync<RegistrationIndexResponse>(url, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<RegistrationPageResponse?> GetRegistrationPageAsync(
        string pageUrl,
        CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<RegistrationPageResponse>(pageUrl, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<RegistrationLeafResponse?> GetRegistrationLeafAsync(
        string leafUrl,
        CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<RegistrationLeafResponse>(leafUrl, cancellationToken);
    }
}
