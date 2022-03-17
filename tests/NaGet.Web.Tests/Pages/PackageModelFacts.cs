using NaGet.Core;
using Moq;
using NuGet.Versioning;
using Xunit;

namespace NaGet.Web.Tests
{
    public class PackageModelFacts
    {
        private readonly Mock<IPackageContentService> content;
        private readonly Mock<IPackageService> packages;
        private readonly Mock<ISearchService> search;
        private readonly Mock<IUrlGenerator> url;
        private readonly PackageModel target;

        private readonly CancellationToken cancellation = CancellationToken.None;

        public PackageModelFacts()
        {
            content = new Mock<IPackageContentService>();
            packages = new Mock<IPackageService>();
            search = new Mock<ISearchService>();
            url = new Mock<IUrlGenerator>();
            target = new PackageModel(
                packages.Object,
                content.Object,
                search.Object,
                url.Object);

            search
                .Setup(s => s.FindDependentsAsync("testpackage", cancellation))
                .ReturnsAsync(new DependentsResponse());
        }

        [Fact]
        public async Task ReturnsNotFound()
        {
            packages
                .Setup(m => m.FindPackagesAsync("testpackage", cancellation))
                .ReturnsAsync(new List<Package>());

            await target.OnGetAsync("testpackage", "1.0.0", cancellation);

            Assert.False(target.Found);
            Assert.NotNull(target.Package);
            Assert.Equal("testpackage", target.Package!.Id);
            Assert.Null(target.DependencyGroups);
            Assert.Null(target.Versions);
        }

        [Fact]
        public async Task ReturnsNotFoundIfAllUnlisted()
        {
            packages
                .Setup(m => m.FindPackagesAsync("testpackage", cancellation))
                .ReturnsAsync(new List<Package>
                {
                    CreatePackage("1.0.0", listed: false),
                });

            await target.OnGetAsync("testpackage", version: null, cancellation);

            Assert.False(target.Found);
            Assert.NotNull(target.Package);
            Assert.Equal("testpackage", target.Package!.Id);
            Assert.Null(target.DependencyGroups);
            Assert.Null(target.Versions);
        }

        [Fact]
        public async Task ReturnsRequestedVersion()
        {
            packages
                .Setup(m => m.FindPackagesAsync("testpackage", cancellation))
                .ReturnsAsync(new List<Package>
                {
                    CreatePackage("1.0.0"),
                    CreatePackage("2.0.0"),
                    CreatePackage("3.0.0"),
                });

            await target.OnGetAsync("testpackage", "2.0.0", cancellation);

            Assert.True(target.Found);
            Assert.NotNull(target.Package);
            Assert.Equal("testpackage", target.Package!.Id);
            Assert.Equal("2.0.0", target.Package!.NormalizedVersionString);

            Assert.Equal(3, target.Versions.Count);
            Assert.NotNull(target.Versions[0].Version);
            Assert.Equal("3.0.0", target.Versions[0].Version!.OriginalVersion);
            Assert.False(target.Versions[0].Selected);
            Assert.NotNull(target.Versions[1].Version);
            Assert.Equal("2.0.0", target.Versions[1].Version!.OriginalVersion);
            Assert.True(target.Versions[1].Selected);
            Assert.NotNull(target.Versions[2].Version);
            Assert.Equal("1.0.0", target.Versions[2].Version!.OriginalVersion);
            Assert.False(target.Versions[2].Selected);
        }

        [Fact]
        public async Task ReturnsRequestedUnlistedVersion()
        {
            packages
                .Setup(m => m.FindPackagesAsync("testpackage", cancellation))
                .ReturnsAsync(new List<Package>
                {
                    CreatePackage("1.0.0"),
                    CreatePackage("2.0.0", listed: false),
                    CreatePackage("3.0.0"),
                });

            await target.OnGetAsync("testpackage", "2.0.0", cancellation);

            Assert.True(target.Found);
            Assert.NotNull(target.Package);
            Assert.Equal("testpackage", target.Package!.Id);
            Assert.Equal("2.0.0", target.Package!.NormalizedVersionString);

            Assert.Equal(2, target.Versions.Count);
            Assert.NotNull(target.Versions[0].Version);
            Assert.Equal("3.0.0", target.Versions[0].Version!.OriginalVersion);
            Assert.False(target.Versions[0].Selected);
            Assert.NotNull(target.Versions[1].Version);
            Assert.Equal("1.0.0", target.Versions[1].Version!.OriginalVersion);
            Assert.False(target.Versions[1].Selected);
        }

