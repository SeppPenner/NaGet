using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Xunit;
using Xunit.Abstractions;

namespace NaGet.Tests
{
    /// <summary>
    /// Uses the official NuGet client to interact with the NaGet test host.
    /// </summary>
    public class NuGetClientIntegrationTests : IDisposable
    {
        private readonly NaGetApplication app;
        private readonly HttpClient client;

        private readonly Stream? packageStream;

        private readonly SourceRepository repository;
        private readonly SourceCacheContext cache;
        private readonly NuGet.Common.ILogger logger;
        private readonly CancellationToken cancellationToken;

        public NuGetClientIntegrationTests(ITestOutputHelper output)
        {
            app = new NaGetApplication(output);
            client = app.CreateDefaultClient();
            packageStream = TestResources.GetResourceStream(TestResources.Package);

            var sourceUri = new Uri(app.Server.BaseAddress, "v3/index.json");
            var packageSource = new PackageSource(sourceUri.AbsoluteUri);
            var providers = new List<Lazy<INuGetResourceProvider>>();

            providers.Add(new Lazy<INuGetResourceProvider>(() => new HttpSourceResourceProviderTestHost(client)));
            providers.AddRange(Repository.Provider.GetCoreV3());

            repository = new SourceRepository(packageSource, providers);
            cache = new SourceCacheContext { NoCache = true, MaxAge = new DateTimeOffset(), DirectDownload = true };
            logger = NuGet.Common.NullLogger.Instance;
            cancellationToken = CancellationToken.None;
        }

        [Fact]
        public async Task ValidIndex()
        {
            var index = await repository.GetResourceAsync<ServiceIndexResourceV3>();

            Assert.Equal(12, index.Entries.Count);

            Assert.NotEmpty(index.GetServiceEntries("PackageBaseAddress/3.0.0"));
            Assert.NotEmpty(index.GetServiceEntries("PackagePublish/2.0.0"));
            Assert.NotEmpty(index.GetServiceEntries("RegistrationsBaseUrl"));
            Assert.NotEmpty(index.GetServiceEntries("SearchAutocompleteService"));
            Assert.NotEmpty(index.GetServiceEntries("SearchQueryService"));
            Assert.NotEmpty(index.GetServiceEntries("SymbolPackagePublish/4.9.0"));
        }

        [Fact]
        public async Task SearchReturnsResults()
        {
            await app.AddPackageAsync(packageStream);

            var resource = await repository.GetResourceAsync<PackageSearchResource>();
            var searchFilter = new SearchFilter(includePrerelease: true);

            var results = await resource.SearchAsync(
                "",
                searchFilter,
                skip: 0,
                take: 20,
                logger,
                cancellationToken);

            var result = Assert.Single(results);

            Assert.Equal("TestData", result.Identity.Id);
            Assert.Equal("1.2.3", result.Identity.Version.ToNormalizedString());
            Assert.Equal("Test description", result.Description);
            Assert.Equal("Test author", result.Authors);
            Assert.Equal(0, result.DownloadCount);

            var versions = await result.GetVersionsAsync();
            var version = Assert.Single(versions);

            Assert.Equal("1.2.3", version.Version.ToNormalizedString());
            Assert.Equal(0, version.DownloadCount);
        }

        [Fact]
        public async Task SearchReturnsEmpty()
        {
            var resource = await repository.GetResourceAsync<PackageSearchResource>();
            var searchFilter = new SearchFilter(includePrerelease: true);

            var results = await resource.SearchAsync(
                "PackageDoesNotExist",
                searchFilter,
                skip: 0,
                take: 20,
                logger,
                cancellationToken);

            Assert.Empty(results);
        }

        [Fact]
        public async Task AutocompleteReturnsResults()
        {
            await app.AddPackageAsync(packageStream);

            var resource = await repository.GetResourceAsync<AutoCompleteResource>();
            var results = await resource.IdStartsWith(
                "",
                includePrerelease: true,
                logger,
                cancellationToken);

            var result = Assert.Single(results);

            Assert.Equal("TestData", result);
        }

