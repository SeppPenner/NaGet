using System;
using Microsoft.Extensions.DependencyInjection;

namespace NaGet.Core
{
    public class NaGetApplication
    {
        public NaGetApplication(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public IServiceCollection Services { get; }
    }
}
