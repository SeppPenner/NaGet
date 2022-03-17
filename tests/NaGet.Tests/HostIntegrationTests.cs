using NaGet.Core;
using NaGet.Database.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace NaGet.Tests
{
    public class HostIntegrationTests
    {
        private readonly string DatabaseTypeKey = "Database:Type";
        private readonly string ConnectionStringKey = "Database:ConnectionString";

        [Fact]
        public void ThrowsIfDatabaseTypeInvalid()
        {
            var provider = BuildServiceProvider(new Dictionary<string, string>
            {
                { DatabaseTypeKey, "InvalidType" }
            });

            Assert.Throws<InvalidOperationException>(
                () => provider.GetRequiredService<IContext>());
        }

        [Fact]
        public void ReturnsDatabaseContext()
        {
            var provider = BuildServiceProvider(new Dictionary<string, string>
            {
                { DatabaseTypeKey, "Sqlite" },
                { ConnectionStringKey, "..." }
            });

            Assert.NotNull(provider.GetRequiredService<IContext>());
        }

        [Fact]
        public void ReturnsSqliteContext()
        {
            var provider = BuildServiceProvider(new Dictionary<string, string>
            {
                { DatabaseTypeKey, "Sqlite" },
                { ConnectionStringKey, "..." }
            });

            Assert.NotNull(provider.GetRequiredService<SqliteContext>());
        }

        [Fact]
        public void DefaultsToSqlite()
        {
            var provider = BuildServiceProvider(new Dictionary<string, string>());

            var context = provider.GetRequiredService<IContext>();

            Assert.IsType<SqliteContext>(context);
        }

        private IServiceProvider BuildServiceProvider(Dictionary<string, string> configs)
        {
            var host = Program
                .CreateHostBuilder(Array.Empty<string>())
                .ConfigureAppConfiguration((ctx, config) =>
                {
                    config.AddInMemoryCollection(configs);
                })
                .Build();

            return host.Services;
        }
    }
}
