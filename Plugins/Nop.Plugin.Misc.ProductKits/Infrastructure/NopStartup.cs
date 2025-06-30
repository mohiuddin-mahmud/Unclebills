using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopValley.Plugins.Misc.Services;
using NopValley.Plugins.Misc.Areas.Admin.Services;
using NopValley.Plugins.Misc.Areas.Admin.Factories;
using NopValley.Plugins.Misc.Factories;

namespace NopValley.Plugins.Misc.Infrastructure
{
    public class NopStartup : INopStartup
    {

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {

            services.AddScoped<IBlogAdditionalService, BlogAdditionalService>();
            services.AddScoped<IServicePostService, ServicePostService>();
            services.AddScoped<IServicePostAdminModelFactory, ServicePostAdminModelFactory>();
            services.AddScoped<IServicePostModelFactory, ServicePostModelFactory>();
            //services.AddScoped<SlugRouteTransformer>();

            //services.AddScoped<SlugRouteTransformer>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        /// <summary>
        /// Gets order of this startup configuration implementation
        /// </summary>
        public int Order => int.MaxValue;
    }
}
