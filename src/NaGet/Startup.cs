// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Startup.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   The startup class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet;

/// <summary>
/// The startup class.
/// </summary>
public class Startup
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Startup"/> class.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <exception cref="ArgumentNullException">Thrown if the configuration is null.</exception>
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Gets the configuration.
    /// </summary>
    private IConfiguration Configuration { get; }

    /// <summary>
    /// Configures the services.
    /// </summary>
    /// <param name="services">The services.</param>
    public void ConfigureServices(IServiceCollection services)
    {
        // TODO: Ideally we'd use:
        //
        //       services.ConfigureOptions<ConfigureNaGetOptions>();
        //
        //       However, "ConfigureOptions" doesn't register validations as expected.
        //       We'll instead register all these configurations manually.
        // See: https://github.com/dotnet/runtime/issues/38491
        services.AddTransient<IConfigureOptions<CorsOptions>, ConfigureNaGetOptions>();
        services.AddTransient<IConfigureOptions<FormOptions>, ConfigureNaGetOptions>();
        services.AddTransient<IConfigureOptions<ForwardedHeadersOptions>, ConfigureNaGetOptions>();
        services.AddTransient<IConfigureOptions<IISServerOptions>, ConfigureNaGetOptions>();
        services.AddTransient<IValidateOptions<NaGetOptions>, ConfigureNaGetOptions>();

        services.AddNaGetOptions<IISServerOptions>(nameof(IISServerOptions));
        services.AddNaGetWebApplication(ConfigureNaGetApplication);

        // You can swap between implementations of subsystems like storage and search using NaGet's configuration.
        // Each subsystem's implementation has a provider that reads the configuration to determine if it should be
        // activated. NaGet will run through all its providers until it finds one that is active.
        services.AddScoped(DependencyInjectionExtensions.GetServiceFromProviders<IContext>);
        services.AddTransient(DependencyInjectionExtensions.GetServiceFromProviders<IStorageService>);
        services.AddTransient(DependencyInjectionExtensions.GetServiceFromProviders<IPackageDatabase>);
        services.AddTransient(DependencyInjectionExtensions.GetServiceFromProviders<ISearchService>);
        services.AddTransient(DependencyInjectionExtensions.GetServiceFromProviders<ISearchIndexer>);

        services.AddSingleton<IConfigureOptions<MvcRazorRuntimeCompilationOptions>, ConfigureRazorRuntimeCompilation>();

        services.AddCors();
    }

    /// <summary>
    /// Configures the application.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="env">The web host environment.</param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        var options = Configuration.Get<NaGetOptions>();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseStatusCodePages();
        }

        app.UseForwardedHeaders();
        app.UsePathBase(options.PathBase);

        app.UseStaticFiles();
        app.UseRouting();

        app.UseCors(ConfigureNaGetOptions.CorsPolicy);
        app.UseOperationCancelledMiddleware();

        app.UseEndpoints(endpoints =>
        {
            var naget = new NaGetEndpointBuilder();
            naget.MapEndpoints(endpoints);
        });
    }

    /// <summary>
    /// Configures the NaGet applictaion.
    /// </summary>
    /// <param name="app">The application.</param>
    private void ConfigureNaGetApplication(NaGetApplication app)
    {
        // Add database providers.
        app.AddAzureTableDatabase();
        app.AddMySqlDatabase();
        app.AddPostgreSqlDatabase();
        app.AddSqliteDatabase();
        app.AddSqlServerDatabase();

        // Add storage providers.
        app.AddFileStorage();
        app.AddAliyunOssStorage();
        app.AddAmazonS3Storage();
        app.AddAzureBlobStorage();
        app.AddGoogleCloudStorage();

        // Add search providers.
        app.AddAzureSearch();
    }
}
