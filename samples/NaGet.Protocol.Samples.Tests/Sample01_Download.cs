using NuGet.Versioning;
using Xunit;

namespace NaGet.Protocol.Samples.Tests
{
    public class Sample01_Download
    {
        [Fact]
        public async Task DownloadPackage()
        {
            // Downloads a package file (.nupkg)
            var client = new NuGetClient("https://api.nuget.org/v3/index.json");

            var packageId = "Newtonsoft.Json";
            var packageVersion = new NuGetVersion("12.0.1");

            using var packageStream = await client.DownloadPackageAsync(packageId, packageVersion);
            Console.WriteLine($"Downloaded package {packageId} {packageVersion}");
        }

        [Fact]
        public async Task DownloadPackageManifest()
        {
            // Downloads a package manifest (.nuspec)
            var client = new NuGetClient("https://api.nuget.org/v3/index.json");

            var packageId = "Newtonsoft.Json";
            var packageVersion = new NuGetVersion("12.0.1");

            using var manifestStream = await client.DownloadPackageManifestAsync(packageId, packageVersion);
            Console.WriteLine($"Downloaded package {packageId} {packageVersion}'s nuspec");
        }
    }
}
