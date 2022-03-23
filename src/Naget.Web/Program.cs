namespace NaGet.Web;

/// <summary>
/// The program class.
/// </summary>
public static class Program
{
    /// <summary>
    /// Checks whether the application is run in Docker environment or not. The variable is set in all Microsoft runtime images.
    /// </summary>
    public static bool InDocker => Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER")?.ToLowerInvariant() == "true";

    /// <summary>
    /// Checks whether the application is run in Docker compose environment or not.
    /// </summary>
    public static bool InCompose => Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_COMPOSE")?.ToLowerInvariant() == "true";

    /// <summary>
    /// Gets the environment name.
    /// </summary>
    public static string EnvironmentName => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? string.Empty;

    /// <summary>
    /// Gets or sets the NaGet service configuration.
    /// </summary>
    public static NaGetOptions Configuration { get; set; } = new();

    /// <summary>
    /// The service name.
    /// </summary>
    public static AssemblyName ServiceName => Assembly.GetExecutingAssembly().GetName();

    /// <summary>
    /// The main method.
    /// </summary>
    /// <param name="args">Some arguments.</param>
    /// <returns>The result code.</returns>
    public static async Task<int> Main(string[] args)
    {
        ReadConfiguration();
        SetupLogging();

        try
        {
            Log.Information("Starting {ServiceName}, Version {Version}...", ServiceName.Name, ServiceName.Version);
            // Todo: Get from config
            //Log.Information("Running on {Ports}...", Configuration.WebServerSettings?.HttpEndPoint);
            var currentLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
            await CreateHostBuilder(args, currentLocation).Build().RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly.");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }

        return 0;
    }

    /// <summary>
    /// Creates the host builder.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <param name="currentLocation">The current assembly location.</param>
    /// <returns>A new <see cref="IHostBuilder"/>.</returns>
    private static IHostBuilder CreateHostBuilder(string[] args, string currentLocation)
    {
        return Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(
            webBuilder =>
            {
                webBuilder.UseContentRoot(currentLocation);
                webBuilder.UseSetting(WebHostDefaults.DetailedErrorsKey, "true");
                webBuilder.ConfigureKestrel(
                options =>
                {
                    // Todo: Get from config
                    //options.Listen(Configuration.WebServerSettings!.HttpEndPoint);
                    options.Listen(IPAddress.Any, 5000);
                });
                webBuilder.UseStartup<Startup>();
            })
            .UseSerilog()
            .UseWindowsService()
            .UseSystemd();
    }

    /// <summary>
    /// Reads the configuration.
    /// </summary>
    private static void ReadConfiguration()
    {
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddJsonFile("appsettings.json", false, true);

        if (EnvironmentName != null)
        {
            var appsettingsFileName = $"appsettings.{EnvironmentName}.json";

            if (File.Exists(appsettingsFileName))
            {
                configurationBuilder.AddJsonFile(appsettingsFileName, false, true);
            }
        }

        var configuration = configurationBuilder.Build();
        configuration.Bind(ServiceName.Name, Configuration);

        // Todo: Validate config.
        //if (!Configuration.IsValid())
        //{
        //    throw new InvalidOperationException("The configuration is invalid!");
        //}
    }

    /// <summary>
    /// Setup the logging.
    /// </summary>
    private static void SetupLogging()
    {
        const string customTemplate = "{Timestamp:dd.MM.yy HH:mm:ss.fff}\t[{Level:u3}]\t{Message}{NewLine}{Exception}";

        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Orleans", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithMachineName()
            .WriteTo.Console(outputTemplate: customTemplate);

        Log.Logger = loggerConfiguration.CreateLogger();
    }
}
