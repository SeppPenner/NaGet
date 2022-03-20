// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GoogleCloudApplicationExtensions.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The Google cloud application extensions class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet;

/// <summary>
/// The Google cloud application extensions class.
/// </summary>
public static class GoogleCloudApplicationExtensions
{
    /// <summary>
    /// Adds the Google cloud storage.
    /// </summary>
    /// <param name="app">The NaGet application.</param>
    /// <returns>The <see cref="NaGetApplication"/>.</returns>
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

    /// <summary>
    /// Adds the Google cloud storage.
    /// </summary>
    /// <param name="app">The NaGet application.</param>
    /// <param name="configure">The configuration options builder.</param>
    /// <returns>The <see cref="NaGetApplication"/>.</returns>
    public static NaGetApplication AddGoogleCloudStorage(this NaGetApplication app, Action<GoogleCloudStorageOptions> configure)
    {
        app.AddGoogleCloudStorage();
        app.Services.Configure(configure);
        return app;
    }
}
