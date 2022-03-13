using System;
using NaGet.Core;
using NaGet.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

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
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
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
