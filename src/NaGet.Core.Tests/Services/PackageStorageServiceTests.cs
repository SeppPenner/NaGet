using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using NuGet.Versioning;
using Xunit;

namespace NaGet.Core.Tests.Services
{
    public class PackageStorageServiceTests
    {
        public class SavePackageContentAsync : FactsBase
        {
            [Fact]
            public async Task ThrowsIfPackageIsNull()
            {
                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => target.SavePackageContentAsync(
                        null,
                        packageStream: Stream.Null,
                        nuspecStream: Stream.Null,
                        readmeStream: Stream.Null,
                        iconStream: Stream.Null));
            }

            [Fact]
            public async Task ThrowsIfPackageStreamIsNull()
            {
                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => target.SavePackageContentAsync(
                        package,
                        packageStream: null,
                        nuspecStream: Stream.Null,
                        readmeStream: Stream.Null,
                        iconStream: Stream.Null));
            }

            [Fact]
            public async Task ThrowsIfNuspecStreamIsNull()
            {
                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => target.SavePackageContentAsync(
                        package,
                        packageStream: Stream.Null,
                        nuspecStream: null,
                        readmeStream: Stream.Null,
                        iconStream: Stream.Null));
            }

            [Fact]
            public async Task SavesContent()
            {
                // Arrange
                SetupPutResult(StoragePutResult.Success);

                using (var packageStream = StringStream("My package"))
                using (var nuspecStream = StringStream("My nuspec"))
                using (var readmeStream = StringStream("My readme"))
                using (var iconStream = StringStream("My icon"))
                {
                    // Act
                    await target.SavePackageContentAsync(
                        package,
                        packageStream: packageStream,
                        nuspecStream: nuspecStream,
                        readmeStream: readmeStream,
                        iconStream: iconStream);

                    // Assert
                    Assert.True(puts.ContainsKey(PackagePath));
                    Assert.Equal("My package", await ToStringAsync(puts[PackagePath].Content));
                    Assert.Equal("binary/octet-stream", puts[PackagePath].ContentType);

                    Assert.True(puts.ContainsKey(NuspecPath));
                    Assert.Equal("My nuspec", await ToStringAsync(puts[NuspecPath].Content));
                    Assert.Equal("text/plain", puts[NuspecPath].ContentType);

                    Assert.True(puts.ContainsKey(ReadmePath));
                    Assert.Equal("My readme", await ToStringAsync(puts[ReadmePath].Content));
                    Assert.Equal("text/markdown", puts[ReadmePath].ContentType);

                    Assert.True(puts.ContainsKey(IconPath));
                    Assert.Equal("My icon", await ToStringAsync(puts[IconPath].Content));
                    Assert.Equal("image/xyz", puts[IconPath].ContentType);
                }
            }

            [Fact]
            public async Task DoesNotSaveReadmeIfItIsNull()
            {
                // Arrange
                SetupPutResult(StoragePutResult.Success);

                using (var packageStream = StringStream("My package"))
                using (var nuspecStream = StringStream("My nuspec"))
                {
                    // Act
                    await target.SavePackageContentAsync(
                        package,
                        packageStream: packageStream,
                        nuspecStream: nuspecStream,
                        readmeStream: null,
                        iconStream: null);
                }

                // Assert
                Assert.False(puts.ContainsKey(ReadmePath));
            }

            [Fact]
            public async Task NormalizesVersionWhenContentIsSaved()
            {
                // Arrange
                SetupPutResult(StoragePutResult.Success);

                package.Version = new NuGetVersion("1.2.3.0");
                using (var packageStream = StringStream("My package"))
                using (var nuspecStream = StringStream("My nuspec"))
                using (var readmeStream = StringStream("My readme"))
                using (var iconStream = StringStream("My icon"))
                {
                    // Act
                    await target.SavePackageContentAsync(
                        package,
                        packageStream: packageStream,
                        nuspecStream: nuspecStream,
                        readmeStream: readmeStream,
                        iconStream: iconStream);
                }

                // Assert
                Assert.True(puts.ContainsKey(PackagePath));
                Assert.True(puts.ContainsKey(NuspecPath));
                Assert.True(puts.ContainsKey(ReadmePath));
            }

            [Fact]
            public async Task DoesNotThrowIfContentAlreadyExistsAndContentsMatch()
            {
                // Arrange
                SetupPutResult(StoragePutResult.AlreadyExists);

                using (var packageStream = StringStream("My package"))
                using (var nuspecStream = StringStream("My nuspec"))
                using (var readmeStream = StringStream("My readme"))
                using (var iconStream = StringStream("My icon"))
                {
                    await target.SavePackageContentAsync(
                        package,
                        packageStream: packageStream,
                        nuspecStream: nuspecStream,
                        readmeStream: readmeStream,
                        iconStream: iconStream);

                    // Assert
                    Assert.True(puts.ContainsKey(PackagePath));
                    Assert.Equal("My package", await ToStringAsync(puts[PackagePath].Content));
                    Assert.Equal("binary/octet-stream", puts[PackagePath].ContentType);

                    Assert.True(puts.ContainsKey(NuspecPath));
                    Assert.Equal("My nuspec", await ToStringAsync(puts[NuspecPath].Content));
                    Assert.Equal("text/plain", puts[NuspecPath].ContentType);

                    Assert.True(puts.ContainsKey(ReadmePath));
                    Assert.Equal("My readme", await ToStringAsync(puts[ReadmePath].Content));
                    Assert.Equal("text/markdown", puts[ReadmePath].ContentType);

                    Assert.True(puts.ContainsKey(IconPath));
                    Assert.Equal("My icon", await ToStringAsync(puts[IconPath].Content));
                    Assert.Equal("image/xyz", puts[IconPath].ContentType);
                }
            }

            [Fact]
            public async Task ThrowsIfContentAlreadyExistsButContentsDoNotMatch()
            {
                // Arrange
                SetupPutResult(StoragePutResult.Conflict);

                using (var packageStream = StringStream("My package"))
                using (var nuspecStream = StringStream("My nuspec"))
                using (var readmeStream = StringStream("My readme"))
                using (var iconStream = StringStream("My icon"))
                {
                    // Act
                    await Assert.ThrowsAsync<InvalidOperationException>(() =>
                        target.SavePackageContentAsync(
                            package,
                            packageStream: packageStream,
                            nuspecStream: nuspecStream,
                            readmeStream: readmeStream,
                            iconStream: iconStream));
                }
            }
        }

        public class GetPackageStreamAsync : FactsBase
        {
            [Fact]
            public async Task ThrowsIfStorageThrows()
            {
                // Arrange
                var cancellationToken = CancellationToken.None;
                storage
                    .Setup(s => s.GetAsync(PackagePath, cancellationToken))
                    .ThrowsAsync(new DirectoryNotFoundException());

                // Act
                await Assert.ThrowsAsync<DirectoryNotFoundException>(() =>
                    target.GetPackageStreamAsync(package.Id, package.Version, cancellationToken));
            }

            [Fact]
            public async Task GetsStream()
            {
                // Arrange
                var cancellationToken = CancellationToken.None;
                using var packageStream = StringStream("My package");
                storage
                    .Setup(s => s.GetAsync(PackagePath, cancellationToken))
                    .ReturnsAsync(packageStream);

                // Act
                var result = await target.GetPackageStreamAsync(package.Id, package.Version, cancellationToken);

                // Assert
                Assert.NotNull(result);
                Assert.Equal("My package", await ToStringAsync(result!));

                storage.Verify(s => s.GetAsync(PackagePath, cancellationToken), Times.Once);
            }
        }

        public class GetNuspecStreamAsync : FactsBase
        {
            [Fact]
            public async Task ThrowsIfDoesntExist()
            {
                // Arrange
                var cancellationToken = CancellationToken.None;
                storage
                    .Setup(s => s.GetAsync(NuspecPath, cancellationToken))
                    .ThrowsAsync(new DirectoryNotFoundException());

                // Act
                await Assert.ThrowsAsync<DirectoryNotFoundException>(() =>
                    target.GetNuspecStreamAsync(package.Id, package.Version, cancellationToken));
            }

            [Fact]
            public async Task GetsStream()
            {
                // Arrange
                var cancellationToken = CancellationToken.None;
                using var nuspecStream = StringStream("My nuspec");
                storage
                    .Setup(s => s.GetAsync(NuspecPath, cancellationToken))
                    .ReturnsAsync(nuspecStream);

                // Act
                var result = await target.GetNuspecStreamAsync(package.Id, package.Version, cancellationToken);

                // Assert
                Assert.NotNull(result);
                Assert.Equal("My nuspec", await ToStringAsync(result!));

                storage.Verify(s => s.GetAsync(NuspecPath, cancellationToken), Times.Once);
            }
        }

        public class GetReadmeStreamAsync : FactsBase
        {
            [Fact]
            public async Task ThrowsIfDoesntExist()
            {
                // Arrange
                var cancellationToken = CancellationToken.None;
                storage
                    .Setup(s => s.GetAsync(ReadmePath, cancellationToken))
                    .ThrowsAsync(new DirectoryNotFoundException());

                // Act
                await Assert.ThrowsAsync<DirectoryNotFoundException>(() =>
                    target.GetReadmeStreamAsync(package.Id, package.Version, cancellationToken));
            }

            [Fact]
            public async Task GetsStream()
            {
                // Arrange
                var cancellationToken = CancellationToken.None;
                using (var readmeStream = StringStream("My readme"))
                {
                    storage
                        .Setup(s => s.GetAsync(ReadmePath, cancellationToken))
                        .ReturnsAsync(readmeStream);

                    // Act
                    var result = await target.GetReadmeStreamAsync(package.Id, package.Version, cancellationToken);

                    // Assert
                    Assert.NotNull(result);
                    Assert.Equal("My readme", await ToStringAsync(result!));

                    storage.Verify(s => s.GetAsync(ReadmePath, cancellationToken), Times.Once);
                }
            }
        }

        public class DeleteAsync : FactsBase
        {
            [Fact]
            public async Task Deletes()
            {
                // Act
                var cancellationToken = CancellationToken.None;
                await target.DeleteAsync(package.Id, package.Version, cancellationToken);

                storage.Verify(s => s.DeleteAsync(PackagePath, cancellationToken), Times.Once);
                storage.Verify(s => s.DeleteAsync(NuspecPath, cancellationToken), Times.Once);
                storage.Verify(s => s.DeleteAsync(ReadmePath, cancellationToken), Times.Once);
            }
        }

        public class FactsBase
        {
            protected readonly Package package = new()
            {
                Id = "My.Package",
                Version = new NuGetVersion("1.2.3")
            };

            protected readonly Mock<IStorageService> storage;
            protected readonly PackageStorageService target;

            protected readonly Dictionary<string, (Stream Content, string ContentType)> puts;

            public FactsBase()
            {
                storage = new Mock<IStorageService>();
                target = new PackageStorageService(storage.Object, Mock.Of<ILogger<PackageStorageService>>());
                puts = new Dictionary<string, (Stream Content, string ContentType)>();
            }

            protected string PackagePath => Path.Combine("packages", "my.package", "1.2.3", "my.package.1.2.3.nupkg");
            protected string NuspecPath => Path.Combine("packages", "my.package", "1.2.3", "my.package.nuspec");
            protected string ReadmePath => Path.Combine("packages", "my.package", "1.2.3", "readme");
            protected string IconPath => Path.Combine("packages", "my.package", "1.2.3", "icon");

            protected Stream StringStream(string input)
            {
                var bytes = Encoding.ASCII.GetBytes(input);
                return new MemoryStream(bytes);
            }

            protected async Task<string> ToStringAsync(Stream input)
            {
                using var reader = new StreamReader(input);
                return await reader.ReadToEndAsync();
            }

            protected void SetupPutResult(StoragePutResult result)
            {
                storage
                    .Setup(
                        s => s.PutAsync(
                            It.IsAny<string>(),
                            It.IsAny<Stream>(),
                            It.IsAny<string>(),
                            It.IsAny<CancellationToken>()))
                    .Callback((string path, Stream content, string contentType, CancellationToken cancellationToken) =>
                    {
                        puts[path] = (content, contentType);
                    })
                    .ReturnsAsync(result);
            }
        }
    }
}
