namespace NaGet.Protocol;

public partial class NuGetClientFactory
{
    private class SearchClient : ISearchClient
    {
        private readonly NuGetClientFactory clientfactory;

        public SearchClient(NuGetClientFactory clientFactory)
        {
            clientfactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        }

        public async Task<SearchResponse?> SearchAsync(
            string? query = null,
            int skip = 0,
            int take = 20,
            bool includePrerelease = true,
            bool includeSemVer2 = true,
            CancellationToken cancellationToken = default)
        {
            // TODO: Support search failover.
            // See: https://github.com/loic-sharma/NaGet/issues/314
            var client = await clientfactory.GetSearchClientAsync(cancellationToken);

            if (client is null)
            {
                return null;
            }

            return await client.SearchAsync(query, skip, take, includePrerelease, includeSemVer2, cancellationToken);
        }
    }
}
