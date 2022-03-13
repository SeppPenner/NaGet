namespace NaGet;

public static class SqliteApplicationExtensions
{
    public static NaGetApplication AddSqliteDatabase(this NaGetApplication app)
    {
        app.Services.AddNaGetDbContextProvider<SqliteContext>("Sqlite", (provider, options) =>
        {
            var databaseOptions = provider.GetRequiredService<IOptionsSnapshot<DatabaseOptions>>();

            options.UseSqlite(databaseOptions.Value.ConnectionString);
        });

        return app;
    }

    public static NaGetApplication AddSqliteDatabase(
        this NaGetApplication app,
        Action<DatabaseOptions> configure)
    {
        app.AddSqliteDatabase();
        app.Services.Configure(configure);
        return app;
    }
}
