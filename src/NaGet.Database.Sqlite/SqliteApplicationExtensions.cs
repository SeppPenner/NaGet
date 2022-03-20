// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqliteApplicationExtensions.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The SQLite application extensions class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet;

/// <summary>
/// The SQLite application extensions class.
/// </summary>
public static class SqliteApplicationExtensions
{
    /// <summary>
    /// Adds the SQLite database.
    /// </summary>
    /// <param name="app">The NaGet application.</param>
    /// <returns>The <see cref="NaGetApplication"/>.</returns>
    public static NaGetApplication AddSqliteDatabase(this NaGetApplication app)
    {
        app.Services.AddNaGetDbContextProvider<SqliteContext>("Sqlite", (provider, options) =>
        {
            var databaseOptions = provider.GetRequiredService<IOptionsSnapshot<DatabaseOptions>>();
            options.UseSqlite(databaseOptions.Value.ConnectionString);
        });

        return app;
    }

    /// <summary>
    /// Adds the SQLite database.
    /// </summary>
    /// <param name="app">The NaGet application.</param>
    /// <param name="configure">The configuration options builder.</param>
    /// <returns>The <see cref="NaGetApplication"/>.</returns>
    public static NaGetApplication AddSqliteDatabase(this NaGetApplication app, Action<DatabaseOptions> configure)
    {
        app.AddSqliteDatabase();
        app.Services.Configure(configure);
        return app;
    }
}
