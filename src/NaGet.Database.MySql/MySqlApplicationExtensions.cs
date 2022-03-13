namespace NaGet;

public static class MySqlApplicationExtensions
{
    public static NaGetApplication AddMySqlDatabase(this NaGetApplication app)
    {
        app.Services.AddNaGetDbContextProvider<MySqlContext>("MySql", (provider, options) =>
        {
            var databaseOptions = provider.GetRequiredService<IOptionsSnapshot<DatabaseOptions>>();

            options.UseMySql(databaseOptions.Value.ConnectionString);
        });

        return app;
    }

    public static NaGetApplication AddMySqlDatabase(
        this NaGetApplication app,
        Action<DatabaseOptions> configure)
    {
        app.AddMySqlDatabase();
        app.Services.Configure(configure);
        return app;
    }
}
