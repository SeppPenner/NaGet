using NaGet.Core;
using NaGet.Protocol.Models;
using Moq;
using Xunit;

namespace NaGet.Web.Tests
{
    public class IndexModelFacts
    {
        private readonly IndexModel target;

        private SearchRequest? capturedRequest = null;
        private readonly SearchResponse response = new SearchResponse();
        private readonly CancellationToken cancellation = CancellationToken.None;

        public IndexModelFacts()
        {
            var search = new Mock<ISearchService>();
            search
                .Setup(s => s.Search(It.IsAny<SearchRequest>(), cancellation))
                .Callback((SearchRequest r, CancellationToken c) => capturedRequest = r)
                .ReturnsAsync(response);

            target = new IndexModel(search.Object);
        }

        [Fact]
        public async Task DefaultSearch()
        {
            await target.OnGetAsync(cancellation);

            Assert.NotNull(capturedRequest);
            Assert.Equal(0, capturedRequest.Skip);
            Assert.Equal(20, capturedRequest.Take);
            Assert.True(capturedRequest.IncludePrerelease);
            Assert.True(capturedRequest.IncludeSemVer2);
            Assert.Null(capturedRequest.PackageType);
            Assert.Null(capturedRequest.Framework);
            Assert.Null(capturedRequest.Query);
        }

        [Fact]
        public async Task AcceptsParameters()
        {
            target.Prerelease = false;
            target.PageIndex = 5;
            target.PackageType = "foo";
            target.Framework = "bar";
            target.Query = "Hello world";

            await target.OnGetAsync(cancellation);

            Assert.NotNull(capturedRequest);
            Assert.Equal(80, capturedRequest.Skip);
            Assert.Equal(20, capturedRequest.Take);
            Assert.False(capturedRequest.IncludePrerelease);
            Assert.True(capturedRequest.IncludeSemVer2);
            Assert.Equal("foo", capturedRequest.PackageType);
            Assert.Equal("bar", capturedRequest.Framework);
            Assert.Equal("Hello world", capturedRequest.Query);
        }
    }
}
