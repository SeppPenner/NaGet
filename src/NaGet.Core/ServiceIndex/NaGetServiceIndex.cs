namespace NaGet.Core;

public class NaGetServiceIndex : IServiceIndexService
{
    private readonly IUrlGenerator url;

    public NaGetServiceIndex(IUrlGenerator url)
    {
        this.url = url ?? throw new ArgumentNullException(nameof(url));
    }

    private IEnumerable<ServiceIndexItem> BuildResource(string name, string url, params string[] versions)
    {
        foreach (var version in versions)
        {
            var type = string.IsNullOrWhiteSpace(version) ? name : $"{name}/{version}";

            yield return new ServiceIndexItem
            {
                ResourceUrl = url,
                Type = type,
            };
        }
    }

    public Task<ServiceIndexResponse> GetAsync(CancellationToken cancellationToken = default)
    {
        var resources = new List<ServiceIndexItem>();

        resources.AddRange(BuildResource("PackagePublish", url.GetPackagePublishResourceUrl(), "2.0.0"));
        resources.AddRange(BuildResource("SymbolPackagePublish", url.GetSymbolPublishResourceUrl(), "4.9.0"));
        resources.AddRange(BuildResource("SearchQueryService", url.GetSearchResourceUrl(), "", "3.0.0-beta", "3.0.0-rc"));
        resources.AddRange(BuildResource("RegistrationsBaseUrl", url.GetPackageMetadataResourceUrl(), "", "3.0.0-rc", "3.0.0-beta"));
        resources.AddRange(BuildResource("PackageBaseAddress", url.GetPackageContentResourceUrl(), "3.0.0"));
        resources.AddRange(BuildResource("SearchAutocompleteService", url.GetAutocompleteResourceUrl(), "", "3.0.0-rc", "3.0.0-beta"));

        var result = new ServiceIndexResponse
        {
            Version = "3.0.0",
            Resources = resources,
        };

        return Task.FromResult(result);
    }
}
