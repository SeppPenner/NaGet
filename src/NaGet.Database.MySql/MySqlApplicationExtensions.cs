// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MySqlApplicationExtensions.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The MySQL application extensions class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet;

/// <summary>
/// The MySQL application extensions class.
/// </summary>
public static class MySqlApplicationExtensions
{
    /// <summary>
    /// Adds the MySQL database.
    /// </summary>
    /// <param name="app">The NaGet application.</param>
    /// <returns>The <see cref="NaGetApplication"/>.</returns>
    public static NaGetApplication AddMySqlDatabase(this NaGetApplication app)
    {
        app.Services.AddNaGetDbContextProvider<MySqlContext>("MySql", (provider, options) =>
        {
            var databaseOptions = provider.GetRequiredService<IOptionsSnapshot<DatabaseOptions>>();
            options.UseMySql(databaseOptions.Value.ConnectionString);
        });

        return app;
    }

    /// <summary>
    /// Adds the MySQL database.
    /// </summary>
    /// <param name="app">The NaGet application.</param>
    /// <param name="configure">The configuration options builder.</param>
    /// <returns>The <see cref="NaGetApplication"/>.</returns>
    public static NaGetApplication AddMySqlDatabase(this NaGetApplication app, Action<DatabaseOptions> configure)
    {
        app.AddMySqlDatabase();
        app.Services.Configure(configure);
        return app;
    }
}