        [Fact]
        public async Task FallsBackToLatestListedVersion()
        {
            packages
                .Setup(m => m.FindPackagesAsync("testpackage", cancellation))
                .ReturnsAsync(new List<Package>
                {
                    CreatePackage("1.0.0"),
                    CreatePackage("2.0.0"),
                    CreatePackage("3.0.0", listed: false),
                });

            await target.OnGetAsync("testpackage", "4.0.0", cancellation);

            Assert.True(target.Found);
            Assert.NotNull(target.Package);
            Assert.Equal("testpackage", target.Package!.Id);
            Assert.Equal("2.0.0", target.Package.NormalizedVersionString);

            Assert.Equal(2, target.Versions.Count);
            Assert.NotNull(target.Versions[0].Version);
            Assert.Equal("2.0.0", target.Versions[0].Version!.OriginalVersion);
            Assert.True(target.Versions[0].Selected);
            Assert.NotNull(target.Versions[1].Version);
            Assert.Equal("1.0.0", target.Versions[1].Version!.OriginalVersion);
            Assert.False(target.Versions[1].Selected);
        }

        [Theory]
        [InlineData(new[] { "test" }, /*expectDotnetTemplate: */ false, /*expectDotnetTool: */ false)]
        [InlineData(new[] { "template" }, /*expectDotnetTemplate: */ true, /*expectDotnetTool: */ false)]
        [InlineData(new[] { "dOtNeTtOoL" }, /*expectDotnetTemplate: */ false, /*expectDotnetTool: */ true)]

        [InlineData(new[] { "tEmPlAte", "dOtNeTtOoL" }, /*expectDotnetTemplate: */ true, /*expectDotnetTool: */ true)]
        public async Task HandlesPackageTypes(IEnumerable<string> packageTypes, bool expectDotnetTemplate, bool expectDotnetTool)
        {
            packages
                .Setup(m => m.FindPackagesAsync("testpackage", cancellation))
                .ReturnsAsync(new List<Package>
                {
                    CreatePackage("1.0.0", packageTypes: packageTypes)
                });

            await target.OnGetAsync("testpackage", "1.0.0", cancellation);

            Assert.True(target.Found);
            Assert.Equal(expectDotnetTemplate, target.IsDotnetTemplate);
            Assert.Equal(expectDotnetTool, target.IsDotnetTool);
        }

        [Fact]
        public async Task FindsDependentPackages()
        {
            packages
                .Setup(m => m.FindPackagesAsync("testpackage", cancellation))
                .ReturnsAsync(new List<Package>
                {
                    CreatePackage("1.0.0")
                });

            search
                .Setup(s => s.FindDependentsAsync("testpackage", cancellation))
                .ReturnsAsync(new DependentsResponse
                {
                    Data = new List<PackageDependent>
                    {
                        new PackageDependent  { Id = "Used by 1" },
                        new PackageDependent  { Id = "Used by 2" },
                    }
                });

            await target.OnGetAsync("testpackage", "1.0.0", cancellation);

            Assert.Equal(2, target.UsedBy.Count);
            Assert.Equal("Used by 1", target.UsedBy[0].Id);
            Assert.Equal("Used by 2", target.UsedBy[1].Id);
        }

        [Fact]
        public async Task GroupsVersions()
        {
            packages
                .Setup(m => m.FindPackagesAsync("testpackage", cancellation))
                .ReturnsAsync(new List<Package>
                {
                    CreatePackage("1.0.0", dependencies: new[]
                    {
                        new PackageDependency
                        {
                            TargetFramework = "net5.0",
                            Id = "Dependency1",
                            VersionRange = "[1.0.0, )",
                        },
                        new PackageDependency
                        {
                            TargetFramework = "net4.8",
                            Id = "Dependency2",
                            VersionRange = "[2.0.0, )",
                        },
                        new PackageDependency
                        {
                            TargetFramework = "net5.0",
                            Id = "Dependency3",
                            VersionRange = "[3.0.0, )",
                        },
                    })
                });

            await target.OnGetAsync("testpackage", "1.0.0", cancellation);

            Assert.True(target.Found);
            Assert.Equal(2, target.DependencyGroups.Count);
            Assert.Equal(".NET 5.0", target.DependencyGroups[0].Name);
            Assert.Equal(".NET Framework 4.8", target.DependencyGroups[1].Name);

            Assert.Equal(2, target.DependencyGroups[0].Dependencies.Count);
            Assert.Equal(1, target.DependencyGroups[1].Dependencies.Count);

            Assert.Equal("Dependency1", target.DependencyGroups[0].Dependencies[0].PackageId);
            Assert.Equal("(>= 1.0.0)", target.DependencyGroups[0].Dependencies[0].VersionSpec);

            Assert.Equal("Dependency3", target.DependencyGroups[0].Dependencies[1].PackageId);
            Assert.Equal("(>= 3.0.0)", target.DependencyGroups[0].Dependencies[1].VersionSpec);

            Assert.Equal("Dependency2", target.DependencyGroups[1].Dependencies[0].PackageId);
            Assert.Equal("(>= 2.0.0)", target.DependencyGroups[1].Dependencies[0].VersionSpec);
        }

