using Nop.Core.Caching;

namespace NopValley.Plugin.Widgets.MegaMenu.Areas.Admin.Infrastructure.Cache
{
    public static partial class NopModelCacheDefaults
    {
       
        /// <summary>
        /// Key for categories caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// </remarks>
        public static CacheKey SmartCategoriesListKey => new("Nop.pres.admin.smartcategories.list-{0}", SmartCategoriesListPrefixCacheKey);
        public static string SmartCategoriesListPrefixCacheKey => "Nop.pres.admin.smartcategories.list";              
       
    }
}
