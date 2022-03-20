// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlServerApplicationExtensions.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The SQL server application extensions class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet;

/// <summary>
/// The SQL server application extensions class.
/// </summary>
public static class SqlServerApplicationExtensions
{
    /// <summary>
    /// Adds the SQL server database.
    /// </summary>
    /// <param name="app">The NaGet application.</param>
    /// <returns>The <see cref="NaGetApplication"/>.</returns>
    public static NaGetApplication AddSqlServerDatabase(this NaGetApplication app)
    {
        app.Services.AddNaGetDbContextProvider<SqlServerContext>("SqlServer", (provider, options) =>
        {
            var databaseOptions = provider.GetRequiredService<IOptionsSnapshot<DatabaseOptions>>();
            options.UseSqlServer(databaseOptions.Value.ConnectionString);
        });

        return app;
    }

    /// <summary>
    /// Adds the SQL server database.
    /// </summary>
    /// <param name="app">The NaGet application.</param>
    /// <param name="configure">The configuration options builder.</param>
    /// <returns>The <see cref="NaGetApplication"/>.</returns>
    public static NaGetApplication AddSqlServerDatabase(this NaGetApplication app, Action<DatabaseOptions> configure)
    {
        app.AddSqlServerDatabase();
        app.Services.Configure(configure);
        return app;
    }
}
