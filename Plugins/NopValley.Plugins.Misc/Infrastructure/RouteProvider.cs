using Autofac.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopValley.Plugins.Misc.Infrastructure
{
    public class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            //get language pattern
            //it's not needed to use language pattern in AJAX requests and for actions returning the result directly (e.g. file to download),
            //use it only for URLs of pages that the user can go to
            var lang = GetLanguageRoutePattern();

            //endpointRouteBuilder.MapControllerRoute(
            //   name: "NopValley.Misc.ProductDetails",
            //   pattern: "product/{productId}",
            //   defaults: new { controller = "Product", action = "CustomProduct", area = "" });


            // Override the product details route
            //endpointRouteBuilder.MapControllerRoute("Product",
            //    "{SeName}",
            //    new { controller = "CustomProduct", action = "ProductDetails" });

            //register
            endpointRouteBuilder.MapControllerRoute(name: "PreRegister",
              pattern: $"{lang}/preregister/",
              defaults: new { controller = "Customer", action = "PreRegister" });


     
            //endpointRouteBuilder.MapControllerRoute(
            //    name: "NopValley.Plugins.Misc",
            //    pattern: "custom-category/{categoryId:int}",
            //    defaults: new { controller = "CustomCatalogController", action = "Category" });
        

            //var genericPattern = $"{lang}/{{{NopRoutingDefaults.RouteValue.SeName}}}";
            ////endpointRouteBuilder.MapDynamicControllerRoute<SlugRouteTransformer>(genericPattern);

            //endpointRouteBuilder.MapControllerRoute(name: NopRoutingDefaults.RouteName.Generic.ServicePost,
            //pattern: genericPattern,
            //defaults: new { controller = "ServicePost", action = "ServicePost" });

            //case study admin crud
            //endpointRouteBuilder.MapControllerRoute("NopValley.HtmlWidget.List", $"Admin/HtmlWidget/List",
            //    new { controller = "HtmlWidget", action = "List", area = AreaNames.Admin });
            //endpointRouteBuilder.MapControllerRoute("NopValley.HtmlWidget.Create", $"Admin/HtmlWidget/Create",
            //    new { controller = "HtmlWidget", action = "Create", area = AreaNames.Admin });
            //endpointRouteBuilder.MapControllerRoute("NopValley.HtmlWidget.Edit", $"Admin/HtmlWidget/Edit",
            //    new { controller = "HtmlWidget", action = "Edit", area = AreaNames.Admin });
            //endpointRouteBuilder.MapControllerRoute("NopValley.HtmlWidget.Delete", $"Admin/HtmlWidget/Delete",
            //    new { controller = "HtmlWidget", action = "Delete", area = AreaNames.Admin });

            //section
            //endpointRouteBuilder.MapControllerRoute("References.CaseStudy.Section.Edit", $"Admin/Section/Edit",
            //   new { controller = "Section", action = "CaseStudySectionAddOrEdit", area = AreaNames.Admin });

            ////testimonial admin crud
            //endpointRouteBuilder.MapControllerRoute("NopValley.Testimonial.List", $"Admin/Testimonial/List",
            //    new { controller = "Testimonial", action = "List", area = AreaNames.Admin });
            //endpointRouteBuilder.MapControllerRoute("NopValley.Testimonial.Create", $"Admin/Testimonial/Create",
            //    new { controller = "Testimonial", action = "Create", area = AreaNames.Admin });
            //endpointRouteBuilder.MapControllerRoute("NopValley.Testimonial.Edit", $"Admin/Testimonial/Edit",
            //    new { controller = "Testimonial", action = "Edit", area = AreaNames.Admin });
            //endpointRouteBuilder.MapControllerRoute("NopValley.Testimonial.Delete", $"Admin/Testimonial/Delete",
            //    new { controller = "Testimonial", action = "Delete", area = AreaNames.Admin });

            ////testimonial admin crud
            //endpointRouteBuilder.MapControllerRoute("NopValley.Category.List", $"Admin/CategoryAdmin/List",
            //    new { controller = "CategoryAdmin", action = "List", area = AreaNames.Admin });

            //endpointRouteBuilder.MapControllerRoute("NopValley.Category.Edit", $"Admin/CategoryAdmin/ProductList",
            //    new { controller = "CategoryAdmin", action = "Edit", area = AreaNames.Admin });

        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => 100;
    }
}