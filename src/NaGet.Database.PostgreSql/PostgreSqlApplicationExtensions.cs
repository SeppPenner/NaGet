using System;
using NaGet.Core;
using NaGet.Database.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace NaGet
{
    public static class PostgreSqlApplicationExtensions
    {
        public static NaGetApplication AddPostgreSqlDatabase(this NaGetApplication app)
        {
            app.Services.AddNaGetDbContextProvider<PostgreSqlContext>("PostgreSql", (provider, options) =>
            {
                var databaseOptions = provider.GetRequiredService<IOptionsSnapshot<DatabaseOptions>>();

                options.UseNpgsql(databaseOptions.Value.ConnectionString);
            });

            return app;
        }

        public static NaGetApplication AddPostgreSqlDatabase(
            this NaGetApplication app,
            Action<DatabaseOptions> configure)
        {
            app.AddPostgreSqlDatabase();
            app.Services.Configure(configure);
            return app;
        }
    }
}
