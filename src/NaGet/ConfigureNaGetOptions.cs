// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigureNaGetOptions.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   NaGet's options configuration, specific to the default NaGet application.
//   Don't use this if you are embedding NaGet into your own custom ASP.NET Core application.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet;

/// <summary>
/// NaGet's options configuration, specific to the default NaGet application.
/// Don't use this if you are embedding NaGet into your own custom ASP.NET Core application.
/// </summary>
public class ConfigureNaGetOptions
    : IConfigureOptions<CorsOptions>
    , IConfigureOptions<FormOptions>
    , IConfigureOptions<ForwardedHeadersOptions>
    , IConfigureOptions<IISServerOptions>
    , IValidateOptions<NaGetOptions>
{
    /// <summary>
    /// The CORS policy to allow all.
    /// </summary>
    public const string CorsPolicy = "AllowAll";

    /// <summary>
    /// A set of valid database types.
    /// </summary>
    private static readonly HashSet<string> ValidDatabaseTypes
        = new(StringComparer.OrdinalIgnoreCase)
        {
            "AzureTable",
            "MySql",
            "PostgreSql",
            "Sqlite",
            "SqlServer",
        };

    /// <summary>
    /// A set of valid storage types.
    /// </summary>
    private static readonly HashSet<string> ValidStorageTypes
        = new(StringComparer.OrdinalIgnoreCase)
        {
            "AliyunOss",
            "AwsS3",
            "AzureBlobStorage",
            "Filesystem",
            "GoogleCloud",
            "Null",
        };

    /// <summary>
    /// A set of valid search types.
    /// </summary>
    private static readonly HashSet<string> ValidSearchTypes
        = new(StringComparer.OrdinalIgnoreCase)
        {
            "AzureSearch",
            "Database",
            "Null",
        };

    /// <inheritdoc cref="IConfigureOptions{TOptions}"/>
    public void Configure(CorsOptions options)
    {
        // TODO: Consider disabling this on production builds.
        options.AddPolicy(
            CorsPolicy,
            builder => builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
    }

    /// <inheritdoc cref="IConfigureOptions{TOptions}"/>
    public void Configure(FormOptions options)
    {
        options.MultipartBodyLengthLimit = int.MaxValue;
    }

    /// <inheritdoc cref="IConfigureOptions{TOptions}"/>
    public void Configure(ForwardedHeadersOptions options)
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

        // Do not restrict to local network/proxy.
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    }

    /// <inheritdoc cref="IConfigureOptions{TOptions}"/>
    public void Configure(IISServerOptions options)
    {
        options.MaxRequestBodySize = 262144000;
    }

    /// <inheritdoc cref="IValidateOptions{TOptions}"/>
    public ValidateOptionsResult Validate(string name, NaGetOptions options)
    {
        var failures = new List<string>();

        if (options.Database is null)
        {
            failures.Add($"The '{nameof(NaGetOptions.Database)}' config is required");
        }

        if (options.Mirror is null)
        {
            failures.Add($"The '{nameof(NaGetOptions.Mirror)}' config is required");
        }

        if (options.Search is null)
        {
            failures.Add($"The '{nameof(NaGetOptions.Search)}' config is required");
        }

        if (options.Storage is null)
        {
            failures.Add($"The '{nameof(NaGetOptions.Storage)}' config is required");
        }

        var databaseType = options.Database?.Type ?? string.Empty;
        var storageType = options.Storage?.Type ?? string.Empty;
        var searchType = options.Search?.Type ?? string.Empty;

        if (!ValidDatabaseTypes.Contains(databaseType))
        {
            failures.Add(
                $"The '{nameof(NaGetOptions.Database)}:{nameof(DatabaseOptions.Type)}' config is invalid. " +
                $"Allowed values: {string.Join(", ", ValidDatabaseTypes)}");
        }

        if (!ValidStorageTypes.Contains(storageType))
        {
            failures.Add(
                $"The '{nameof(NaGetOptions.Storage)}:{nameof(StorageOptions.Type)}' config is invalid. " +
                $"Allowed values: {string.Join(", ", ValidStorageTypes)}");
        }

        if (!ValidSearchTypes.Contains(searchType))
        {
            failures.Add(
                $"The '{nameof(NaGetOptions.Search)}:{nameof(SearchOptions.Type)}' config is invalid. " +
                $"Allowed values: {string.Join(", ", ValidSearchTypes)}");
        }

        if (failures.Any())
        {
            return ValidateOptionsResult.Fail(failures);
        }

        return ValidateOptionsResult.Success;
    }
}
