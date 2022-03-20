// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AliyunApplicationExtensions.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The Aliyun application extensions class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet;

/// <summary>
/// The Aliyun application extensions class.
/// </summary>
public static class AliyunApplicationExtensions
{
    /// <summary>
    /// Adds the Aliyun OSS storage.
    /// </summary>
    /// <param name="app">The NaGet application.</param>
    /// <returns>The <see cref="NaGetApplication"/>.</returns>
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
                return null!;
            }

            return provider.GetRequiredService<AliyunStorageService>();
        });

        return app;
    }

    /// <summary>
    /// Adds the Aliyun OSS storage.
    /// </summary>
    /// <param name="app">The NaGet application.</param>
    /// <param name="configure">The configuration options builder.</param>
    /// <returns>The <see cref="NaGetApplication"/>.</returns>
    public static NaGetApplication AddAliyunOssStorage(this NaGetApplication app, Action<AliyunStorageOptions> configure)
    {
        app.AddAliyunOssStorage();
        app.Services.Configure(configure);
        return app;
    }
}
