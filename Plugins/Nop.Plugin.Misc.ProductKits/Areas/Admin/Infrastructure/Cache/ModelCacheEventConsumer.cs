using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Configuration;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Services.Events;
using Nop.Services.Plugins;
using NopValley.Plugin.Widgets.MegaMenu.Domain;

namespace NopValley.Plugin.Widgets.MegaMenu.Areas.Admin.Infrastructure.Cache
{
    /// <summary>
    /// Model cache event consumer (used for caching of presentation layer models)
    /// </summary>
    public partial class ModelCacheEventConsumer :
       
        //megamenu categories
        IConsumer<EntityInsertedEvent<MegaMenuCategory>>,
        IConsumer<EntityUpdatedEvent<MegaMenuCategory>>,
        IConsumer<EntityDeletedEvent<MegaMenuCategory>>

      
    {
        #region Fields

        private readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region Ctor

        public ModelCacheEventConsumer(IStaticCacheManager staticCacheManager)
        {
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Methods

      
        //categories
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityInsertedEvent<MegaMenuCategory> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.SmartCategoriesListPrefixCacheKey);
        }
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityUpdatedEvent<MegaMenuCategory> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.SmartCategoriesListPrefixCacheKey);
        }
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityDeletedEvent<MegaMenuCategory> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.SmartCategoriesListPrefixCacheKey);
        }

        

     
        #endregion
    }
}