namespace NaGet.Protocol.Internal;

/// <summary>
/// The client to interact with an upstream source's Package Content resource.
/// </summary>
public class RawPackageContentClient : IPackageContentClient
{
    private readonly HttpClient httpClient;
    private readonly string packageContentUrl;

    /// <summary>
    /// Create a new Package Content client.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to send requests.</param>
    /// <param name="packageContentUrl">The NuGet Server's package content URL.</param>
    public RawPackageContentClient(HttpClient httpClient, string packageContentUrl)
    {
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        this.packageContentUrl = packageContentUrl?.TrimEnd('/')
            ?? throw new ArgumentNullException(nameof(packageContentUrl));
    }

    /// <inheritdoc />
    public async Task<PackageVersionsResponse> GetPackageVersionsOrNullAsync(
        string packageId,
        CancellationToken cancellationToken = default)
    {
        var id = packageId.ToLowerInvariant();
        var url = $"{packageContentUrl}/{id}/index.json";

        return await httpClient.GetFromJsonOrDefaultAsync<PackageVersionsResponse>(url, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Stream> DownloadPackageOrNullAsync(
        string packageId,
        NuGetVersion packageVersion,
        CancellationToken cancellationToken = default)
    {
        var id = packageId.ToLowerInvariant();
        var version = packageVersion.ToNormalizedString().ToLowerInvariant();

        // The response will be disposed when the returned content stream is disposed.
        var url = $"{packageContentUrl}/{id}/{version}/{id}.{version}.nupkg";
        var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        return await response.Content.ReadAsStreamAsync();
    }

    /// <inheritdoc />
    public async Task<Stream> DownloadPackageManifestOrNullAsync(
        string packageId,
        NuGetVersion packageVersion,
        CancellationToken cancellationToken = default)
    {
        var id = packageId.ToLowerInvariant();
        var version = packageVersion.ToNormalizedString().ToLowerInvariant();

        // The response will be disposed when the returned content stream is disposed.
        var url = $"{packageContentUrl}/{id}/{version}/{id}.nuspec";
        var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        return await response.Content.ReadAsStreamAsync();
    }
}
