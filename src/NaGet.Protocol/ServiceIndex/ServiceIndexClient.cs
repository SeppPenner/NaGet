namespace NaGet.Protocol;

public partial class NuGetClientFactory
{
    private class ServiceIndexClient : IServiceIndexClient
    {
        private readonly NuGetClientFactory clientFactory;

        public ServiceIndexClient(NuGetClientFactory clientFactory)
        {
            this.clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        }

        public async Task<ServiceIndexResponse?> GetAsync(CancellationToken cancellationToken = default)
        {
            return await clientFactory.GetServiceIndexAsync(cancellationToken);
        }
    }
}
