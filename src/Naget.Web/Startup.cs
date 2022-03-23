namespace NaGet.Web;

/// <summary>
/// The startup class.
/// </summary>
public class Startup
{
    /// <summary>
    /// The service name.
    /// </summary>
    private readonly AssemblyName serviceName = Assembly.GetExecutingAssembly().GetName();

    /// <summary>
    /// Gets the NaGet configuration.
    /// </summary>
    private readonly NaGetOptions configuration = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Startup"/> class.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    public Startup(IConfiguration configuration)
    {
        // Bind the service configuration.
        configuration.GetSection(this.serviceName.Name).Bind(this.configuration);
    }

    /// <summary>
    /// Configures the services.
    /// </summary>
    /// <param name="services">The services.</param>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddOptions();

        services.AddSingleton(this.configuration);
        services.AddSingleton(Log.Logger);

        services.AddMvc()
            .AddJsonOptions(o =>
            {
                o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            }).AddRazorPagesOptions(options => options.RootDirectory = "/")
            .AddDataAnnotationsLocalization();

        services.AddConnections();
        services.AddRazorPages(o => o.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute()));

        services.AddServerSideBlazor();

        services.AddResponseCompression(opts =>
        {
            // Todo: Add new octet mime types
            //opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
            //    new[]
            //    {
            //        MimeTypes.Application.Octet
            //    });
        });

        services.AddEndpointsApiExplorer();

        // Add Swagger document for the API.
        services.AddOpenApiDocument(
            config =>
            {
                config.DocumentName = $"{this.serviceName.Name} {this.serviceName.Version}";
                config.PostProcess = document =>
                {
                    document.Info.Version = $"{this.serviceName.Version}";
                    document.Info.Title = $"{this.serviceName.Name}";
                    document.Info.Description = $"{this.serviceName.Name}";
                    document.Info.TermsOfService = "https://github.com/SeppPenner/NaGet/blob/master/License.txt";
                    document.Info.Contact = new OpenApiContact
                    {
                        Name = "Hämmer Electronics",
                        Email = string.Empty,
                        Url = "https://github.com/SeppPenner/NaGet"
                    };
                    document.Info.License = new OpenApiLicense
                    {
                        Name = "Use prohibited unless explicitly allowed.",
                        Url = string.Empty
                    };
                };

                // Add authorization support for Swagger.
                config.OperationProcessors.Add(new OperationSecurityScopeProcessor("auth"));
                config.DocumentProcessors.Add(new SecurityDefinitionAppender("auth", new OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.Http,
                    In = OpenApiSecurityApiKeyLocation.Header,
                    Scheme = "bearer",
                    BearerFormat = "jwt"
                }));
            });

        services.AddAuthorizationCore();
        services.AddCors(
            options => options.AddPolicy(
                "Open",
                builder => builder
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()));

        // Add the background service.
        services.AddSingleton<WebServerService>();
        services.AddSingleton<IHostedService>(p =>
        {
            var heartbeatService = p.GetRequiredService<WebServerService>();
            heartbeatService.DelayInMilliSeconds = 5000;
            return heartbeatService;
        });

        // Add localization.
        services.AddLocalization(options => options.ResourcesPath = "Resources");

        // Todo: Get port from settings
        //if (services.All(x => x.ServiceType != typeof(HttpClient)))
        //{
        //    services.AddScoped(s =>
        //    {
        //        var httpUrl = this.configuration.WebServerSettings!.HttpUrl!.ToLocalHostUrl();

        //        return new HttpClient
        //        {
        //            BaseAddress = new Uri(httpUrl)
        //        };
        //    });
        //}
    }

    private RequestLocalizationOptions GetLocalizationOptions()
    {
        var localizationOptions =
            new RequestLocalizationOptions();
            //.AddSupportedCultures(supportedCultures)
            //.AddSupportedUICultures(supportedCultures);

        // Cookie culture provider is included by default, but we want it to be the only one.
        localizationOptions.RequestCultureProviders.Clear();
        localizationOptions.RequestCultureProviders.Add(new CookieRequestCultureProvider());

        // Todo: Make configurable.
        // Set the default locale to the English one.
        localizationOptions.SetDefaultCulture("en-US");

        return localizationOptions;
    }

    /// <summary>
    /// This method gets called by the runtime.
    /// </summary>
    /// <param name="app">The application.</param>
    /// <param name="env">The web hosting environment.</param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseResponseCompression();

        app.UseStaticFiles();

        app.UseSerilogRequestLogging();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/error");
        }

        // Add localization.
        app.UseRequestLocalization(this.GetLocalizationOptions());

        app.UseRouting();
        app.UseCors("Open");
        app.UseAuthentication();
        app.UseAuthorization();

        // Add swagger stuff.
        app.UseOpenApi();
        app.UseSwaggerUi3();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();

            endpoints.MapDefaultControllerRoute();
            endpoints.MapBlazorHub();

            endpoints.MapFallbackToPage("/_Host");
        });
    }
}
