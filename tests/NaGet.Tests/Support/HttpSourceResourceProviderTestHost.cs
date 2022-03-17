using System.Collections.Concurrent;
using System.Diagnostics;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace NaGet.Tests
{
    /// <summary>
    /// Similar to official HttpSourceResourceProvider, but uses test host.
    /// </summary>
    public class HttpSourceResourceProviderTestHost : ResourceProvider
    {
        // Only one HttpSource per source should exist. This is to reduce the number of TCP connections.
        private readonly ConcurrentDictionary<PackageSource, HttpSourceResource> cache
            = new ConcurrentDictionary<PackageSource, HttpSourceResource>();
        private readonly HttpClient httpClient;

        public HttpSourceResourceProviderTestHost(HttpClient httpClient)
            : base(typeof(HttpSourceResource),
                  nameof(HttpSourceResource),
                  NuGetResourceProviderPositions.Last)
        {
            this.httpClient = httpClient;
        }

        public override Task<Tuple<bool, INuGetResource>> TryCreate(SourceRepository source, CancellationToken token)
        {
            Debug.Assert(source.PackageSource.IsHttp, "HTTP source requested for a non-http source.");

            HttpSourceResource? curResource = null;

            if (source.PackageSource.IsHttp)
            {
                curResource = cache.GetOrAdd(
                    source.PackageSource, 
                    packageSource => new HttpSourceResource(TestableHttpSource.Create(source, httpClient)));
            }

            return Task.FromResult(new Tuple<bool, INuGetResource>(curResource != null, curResource));
        }
    }
}
