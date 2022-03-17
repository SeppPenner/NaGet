using NaGet.Protocol;
using NaGet.Protocol.Models;
using NuGet.Versioning;
using Xunit;
using Xunit.Abstractions;

namespace NaGet.Tests
{
    /// <summary>
    /// Uses NaGet's client SDK to interact with the NaGet test host.
    /// </summary>
    public class NaGetClientIntegrationTests : IDisposable
    {
        private readonly NaGetApplication app;
        private readonly HttpClient httpClient;
        private readonly NuGetClientFactory clientFactory;
        private readonly NuGetClient client;

        private readonly Stream? packageStream;

        public NaGetClientIntegrationTests(ITestOutputHelper output)
        {
            app = new NaGetApplication(output);

            var serviceIndexUrl = new Uri(app.Server.BaseAddress, "v3/index.json");

            httpClient = app.CreateClient();
            clientFactory = new NuGetClientFactory(httpClient, serviceIndexUrl.AbsoluteUri);
            client = new NuGetClient(clientFactory);

            packageStream = TestResources.GetResourceStream(TestResources.Package);
        }

        [Fact]
        public async Task ValidIndex()
        {
            var client = clientFactory.CreateServiceIndexClient();
            var index = await client.GetAsync();

            Assert.NotNull(index);

            Assert.Equal("3.0.0", index.Version);
            Assert.Equal(12, index.Resources.Count);

            Assert.NotEmpty(index.GetResourceUrl(new[] { "PackageBaseAddress/3.0.0" }));
            Assert.NotEmpty(index.GetResourceUrl(new[] { "PackagePublish/2.0.0" }));
            Assert.NotEmpty(index.GetResourceUrl(new[] { "RegistrationsBaseUrl" }));
            Assert.NotEmpty(index.GetResourceUrl(new[] { "SearchAutocompleteService" }));
            Assert.NotEmpty(index.GetResourceUrl(new[] { "SearchQueryService" }));
            Assert.NotEmpty(index.GetResourceUrl(new[] { "SymbolPackagePublish/4.9.0" }));
        }

        [Fact]
        public async Task SearchReturnsResults()
        {
            await app.AddPackageAsync(packageStream);

            var results = await client.SearchAsync();

            var result = Assert.Single(results);
            var author = Assert.Single(result.Authors);
            var version = Assert.Single(result.Versions);

            Assert.Equal("TestData", result.PackageId);
            Assert.Equal("1.2.3", result.Version);
            Assert.Equal("Test description", result.Description);
            Assert.Equal("Test author", author);
            Assert.Equal(0, result.TotalDownloads);

            Assert.Equal("1.2.3", version.Version);
            Assert.Equal(0, version.Downloads);
        }

        [Fact]
        public async Task SearchReturnsEmpty()
        {
            var results = await client.SearchAsync("PackageDoesNotExist");

            Assert.Empty(results);
        }

        [Fact]
        public async Task AutocompleteReturnsResults()
        {
            await app.AddPackageAsync(packageStream);

            var results = await client.AutocompleteAsync();

            var result = Assert.Single(results);

            Assert.Equal("TestData", result);
        }

        [Fact]
        public async Task AutocompleteReturnsEmpty()
        {
            var results = await client.AutocompleteAsync("PackageDoesNotExist");

            Assert.Empty(results);
        }

        [Fact]
        public async Task AutocompleteVersions()
        {
            await app.AddPackageAsync(packageStream);

            var client = clientFactory.CreateAutocompleteClient();
            var results = await client.ListPackageVersionsAsync("TestData");

            var result = Assert.Single(results.Data);

            Assert.Equal(1, results.TotalHits);
            Assert.Equal("1.2.3", result);
        }

        [Fact]
        public async Task AutocompleteVersionsReturnsEmpty()
        {
            var client = clientFactory.CreateAutocompleteClient();
            var results = await client.ListPackageVersionsAsync("PackageDoesNotExist");

            Assert.Empty(results.Data);
            Assert.Equal(0, results.TotalHits);
        }

        [Fact]
        public async Task VersionListReturnsResults()
        {
            await app.AddPackageAsync(packageStream);

            var versions = await client.ListPackageVersionsAsync("TestData");

            var version = Assert.Single(versions);

            Assert.Equal("1.2.3", version.ToNormalizedString());
        }

        [Fact]
        public async Task VersionListReturnsEmpty()
        {
            var versions = await client.ListPackageVersionsAsync("PackageDoesNotExist");

            Assert.Empty(versions);
        }

        [Theory]
        [InlineData("TestData", "1.0.0", false)]
        [InlineData("TestData", "1.2.3", true)]
        [InlineData("PackageDoesNotExists", "1.0.0", false)]
        public async Task PackageDownloadWorks(string packageId, string packageVersion, bool exists)
        {
            await app.AddPackageAsync(packageStream);

            try
            {
                var version = NuGetVersion.Parse(packageVersion);

                using var memoryStream = new MemoryStream();
                using var packageStream = await client.DownloadPackageAsync(packageId, version);

                Assert.NotNull(packageStream);

                await packageStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                Assert.True(exists);
                Assert.Equal(exists, memoryStream.Length > 0);
            }
            catch (PackageNotFoundException)
            {
                Assert.False(exists);
            }
        }

        [Theory]
        [InlineData("TestData", "1.0.0", false)]
        [InlineData("TestData", "1.2.3", true)]
        [InlineData("PackageDoesNotExists", "1.0.0", false)]
        public async Task ManifestDownloadWorks(string packageId, string packageVersion, bool exists)
        {
            await app.AddPackageAsync(packageStream);

            try
            {
                var version = NuGetVersion.Parse(packageVersion);

                using var memoryStream = new MemoryStream();
                using var packageStream = await client.DownloadPackageManifestAsync(packageId, version);

                Assert.NotNull(packageStream);

                await packageStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                Assert.True(exists);
                Assert.Equal(exists, memoryStream.Length > 0);
            }
            catch (PackageNotFoundException)
            {
                Assert.False(exists);
            }
        }

        [Fact]
        public async Task PackageMetadataReturnsOk()
        {
            await app.AddPackageAsync(packageStream);

            var packages = await client.GetPackageMetadataAsync("TestData");

            var package = Assert.Single(packages);

            Assert.Equal("TestData", package.PackageId);
            Assert.Equal("1.2.3", package.Version);
            Assert.Equal("Test description", package.Description);
            Assert.Equal("Test author", package.Authors);
            Assert.True(package.Listed);
        }

        [Fact]
        public async Task PackageMetadataReturnsEmty()
        {
            var packages = await client.GetPackageMetadataAsync("PackageDoesNotExist");

            Assert.Empty(packages);
        }

        public void Dispose()
        {
            app.Dispose();
            httpClient.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
