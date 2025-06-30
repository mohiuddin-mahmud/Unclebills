using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;

namespace NopValley.Plugins.Misc.Areas.Admin.Services
{
    /// <summary>
    /// Represents default values related to service post catalog services
    /// </summary>
    public static partial class NopServicePostCatalogDefaults
    {
        
        #region Caching defaults

        #region ServicePost Categories
        

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : current store ID
        /// {1} : roles of the current user
        /// {2} : show hidden records?
        /// </remarks>
        public static CacheKey ServicePostCategoriesAllCacheKey => new CacheKey("Nop.servicepostcategory.all.{0}-{1}-{2}", NopEntityCacheDefaults<Category>.AllPrefix);

        

        #endregion     

        #endregion
    }
}