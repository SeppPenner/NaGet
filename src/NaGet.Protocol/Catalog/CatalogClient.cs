namespace NaGet.Protocol;

public partial class NuGetClientFactory
{
    private class CatalogClient : ICatalogClient
    {
        private readonly NuGetClientFactory clientfactory;

        public CatalogClient(NuGetClientFactory clientFactory)
        {
            clientfactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        }

        public async Task<CatalogIndex?> GetIndexAsync(CancellationToken cancellationToken = default)
        {
            var client = await clientfactory.GetCatalogClientAsync(cancellationToken);

            if (client is null)
            {
                return null;
            }

            return await client.GetIndexAsync(cancellationToken);
        }

        public async Task<CatalogPage?> GetPageAsync(string pageUrl, CancellationToken cancellationToken = default)
        {
            var client = await clientfactory.GetCatalogClientAsync(cancellationToken);

            if (client is null)
            {
                return null;
            }

            return await client.GetPageAsync(pageUrl, cancellationToken);
        }

        public async Task<PackageDetailsCatalogLeaf?> GetPackageDetailsLeafAsync(string leafUrl, CancellationToken cancellationToken = default)
        {
            var client = await clientfactory.GetCatalogClientAsync(cancellationToken);

            if (client is null)
            {
                return null;
            }

            return await client.GetPackageDetailsLeafAsync(leafUrl, cancellationToken);
        }

        public async Task<PackageDeleteCatalogLeaf?> GetPackageDeleteLeafAsync(string leafUrl, CancellationToken cancellationToken = default)
        {
            var client = await clientfactory.GetCatalogClientAsync(cancellationToken);

            if (client is null)
            {
                return null;
            }

            return await client.GetPackageDeleteLeafAsync(leafUrl, cancellationToken);
        }
    }
}
