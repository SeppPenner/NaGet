namespace NaGet;

public static class NaGetApplicationExtensions
{
    public static NaGetApplication AddFileStorage(this NaGetApplication app)
    {
        app.Services.TryAddTransient<IStorageService>(provider => provider.GetRequiredService<FileStorageService>());
        return app;
    }

    public static NaGetApplication AddFileStorage(
        this NaGetApplication app,
        Action<FileSystemStorageOptions> configure)
    {
        app.AddFileStorage();
        app.Services.Configure(configure);
        return app;
    }

    public static NaGetApplication AddNullStorage(this NaGetApplication app)
    {
        app.Services.TryAddTransient<IStorageService>(provider => provider.GetRequiredService<NullStorageService>());
        return app;
    }

    public static NaGetApplication AddNullSearch(this NaGetApplication app)
    {
        app.Services.TryAddTransient<ISearchIndexer>(provider => provider.GetRequiredService<NullSearchIndexer>());
        app.Services.TryAddTransient<ISearchService>(provider => provider.GetRequiredService<NullSearchService>());
        return app;
    }
}
