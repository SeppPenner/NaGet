namespace NaGet
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddNaGetWebApplication(
            this IServiceCollection services,
            Action<NaGetApplication> configureAction)
        {
            services
                .AddRouting(options => options.LowercaseUrls = true)
                .AddControllers()
                .AddApplicationPart(typeof(PackageContentController).Assembly)
                .SetCompatibilityVersion(CompatibilityVersion.Latest)
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.IgnoreNullValues = true;
                });

            services.AddRazorPages();

            services.AddHttpContextAccessor();
            services.AddTransient<IUrlGenerator, NaGetUrlGenerator>();

            services.AddNaGetApplication(configureAction);

            return services;
        }
    }
}
