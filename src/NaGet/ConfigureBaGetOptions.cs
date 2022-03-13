using System;
using System.Collections.Generic;
using System.Linq;
using NaGet.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;

namespace NaGet
{
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
        public const string CorsPolicy = "AllowAll";

        private static readonly HashSet<string> ValidDatabaseTypes
            = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "AzureTable",
                "MySql",
                "PostgreSql",
                "Sqlite",
                "SqlServer",
            };

        private static readonly HashSet<string> ValidStorageTypes
            = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "AliyunOss",
                "AwsS3",
                "AzureBlobStorage",
                "Filesystem",
                "GoogleCloud",
                "Null",
            };

        private static readonly HashSet<string> ValidSearchTypes
            = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "AzureSearch",
                "Database",
                "Null",
            };

        public void Configure(CorsOptions options)
        {
            // TODO: Consider disabling this on production builds.
            options.AddPolicy(
                CorsPolicy,
                builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
        }

        public void Configure(FormOptions options)
        {
            options.MultipartBodyLengthLimit = int.MaxValue;
        }

        public void Configure(ForwardedHeadersOptions options)
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

            // Do not restrict to local network/proxy
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        }

        public void Configure(IISServerOptions options)
        {
            options.MaxRequestBodySize = 262144000;
        }

        public ValidateOptionsResult Validate(string name, NaGetOptions options)
        {
            var failures = new List<string>();

            if (options.Database == null) failures.Add($"The '{nameof(NaGetOptions.Database)}' config is required");
            if (options.Mirror == null) failures.Add($"The '{nameof(NaGetOptions.Mirror)}' config is required");
            if (options.Search == null) failures.Add($"The '{nameof(NaGetOptions.Search)}' config is required");
            if (options.Storage == null) failures.Add($"The '{nameof(NaGetOptions.Storage)}' config is required");

            if (!ValidDatabaseTypes.Contains(options.Database?.Type))
            {
                failures.Add(
                    $"The '{nameof(NaGetOptions.Database)}:{nameof(DatabaseOptions.Type)}' config is invalid. " +
                    $"Allowed values: {string.Join(", ", ValidDatabaseTypes)}");
            }

            if (!ValidStorageTypes.Contains(options.Storage?.Type))
            {
                failures.Add(
                    $"The '{nameof(NaGetOptions.Storage)}:{nameof(StorageOptions.Type)}' config is invalid. " +
                    $"Allowed values: {string.Join(", ", ValidStorageTypes)}");
            }

            if (!ValidSearchTypes.Contains(options.Search?.Type))
            {
                failures.Add(
                    $"The '{nameof(NaGetOptions.Search)}:{nameof(SearchOptions.Type)}' config is invalid. " +
                    $"Allowed values: {string.Join(", ", ValidSearchTypes)}");
            }

            if (failures.Any()) return ValidateOptionsResult.Fail(failures);

            return ValidateOptionsResult.Success;
        }
    }
}
