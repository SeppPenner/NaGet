namespace NaGet;

public static class SqlServerApplicationExtensions
{
    public static NaGetApplication AddSqlServerDatabase(this NaGetApplication app)
    {
        app.Services.AddNaGetDbContextProvider<SqlServerContext>("SqlServer", (provider, options) =>
        {
            var databaseOptions = provider.GetRequiredService<IOptionsSnapshot<DatabaseOptions>>();

            options.UseSqlServer(databaseOptions.Value.ConnectionString);
        });

        return app;
    }

    public static NaGetApplication AddSqlServerDatabase(
        this NaGetApplication app,
        Action<DatabaseOptions> configure)
    {
        app.AddSqlServerDatabase();
        app.Services.Configure(configure);
        return app;
    }
}
