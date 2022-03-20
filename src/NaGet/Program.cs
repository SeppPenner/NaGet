// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   The main program class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet;

/// <summary>
/// The main program class.
/// </summary>
public class Program
{
    /// <summary>
    /// The main method.
    /// </summary>
    /// <param name="args">Some arguments.</param>
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        if (!host.ValidateStartupOptions())
        {
            return;
        }

        var app = new CommandLineApplication
        {
            Name = "naget",
            Description = "A light-weight NuGet service",
        };

        app.HelpOption(inherited: true);

        app.Command("import", import =>
        {
            import.Command("downloads", downloads =>
            {
                downloads.OnExecuteAsync(async cancellationToken =>
                {
                    using var scope = host.Services.CreateScope();
                    var importer = scope.ServiceProvider.GetRequiredService<DownloadsImporter>();
                    await importer.ImportAsync(cancellationToken);
                });
            });
        });

        app.Option("--urls", "The URLs that NaGet should bind to.", CommandOptionType.SingleValue);

        app.OnExecuteAsync(async cancellationToken =>
        {
            await host.RunMigrationsAsync(cancellationToken);
            await host.RunAsync(cancellationToken);
        });

        await app.ExecuteAsync(args);
    }

    /// <summary>
    /// Creates the host builder.
    /// </summary>
    /// <param name="args">Some arguments.</param>
    /// <returns>The <see cref="IHostBuilder"/>.</returns>
    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host
            .CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((ctx, config) =>
            {
                var root = Environment.GetEnvironmentVariable("NAGET_CONFIG_ROOT");

                if (!string.IsNullOrWhiteSpace(root))
                {
                    config.SetBasePath(root);
                }
            })
            .ConfigureWebHostDefaults(web =>
            {
                web.ConfigureKestrel(options =>
                {
                    // Remove the upload limit from Kestrel. If needed, an upload limit can
                    // be enforced by a reverse proxy server, like IIS.
                    options.Limits.MaxRequestBodySize = null;
                });

                web.UseStartup<Startup>();
            });
    }
}
