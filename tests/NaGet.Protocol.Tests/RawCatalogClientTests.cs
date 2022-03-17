using System;
using System.Linq;
using System.Threading.Tasks;
using NaGet.Protocol.Internal;
using NaGet.Protocol.Models;
using Xunit;

namespace NaGet.Protocol.Tests
{
    public class RawCatalogClientTests : IClassFixture<ProtocolFixture>
    {
        private readonly RawCatalogClient target;

        public RawCatalogClientTests(ProtocolFixture fixture)
        {
            target = fixture.CatalogClient;
        }

        [Fact]
        public async Task GetCatalogIndex()
        {
            var result = await target.GetIndexAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result!.Count);
            Assert.Equal(2, result!.Items.Count);
            Assert.Equal(TestData.CatalogPageUrl, result!.Items.Select(i => i.CatalogPageUrl).First());
        }

        [Fact]
        public async Task GetCatalogPage()
        {
            var page = await target.GetPageAsync(TestData.CatalogPageUrl);

            Assert.NotNull(page);
            Assert.Equal(2, page!.Count);
            Assert.Equal(2, page!.Items.Count);
            Assert.Equal(TestData.CatalogIndexUrl, page!.CatalogIndexUrl);
            Assert.Equal(TestData.PackageDetailsCatalogLeafUrl, page!.Items[0].CatalogLeafUrl);
            Assert.Equal(TestData.PackageDeleteCatalogLeafUrl, page!.Items[1].CatalogLeafUrl);
        }

        [Fact]
        public async Task GetPackageDetailsLeaf()
        {
            var leaf = await target.GetPackageDetailsLeafAsync(TestData.PackageDetailsCatalogLeafUrl);

            Assert.Equal(TestData.PackageDetailsCatalogLeafUrl, leaf.CatalogLeafUrl);
            Assert.Equal("PackageDetails", leaf.Type[0]);
            Assert.Equal("catalog:Permalink", leaf.Type[1]);

            Assert.Equal("Test.Package", leaf.PackageId);
            Assert.Equal("1.0.0", leaf.PackageVersion);
        }

        [Fact]
        public async Task GetPackageDeleteLeaf()
        {
            var leaf = await target.GetPackageDeleteLeafAsync(TestData.PackageDeleteCatalogLeafUrl);

            Assert.Equal(TestData.PackageDeleteCatalogLeafUrl, leaf.CatalogLeafUrl);
            Assert.Equal("PackageDelete", leaf.Type[0]);
            Assert.Equal("catalog:Permalink", leaf.Type[1]);

            Assert.Equal("Deleted.Package", leaf.PackageId);
            Assert.Equal("1.0.0", leaf.PackageVersion);
        }

        [Fact]
        public async Task ThrowsOnTypeMismatch()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => target.GetPackageDetailsLeafAsync(TestData.PackageDeleteCatalogLeafUrl));
            await Assert.ThrowsAsync<ArgumentException>(() => target.GetPackageDeleteLeafAsync(TestData.PackageDetailsCatalogLeafUrl));
        }
    }
}
