namespace NaGet.Protocol.Internal;

public class RawCatalogClient : ICatalogClient
{
    private readonly HttpClient httpClient;
    private readonly string catalogUrl;

    public RawCatalogClient(HttpClient httpClient, string catalogUrl)
    {
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        this.catalogUrl = catalogUrl ?? throw new ArgumentNullException(nameof(catalogUrl));
    }

    public async Task<CatalogIndex> GetIndexAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<CatalogIndex>(catalogUrl, cancellationToken);
    }

    public async Task<CatalogPage> GetPageAsync(string pageUrl, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<CatalogPage>(pageUrl, cancellationToken);
    }

    public async Task<PackageDeleteCatalogLeaf> GetPackageDeleteLeafAsync(string leafUrl, CancellationToken cancellationToken = default)
    {
        return await GetAndValidateLeafAsync<PackageDeleteCatalogLeaf>(
            "PackageDelete",
            leafUrl,
            cancellationToken);
    }

    public async Task<PackageDetailsCatalogLeaf> GetPackageDetailsLeafAsync(string leafUrl, CancellationToken cancellationToken = default)
    {
        return await GetAndValidateLeafAsync<PackageDetailsCatalogLeaf>(
            "PackageDetails",
            leafUrl,
            cancellationToken);
    }

    private async Task<TCatalogLeaf> GetAndValidateLeafAsync<TCatalogLeaf>(
        string leafType,
        string leafUrl,
        CancellationToken cancellationToken) where TCatalogLeaf : CatalogLeaf
    {
        var leaf = await httpClient.GetFromJsonAsync<TCatalogLeaf>(leafUrl, cancellationToken);

        if (leaf.Type.FirstOrDefault() != leafType)
        {
            throw new ArgumentException(
                $"The leaf type found in the document does not match the expected '{leafType}' type.",
                nameof(leafType));
        }

        return leaf;
    }
}
