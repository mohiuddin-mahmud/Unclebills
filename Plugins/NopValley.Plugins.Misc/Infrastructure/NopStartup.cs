using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;

using Nop.Web.Controllers;
using Nop.Web.Framework.Mvc.Routing;
using NopValley.Plugins.Misc.Controllers;
using NopValley.Plugins.Misc.Factories;


namespace NopValley.Plugins.Misc.Infrastructure
{
    public class NopStartup : INopStartup
    {

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {

            //services.Configure<Microsoft.AspNetCore.Mvc.Razor.RazorViewEngineOptions>(options =>
            //{
            //    options.ViewLocationExpanders.Add(new ProductViewLocationExpander());
            //});
            //services.AddScoped<ProductController, CustomProductController>();
          //  services.AddScoped<ICustomProductModelFactory, CustomProductModelFactory>();

            // Replace the default SlugRouteTransformer with our custom one
           // services.AddSingleton<SlugRouteTransformer, CustomProductSlugRouteTransformer>();

            //services.AddSingleton<IJsonLdModelFactory, JsonLdModelFactory>();
      
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
