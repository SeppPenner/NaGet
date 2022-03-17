using NaGet.Core;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace NaGet.Web
{
    public static class IHostExtensions
    {
        public static IHostBuilder UseNaGet(this IHostBuilder host, Action<NaGetApplication> configure)
        {
            return host.ConfigureServices(services =>
            {
                services.AddNaGetWebApplication(configure);
            });
        }

        public static async Task RunMigrationsAsync(
            this IHost host,
            CancellationToken cancellationToken = default)
        {
            // Run migrations if necessary.
            var options = host.Services.GetRequiredService<IOptions<NaGetOptions>>();

            if (options.Value.RunMigrationsAtStartup)
            {
                using var scope = host.Services.CreateScope();
                var ctx = scope.ServiceProvider.GetService<IContext>();
                if (ctx is not null)
                {
                    await ctx.RunMigrationsAsync(cancellationToken);
                }
            }
        }

        public static bool ValidateStartupOptions(this IHost host)
        {
            return host
                .Services
                .GetRequiredService<ValidateStartupOptions>()
                .Validate();
        }
    }
}
