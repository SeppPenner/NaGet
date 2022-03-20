using System.Net;
using Xunit;
using Xunit.Abstractions;

namespace NaGet.Tests
{
    public class ApiIntegrationTests : IDisposable
    {
        private readonly NaGetApplication app;
        private readonly HttpClient client;

        private readonly Stream? packageStream;
        private readonly Stream? symbolPackageStream;

        public ApiIntegrationTests(ITestOutputHelper output)
        {
            app = new NaGetApplication(output);
            client = app.CreateClient();

            packageStream = TestResources.GetResourceStream(TestResources.Package);
            symbolPackageStream = TestResources.GetResourceStream(TestResources.SymbolPackage);
        }

        [Fact]
        public async Task IndexReturnsOk()
        {
            using var response = await client.GetAsync("v3/index.json");
            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(TestData.ServiceIndex, content);
        }

        [Fact]
        public async Task SearchReturnsOk()
        {
            await app.AddPackageAsync(packageStream);

            using var response = await client.GetAsync("v3/search");
            var content = await response.Content.ReadAsStreamAsync();
            var json = content.ToPrettifiedJson();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(@"{
  ""@context"": {
    ""@vocab"": ""http://schema.nuget.org/schema#"",
    ""@base"": ""http://localhost/v3/registration""
  },
  ""totalHits"": 1,
  ""data"": [
    {
      ""id"": ""TestData"",
      ""version"": ""1.2.3"",
      ""description"": ""Test description"",
      ""authors"": [
        ""Test author""
      ],
      ""iconUrl"": """",
      ""licenseUrl"": """",
      ""projectUrl"": """",
      ""registration"": ""http://localhost/v3/registration/testdata/index.json"",
      ""summary"": """",
      ""tags"": [],
      ""title"": """",
      ""totalDownloads"": 0,
      ""versions"": [
        {
          ""@id"": ""http://localhost/v3/registration/testdata/1.2.3.json"",
          ""version"": ""1.2.3"",
          ""downloads"": 0
        }
      ]
    }
  ]
}", json);
        }

        [Fact]
        public async Task SearchReturnsEmpty()
        {
            using var response = await client.GetAsync("v3/search?q=PackageDoesNotExist");
            var content = await response.Content.ReadAsStreamAsync();
            var json = content.ToPrettifiedJson();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(@"{
  ""@context"": {
    ""@vocab"": ""http://schema.nuget.org/schema#"",
    ""@base"": ""http://localhost/v3/registration""
  },
  ""totalHits"": 0,
  ""data"": []
}", json);
        }

        [Fact]
        public async Task AutocompleteReturnsOk()
        {
            await app.AddPackageAsync(packageStream);

            using var response = await client.GetAsync("v3/autocomplete");
            var content = await response.Content.ReadAsStreamAsync();
            var json = content.ToPrettifiedJson();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(@"{
  ""@context"": {
    ""@vocab"": ""http://schema.nuget.org/schema#""
  },
  ""totalHits"": 1,
  ""data"": [
    ""TestData""
  ]
}", json);
        }

        [Fact]
        public async Task AutocompleteReturnsEmpty()
        {
            using var response = await client.GetAsync("v3/autocomplete?q=PackageDoesNotExist");
            var content = await response.Content.ReadAsStreamAsync();
            var json = content.ToPrettifiedJson();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(@"{
  ""@context"": {
    ""@vocab"": ""http://schema.nuget.org/schema#""
  },
  ""totalHits"": 0,
  ""data"": []
}", json);
        }

        [Fact]
        public async Task AutocompleteVersionsReturnsOk()
        {
            await app.AddPackageAsync(packageStream);

            using var response = await client.GetAsync("v3/autocomplete?id=TestData");
            var content = await response.Content.ReadAsStreamAsync();
            var json = content.ToPrettifiedJson();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(@"{
  ""@context"": {
    ""@vocab"": ""http://schema.nuget.org/schema#""
  },
  ""totalHits"": 1,
  ""data"": [
    ""1.2.3""
  ]
}", json);
        }

        [Fact]
        public async Task AutocompleteVersionsReturnsEmpty()
        {
            using var response = await client.GetAsync("v3/autocomplete?id=PackageDoesNotExist");
            var content = await response.Content.ReadAsStreamAsync();
            var json = content.ToPrettifiedJson();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(@"{
  ""@context"": {
    ""@vocab"": ""http://schema.nuget.org/schema#""
  },
  ""totalHits"": 0,
  ""data"": []
}", json);
        }

        [Fact]
        public async Task VersionListReturnsOk()
        {
            await app.AddPackageAsync(packageStream);

            var response = await client.GetAsync("v3/package/TestData/index.json");
            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(@"{""versions"":[""1.2.3""]}", content);
        }

        [Fact]
        public async Task VersionListReturnsNotFound()
        {
            using var response = await client.GetAsync("v3/package/PackageDoesNotExist/index.json");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task PackageDownloadReturnsOk()
        {
            await app.AddPackageAsync(packageStream);

            using var response = await client.GetAsync("v3/package/TestData/1.2.3/TestData.1.2.3.nupkg");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task PackageDownloadReturnsNotFound()
        {
            using var response = await client.GetAsync(
                "v3/package/PackageDoesNotExist/1.0.0/PackageDoesNotExist.1.0.0.nupkg");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task NuspecDownloadReturnsOk()
        {
            await app.AddPackageAsync(packageStream);

            using var response = await client.GetAsync(
                "v3/package/TestData/1.2.3/TestData.1.2.3.nuspec");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task NuspecDownloadReturnsNotFound()
        {
            using var response = await client.GetAsync(
                "v3/package/PackageDoesNotExist/1.0.0/PackageDoesNotExist.1.0.0.nuspec");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task PackageMetadataReturnsOk()
        {
            await app.AddPackageAsync(packageStream);

            using var response = await client.GetAsync("v3/registration/TestData/index.json");
            var content = await response.Content.ReadAsStreamAsync();
            var json = content.ToPrettifiedJson();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(@"{
  ""@id"": ""http://localhost/v3/registration/testdata/index.json"",
  ""@type"": [
    ""catalog:CatalogRoot"",
    ""PackageRegistration"",
    ""catalog:Permalink""
  ],
  ""count"": 1,
  ""items"": [
    {
      ""@id"": ""http://localhost/v3/registration/testdata/index.json"",
      ""count"": 1,
      ""lower"": ""1.2.3"",
      ""upper"": ""1.2.3"",
      ""items"": [
        {
          ""@id"": ""http://localhost/v3/registration/testdata/1.2.3.json"",
          ""packageContent"": ""http://localhost/v3/package/testdata/1.2.3/testdata.1.2.3.nupkg"",
          ""catalogEntry"": {
            ""downloads"": 0,
            ""hasReadme"": false,
            ""packageTypes"": [
              ""Dependency""
            ],
            ""releaseNotes"": """",
            ""repositoryUrl"": """",
            ""id"": ""TestData"",
            ""version"": ""1.2.3"",
            ""authors"": ""Test author"",
            ""dependencyGroups"": [
              {
                ""targetFramework"": ""net5.0"",
                ""dependencies"": []
              }
            ],
            ""description"": ""Test description"",
            ""iconUrl"": """",
            ""language"": """",
            ""licenseUrl"": """",
            ""listed"": true,
            ""minClientVersion"": """",
            ""packageContent"": ""http://localhost/v3/package/testdata/1.2.3/testdata.1.2.3.nupkg"",
            ""projectUrl"": """",
            ""published"": ""2020-01-01T00:00:00Z"",
            ""requireLicenseAcceptance"": false,
            ""summary"": """",
            ""tags"": [],
            ""title"": """"
          }
        }
      ]
    }
  ],
  ""totalDownloads"": 0
}", json);
        }

        [Fact]
        public async Task PackageMetadataReturnsNotFound()
        {
            using var response = await client.GetAsync("v3/registration/PackageDoesNotExist/index.json");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task PackageMetadataLeafReturnsOk()
        {
            await app.AddPackageAsync(packageStream);

            using var response = await client.GetAsync("v3/registration/TestData/1.2.3.json");
            var content = await response.Content.ReadAsStreamAsync();
            var json = content.ToPrettifiedJson();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(@"{
  ""@id"": ""http://localhost/v3/registration/testdata/1.2.3.json"",
  ""@type"": [
    ""Package"",
    ""http://schema.nuget.org/catalog#Permalink""
  ],
  ""listed"": true,
  ""packageContent"": ""http://localhost/v3/package/testdata/1.2.3/testdata.1.2.3.nupkg"",
  ""published"": ""2020-01-01T00:00:00Z"",
  ""registration"": ""http://localhost/v3/registration/testdata/index.json""
}", json);
        }

        [Fact]
        public async Task PackageMetadataLeafReturnsNotFound()
        {
            using var response = await client.GetAsync("v3/registration/PackageDoesNotExist/1.0.0.json");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task PackageDependentsReturnsOk()
        {
            using var response = await client.GetAsync("v3/dependents?packageId=TestData");

            var content = await response.Content.ReadAsStreamAsync();
            var json = content.ToPrettifiedJson();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(@"{
  ""totalHits"": 0,
  ""data"": []
}", json);
        }

        [Fact]
        public async Task PackageDependentsReturnsBadRequest()
        {
            using var response = await client.GetAsync("v3/dependents");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SymbolDownloadReturnsOk()
        {
            await app.AddPackageAsync(packageStream);
            await app.AddSymbolPackageAsync(symbolPackageStream);

            using var response = await client.GetAsync(
                "api/download/symbols/testdata.pdb/16F71ED8DD574AA2AD4A22D29E9C981Bffffffff/testdata.pdb");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("api/download/symbols/testdata.pdb/16F71ED8DD574AA2AD4A22D29E9C981B1/testdata.pdb")]
        [InlineData("api/download/symbols/testdata.pdb/16F71ED8DD574AA2AD4A22D29E9C981B/testdata.pdb")]
        [InlineData("api/download/symbols/testprefix/testdata.pdb/16F71ED8DD574AA2AD4A22D29E9C981Bffffffff/testdata.pdb")]
        public async Task MalformedSymbolDownloadReturnsOk(string uri)
        {
            await app.AddPackageAsync(packageStream);
            await app.AddSymbolPackageAsync(symbolPackageStream);

            using var response = await client.GetAsync(uri);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task SymbolDownloadReturnsNotFound()
        {
            using var response = await client.GetAsync(
                "api/download/symbols/doesnotexist.pdb/16F71ED8DD574AA2AD4A22D29E9C981Bffffffff/doesnotexist.pdb");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        public void Dispose()
        {
            app.Dispose();
            client.Dispose();
        }
    }
}
