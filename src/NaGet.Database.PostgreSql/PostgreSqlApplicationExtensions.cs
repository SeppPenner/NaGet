// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PostgreSqlApplicationExtensions.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The PostgreSQL application extensions class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet;

/// <summary>
/// The PostgreSQL application extensions class.
/// </summary>
public static class PostgreSqlApplicationExtensions
{
    /// <summary>
    /// Adds the PostgreSQL database.
    /// </summary>
    /// <param name="app">The NaGet application.</param>
    /// <returns>The <see cref="NaGetApplication"/>.</returns>
    public static NaGetApplication AddPostgreSqlDatabase(this NaGetApplication app)
    {
        app.Services.AddNaGetDbContextProvider<PostgreSqlContext>("PostgreSql", (provider, options) =>
        {
            var databaseOptions = provider.GetRequiredService<IOptionsSnapshot<DatabaseOptions>>();
            options.UseNpgsql(databaseOptions.Value.ConnectionString);
        });

        return app;
    }

    /// <summary>
    /// Adds the PostgreSQL database.
    /// </summary>
    /// <param name="app">The NaGet application.</param>
    /// <param name="configure">The configuration options builder.</param>
    /// <returns>The <see cref="NaGetApplication"/>.</returns>
    public static NaGetApplication AddPostgreSqlDatabase(this NaGetApplication app, Action<DatabaseOptions> configure)
    {
        app.AddPostgreSqlDatabase();
        app.Services.Configure(configure);
        return app;
    }
}
