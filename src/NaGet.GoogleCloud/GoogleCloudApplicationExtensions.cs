namespace NaGet;

public static class GoogleCloudApplicationExtensions
{
    public static NaGetApplication AddGoogleCloudStorage(this NaGetApplication app)
    {
        app.Services.AddNaGetOptions<GoogleCloudStorageOptions>(nameof(NaGetOptions.Storage));
        app.Services.AddTransient<GoogleCloudStorageService>();

        app.Services.TryAddTransient<IStorageService>(provider => provider.GetRequiredService<GoogleCloudStorageService>());

        app.Services.AddProvider<IStorageService>((provider, config) =>
        {
            if (!config.HasStorageType("GoogleCloud"))
            {
                return null!;
            }

            return provider.GetRequiredService<GoogleCloudStorageService>();
        });

        return app;
    }

    public static NaGetApplication AddGoogleCloudStorage(
        this NaGetApplication app,
        Action<GoogleCloudStorageOptions> configure)
    {
        app.AddGoogleCloudStorage();
        app.Services.Configure(configure);
        return app;
    }
}