        [Theory]
        [InlineData(null, "All Frameworks")]
        [InlineData("net5.0", ".NET 5.0")]
        [InlineData("netstandard2.1", ".NET Standard 2.1")]
        [InlineData("netcoreapp3.1", ".NET Core 3.1")]
        [InlineData("net4.8", ".NET Framework 4.8")]
        public async Task PrettifiesTargetFramework(string targetFramework, string expectedResult)
        {
            packages
                .Setup(m => m.FindPackagesAsync("testpackage", cancellation))
                .ReturnsAsync(new List<Package>
                {
                    CreatePackage("1.0.0", dependencies: new[]
                    {
                       new PackageDependency
                       {
                           TargetFramework = targetFramework,
                           Id = "DependencyPackage",
                           VersionRange = "[1.0.0, )",
                       }
                    })
                });

            await target.OnGetAsync("testpackage", "1.0.0", cancellation);

            Assert.True(target.Found);
            var group = Assert.Single(target.DependencyGroups);
            Assert.Equal(expectedResult, group.Name);
        }

        [Fact]
        public async Task StatisticsIncludeUnlistedPackages()
        {
            var now = DateTime.Now;

            packages
                .Setup(m => m.FindPackagesAsync("testpackage", cancellation))
                .ReturnsAsync(new List<Package>
                {
                    CreatePackage("1.0.0", downloads: 10, published: DateTime.Now.AddDays(-2)),
                    CreatePackage("2.0.0", listed: false, downloads: 5, published: now),
                });

            await target.OnGetAsync("testpackage", "1.0.0", cancellation);

            Assert.True(target.Found);
            Assert.Equal(15, target.TotalDownloads);
            Assert.Equal(now, target.LastUpdated);
        }

        [Fact]
        public async Task RendersReadme()
        {
            using var readmeStream = new MemoryStream();
            using (var streamWriter = new StreamWriter(readmeStream, leaveOpen: true))
            {
                await streamWriter.WriteLineAsync("# My readme");
                await streamWriter.WriteLineAsync("Hello world!");
                await streamWriter.FlushAsync();
            }

            readmeStream.Position = 0;

            packages
                .Setup(m => m.FindPackagesAsync("testpackage", cancellation))
                .ReturnsAsync(new List<Package>
                {
                        CreatePackage("1.0.0", hasReadme: true),
                });

            content
                .Setup(c => c.GetPackageReadmeStreamOrNullAsync(
                    "testpackage",
                    It.Is<NuGetVersion>(v => v.OriginalVersion == "1.0.0"),
                    cancellation))
                .ReturnsAsync(readmeStream);

            await target.OnGetAsync("testpackage", "1.0.0", cancellation);

            Assert.NotNull(target.Readme);
            Assert.Equal(
                "<h1 id=\"my-readme\">My readme</h1>\n<p>Hello world!</p>\n",
                target.Readme!.Value);
        }

        private Package CreatePackage(
            string version,
            long downloads = 0,
            bool hasReadme = false,
            bool listed = true,
            DateTime? published = null,
            IEnumerable<PackageDependency>? dependencies = null,
            IEnumerable<string>? packageTypes = null)
        {
            published = published ?? DateTime.Now;
            dependencies = dependencies ?? Array.Empty<PackageDependency>();
            packageTypes = packageTypes ?? Array.Empty<string>();

            return new Package
            {
                Id = "testpackage",
                Downloads = downloads,
                HasReadme = hasReadme,
                Listed = listed,
                NormalizedVersionString = version,
                Published = published.Value,

                Dependencies = dependencies.ToList(),
                PackageTypes = packageTypes
                    .Select(name => new PackageType { Name = name })
                    .ToList(),
            };
        }
    }
}
