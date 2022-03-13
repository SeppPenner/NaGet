namespace NaGet;

public static class AliyunApplicationExtensions
{
    public static NaGetApplication AddAliyunOssStorage(this NaGetApplication app)
    {
        app.Services.AddNaGetOptions<AliyunStorageOptions>(nameof(NaGetOptions.Storage));

        app.Services.AddTransient<AliyunStorageService>();
        app.Services.TryAddTransient<IStorageService>(provider => provider.GetRequiredService<AliyunStorageService>());

        app.Services.AddSingleton(provider =>
        {
            var options = provider.GetRequiredService<IOptions<AliyunStorageOptions>>().Value;

            return new OssClient(options.Endpoint, options.AccessKey, options.AccessKeySecret);
        });

        app.Services.AddProvider<IStorageService>((provider, config) =>
        {
            if (!config.HasStorageType("AliyunOss"))
            {
                return null;
            }

            return provider.GetRequiredService<AliyunStorageService>();
        });

        return app;
    }

    public static NaGetApplication AddAliyunOssStorage(
        this NaGetApplication app,
        Action<AliyunStorageOptions> configure)
    {
        app.AddAliyunOssStorage();
        app.Services.Configure(configure);
        return app;
    }
}
