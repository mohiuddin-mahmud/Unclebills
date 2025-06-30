using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Core;
using Nop.Core.Domain.Seo;
using Nop.Core.Events;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Custom.Product.Routing
{
    public class CustomProductSlugRouteTransformer : SlugRouteTransformer
    {
        private readonly IRepository<UrlRecord> _urlRecordRepository;

        public CustomProductSlugRouteTransformer(
            CatalogSettings catalogSettings,
        ICategoryService categoryService,
        IEventPublisher eventPublisher,
        ILanguageService languageService,
        IManufacturerService manufacturerService,
        IStoreContext storeContext,
        IUrlRecordService urlRecordService,
        LocalizationSettings localizationSettings)
            : base(catalogSettings,
         categoryService,         eventPublisher,
         languageService,
         manufacturerService,
         storeContext,
         urlRecordService,
         localizationSettings)
        {
           
        }

        public override ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
        {
            // First let the base transformer do its work
            var result = base.TransformAsync(httpContext, values).Result;

            // Check if this is a product page
            if (result != null &&
                result.TryGetValue("controller", out var controller) &&
                controller?.ToString() == "Product" &&
                result.TryGetValue("action", out var action) &&
                action?.ToString() == "ProductDetails")
            {
                // Change the controller to your custom controller
                result["controller"] = "CustomProduct";
                result["action"] = "ProductDetails";
            }

            return new ValueTask<RouteValueDictionary>(result);
        }
    }
}