namespace NaGet.Protocol.Internal;

/// <summary>
/// The NuGet Service Index client, used to discover other resources.
/// 
/// See https://docs.microsoft.com/en-us/nuget/api/service-index
/// </summary>
public class RawServiceIndexClient : IServiceIndexClient
{
    private readonly HttpClient httpClient;
    private readonly string serviceIndexUrl;

    /// <summary>
    /// Create a service index for the upstream source.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to send requests.</param>
    /// <param name="serviceIndexUrl">The NuGet server's service index URL.</param>
    public RawServiceIndexClient(HttpClient httpClient, string serviceIndexUrl)
    {
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        this.serviceIndexUrl = serviceIndexUrl ?? throw new ArgumentNullException(nameof(serviceIndexUrl));
    }

    /// <inheritdoc />
    public async Task<ServiceIndexResponse?> GetAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<ServiceIndexResponse?>(
            serviceIndexUrl,
            cancellationToken);
    }
}
