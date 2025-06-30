using Nop.Core.Caching;

namespace NopValley.Plugins.Misc.Areas.Admin.Infrastructure.Cache;

public static partial class NopModelCacheDefaults
{
    
    /// <summary>
    /// Key for categories caching
    /// </summary>
    public static CacheKey CategoriesListKey => new("Nop.pres.admin.categories.list");

       
}