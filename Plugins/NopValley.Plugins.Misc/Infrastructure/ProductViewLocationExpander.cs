//using Microsoft.AspNetCore.Mvc.Razor;
//using System.Collections.Generic;
//using System.Linq;

//namespace NopValley.Plugins.Misc.Infrastructure
//{
//    public class ProductViewLocationExpander : IViewLocationExpander
//    {
//        public void PopulateValues(ViewLocationExpanderContext context)
//        {
//        }

//        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
//        {
//            if (context.ViewName.Equals("ProductDetails", System.StringComparison.OrdinalIgnoreCase))
//            {
//                return new[] {
//                    "/Plugins/NopValley.Misc/Views/Product/{0}.cshtml"
//                }.Concat(viewLocations);
//            }

//            return viewLocations;
//        }
//    }
//}
