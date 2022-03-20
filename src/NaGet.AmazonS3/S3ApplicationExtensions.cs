// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AwsApplicationExtensions.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The Amazon S3 application extensions class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet;

/// <summary>
/// The Amazon S3 application extensions class.
/// </summary>
public static class S3ApplicationExtensions
{
    /// <summary>
    /// Adds the Amazon S3 storage.
    /// </summary>
    /// <param name="app">The NaGet application.</param>
    /// <returns>The <see cref="NaGetApplication"/>.</returns>
    public static NaGetApplication AddAmazonS3Storage(this NaGetApplication app)
    {
        app.Services.AddNaGetOptions<S3StorageOptions>(nameof(NaGetOptions.Storage));

        app.Services.AddTransient<S3StorageService>();
        app.Services.TryAddTransient<IStorageService>(provider => provider.GetRequiredService<S3StorageService>());

        app.Services.AddProvider<IStorageService>((provider, config) =>
        {
            if (!config.HasStorageType("AwsS3"))
            {
                return null!;
            }

            return provider.GetRequiredService<S3StorageService>();
        });

        app.Services.AddSingleton(provider =>
        {
            var options = provider.GetRequiredService<IOptions<S3StorageOptions>>().Value;

            var config = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(options.Region)
            };

            if (options.UseInstanceProfile)
            {
                var credentials = FallbackCredentialsFactory.GetCredentials();
                return new AmazonS3Client(credentials, config);
            }

            if (!string.IsNullOrWhiteSpace(options.AssumeRoleArn))
            {
                var credentials = FallbackCredentialsFactory.GetCredentials();
                var assumedCredentials = AssumeRole(
                        credentials,
                        options.AssumeRoleArn,
                        $"NaGet-Session-{Guid.NewGuid()}")
                    .GetAwaiter()
                    .GetResult();

                return new AmazonS3Client(assumedCredentials, config);
            }

            if (!string.IsNullOrWhiteSpace(options.AccessKey))
            {
                return new AmazonS3Client(
                    new BasicAWSCredentials(
                        options.AccessKey,
                        options.SecretKey),
                    config);
            }

            return new AmazonS3Client(config);
        });

        return app;
    }

    /// <summary>
    /// Adds the Amazon S3 storage.
    /// </summary>
    /// <param name="app">The NaGet application.</param>
    /// <param name="configure">The configuration options builder.</param>
    /// <returns>The <see cref="NaGetApplication"/>.</returns>
    public static NaGetApplication AddAmazonS3Storage(this NaGetApplication app, Action<S3StorageOptions> configure)
    {
        app.AddAmazonS3Storage();
        app.Services.Configure(configure);
        return app;
    }

    /// <summary>
    /// Tries to get an Amazon S3 role.
    /// </summary>
    /// <param name="credentials">The credentials.</param>
    /// <param name="roleArn">The role ARN.</param>
    /// <param name="roleSessionName">The role session name.</param>
    /// <returns>The <see cref="AWSCredentials"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no role is found.</exception>
    private static async Task<AWSCredentials> AssumeRole(AWSCredentials credentials, string roleArn, string roleSessionName)
    {
        var assumedCredentials = new AssumeRoleAWSCredentials(credentials, roleArn, roleSessionName);
        var immutableCredentials = await credentials.GetCredentialsAsync();

        if (string.IsNullOrWhiteSpace(immutableCredentials.Token))
        {
            throw new InvalidOperationException($"Unable to assume role {roleArn}");
        }

        return assumedCredentials;
    }
}
