using System.Text;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace NaGet.Core.Tests.Services
{
    public class FileStorageServiceTests
    {
        public class GetAsync : FactsBase
        {
            [Fact]
            public async Task ThrowsIfStorePathDoesNotExist()
            {
                await Assert.ThrowsAsync<DirectoryNotFoundException>(() =>
                    target.Get("hello.txt"));
            }

            [Fact]
            public async Task ThrowsIfFileDoesNotExist()
            {
                // Ensure the store path exists.
                Directory.CreateDirectory(storePath);

                await Assert.ThrowsAsync<FileNotFoundException>(() =>
                    target.Get("hello.txt"));

                await Assert.ThrowsAsync<DirectoryNotFoundException>(() =>
                    target.Get("hello/world.txt"));
            }

            [Fact]
            public async Task GetsStream()
            {
                // Arrange
                using (var content = StringStream("Hello world"))
                {
                    await target.Put("hello.txt", content, "text/plain");
                }

                // Act
                var result = await target.Get("hello.txt");

                // Assert
                Assert.NotNull(result);
                Assert.Equal("Hello world", await ToStringAsync(result!));
            }

            [Fact]
            public async Task NoAccessOutsideStorePath()
            {
                foreach (var path in OutsideStorePathData)
                {
                    await Assert.ThrowsAsync<ArgumentException>(async () =>
                        await target.Get(path));
                }
            }
        }

        public class GetDownloadUriAsync : FactsBase
        {
            [Fact]
            public async Task CreatesUriEvenIfDoesntExist()
            {
                var result = await target.GetDownloadUri("test.txt");
                var expected = new Uri(Path.Combine(storePath, "test.txt"));

                Assert.Equal(expected, result);
            }

            [Fact]
            public async Task NoAccessOutsideStorePath()
            {
                foreach (var path in OutsideStorePathData)
                {
                    await Assert.ThrowsAsync<ArgumentException>(async () =>
                        await target.GetDownloadUri(path));
                }
            }
        }

        public class PutAsync : FactsBase
        {
            [Fact]
            public async Task SavesContent()
            {
                StoragePutResult result;
                using (var content = StringStream("Hello world"))
                {
                    result = await target.Put("test.txt", content, "text/plain");
                }

                var path = Path.Combine(storePath, "test.txt");

                Assert.True(File.Exists(path));
                Assert.Equal(StoragePutResult.Success, result);
                Assert.Equal("Hello world", await File.ReadAllTextAsync(path));
            }

            [Fact]
            public async Task ReturnsAlreadyExistsIfContentAlreadyExists()
            {
                // Arrange
                var path = Path.Combine(storePath, "test.txt");

                Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);
                File.WriteAllText(path, "Hello world");

                StoragePutResult result;
                using (var content = StringStream("Hello world"))
                {
                    // Act
                    result = await target.Put("test.txt", content, "text/plain");
                }

                // Assert
                Assert.Equal(StoragePutResult.AlreadyExists, result);
            }

            [Fact]
            public async Task ReturnsConflictIfContentAlreadyExistsButContentsDoNotMatch()
            {
                // Arrange
                var path = Path.Combine(storePath, "test.txt");

                Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);
                File.WriteAllText(path, "Hello world");

                StoragePutResult result;
                using (var content = StringStream("foo bar"))
                {
                    // Act
                    result = await target.Put("test.txt", content, "text/plain");
                }

                // Assert
                Assert.Equal(StoragePutResult.Conflict, result);
            }

            [Fact]
            public async Task NoAccessOutsideStorePath()
            {
                foreach (var path in OutsideStorePathData)
                {
                    using var content = StringStream("Hello world");
                    await Assert.ThrowsAsync<ArgumentException>(async () =>
                        await target.Put(path, content, "text/plain"));
                }
            }
        }

        public class DeleteAsync : FactsBase
        {
            [Fact]
            public async Task DoesNotThrowIfPathDoesNotExist()
            {
                await target.Delete("test.txt");
            }

            [Fact]
            public async Task Deletes()
            {
                // Arrange
                var path = Path.Combine(storePath, "test.txt");

                Directory.CreateDirectory(storePath);
                await File.WriteAllTextAsync(path, "Hello world");

                // Act & Assert
                await target.Delete("test.txt");

                Assert.False(File.Exists(path));
            }

            [Fact]
            public async Task NoAccessOutsideStorePath()
            {
                foreach (var path in OutsideStorePathData)
                {
                    await Assert.ThrowsAsync<ArgumentException>(async () =>
                        await target.Delete(path));
                }
            }
        }

        public class FactsBase : IDisposable
        {
            protected readonly string storePath;
            protected readonly Mock<IOptionsSnapshot<FileSystemStorageOptions>> _options;
            protected readonly FileStorageService target;

            public FactsBase()
            {
                storePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
                _options = new Mock<IOptionsSnapshot<FileSystemStorageOptions>>();

                _options
                    .Setup(o => o.Value)
                    .Returns(() => new FileSystemStorageOptions { Path = storePath });

                target = new FileStorageService(_options.Object);
            }

            public void Dispose()
            {
                try
                {
                    Directory.Delete(storePath, recursive: true);
                }
                catch (DirectoryNotFoundException)
                {
                }
            }

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

            public IEnumerable<string> OutsideStorePathData
            {
                get
                {
                    var fullPath = Path.GetFullPath(storePath) ?? string.Empty;
                    var rootPath = Path.GetPathRoot(storePath) ?? string.Empty;
                    yield return "../file";
                    yield return ".";
                    yield return $"../{Path.GetFileName(storePath)}";
                    yield return $"../{Path.GetFileName(storePath)}suffix";
                    yield return $"../{Path.GetFileName(storePath)}suffix/file";
                    yield return fullPath;
                    yield return fullPath + Path.DirectorySeparatorChar;
                    yield return fullPath + Path.DirectorySeparatorChar + "..";
                    yield return fullPath + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "file";
                    yield return $"{rootPath}";
                    yield return Path.Combine(rootPath, "file");
                }
            }
        }
    }
}