        [Fact]
        public async Task AutocompleteReturnsEmpty()
        {
            var resource = await repository.GetResourceAsync<AutoCompleteResource>();
            var results = await resource.IdStartsWith(
                "PackageDoesNotExist",
                includePrerelease: true,
                logger,
                cancellationToken);

            Assert.Empty(results);
        }

        [Fact]
        public async Task VersionListReturnsResults()
        {
            await app.AddPackageAsync(packageStream);

            var resource = await repository.GetResourceAsync<FindPackageByIdResource>();
            var versions = await resource.GetAllVersionsAsync(
                "TestData",
                cache,
                logger,
                cancellationToken);

            var version = Assert.Single(versions);

            Assert.Equal("1.2.3", version.ToNormalizedString());
        }

        [Fact]
        public async Task VersionListReturnsEmpty()
        {
            var resource = await repository.GetResourceAsync<FindPackageByIdResource>();
            var versions = await resource.GetAllVersionsAsync(
                "PackageDoesNotExist",
                cache,
                logger,
                cancellationToken);

            Assert.Empty(versions);
        }

        [Theory]
        [InlineData("TestData", "1.0.0", false)]
        [InlineData("TestData", "1.2.3", true)]
        [InlineData("PackageDoesNotExists", "1.0.0", false)]
        public async Task PackageExistsWorks(string packageId, string packageVersion, bool exists)
        {
            await app.AddPackageAsync(packageStream);

            var version = NuGetVersion.Parse(packageVersion);
            var resource = await repository.GetResourceAsync<FindPackageByIdResource>();
            var result = await resource.DoesPackageExistAsync(
                packageId,
                version,
                cache,
                logger,
                cancellationToken);

            Assert.Equal(exists, result);
        }

        [Theory]
        [InlineData("TestData", "1.0.0", false)]
        [InlineData("TestData", "1.2.3", true)]
        [InlineData("PackageDoesNotExists", "1.0.0", false)]
        public async Task PackageDownloadWorks(string packageId, string packageVersion, bool exists)
        {
            await app.AddPackageAsync(this.packageStream);

            using var packageStream = new MemoryStream();

            var version = NuGetVersion.Parse(packageVersion);
            var resource = await repository.GetResourceAsync<FindPackageByIdResource>();
            var result = await resource.CopyNupkgToStreamAsync(
                packageId,
                version,
                packageStream,
                cache,
                logger,
                cancellationToken);

            packageStream.Position = 0;

            Assert.Equal(exists, result);
            Assert.Equal(exists, packageStream.Length > 0);
        }

        [Fact]
        public async Task PackageMetadataReturnsOk()
        {
            await app.AddPackageAsync(packageStream);

            var resource = await repository.GetResourceAsync<PackageMetadataResource>();
            var packages = await resource.GetMetadataAsync(
                "TestData",
                includePrerelease: true,
                includeUnlisted: true,
                cache,
                logger,
                cancellationToken);

            var package = Assert.Single(packages);

            Assert.Equal("TestData", package.Identity.Id);
            Assert.Equal("1.2.3", package.Identity.Version.ToNormalizedString());
            Assert.Equal("Test description", package.Description);
            Assert.Equal("Test author", package.Authors);
            Assert.True(package.IsListed);
        }

        [Fact]
        public async Task PackageMetadataReturnsEmty()
        {
            var resource = await repository.GetResourceAsync<PackageMetadataResource>();
            var packages = await resource.GetMetadataAsync(
                "PackageDoesNotExist",
                includePrerelease: true,
                includeUnlisted: true,
                cache,
                logger,
                cancellationToken);

            Assert.Empty(packages);
        }

        public void Dispose()
        {
            app.Dispose();
            client.Dispose();
            cache.Dispose();
        }
    }
}
