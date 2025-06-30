using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopValley.Plugins.Misc.Infrastructure;

namespace NopValley.Plugins.Misc
{

    public class MiscPlugin : BasePlugin
    {
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IApplicationBuilder _applicationBuilder;
        public MiscPlugin(ISettingService settingService, 
            ILocalizationService localizationService,
            IApplicationBuilder applicationBuilder)
        {
            _settingService = settingService;
            _localizationService = localizationService;
            _applicationBuilder = applicationBuilder;
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        //public override string GetConfigurationPageUrl()
        //{            
        //    return $"{_webHelper.GetStoreLocation()}Admin/WidgetsMegaMenu/Configure";
        //}



        /// <summary>
        /// Install plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task InstallAsync()
        {

 

            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {

               
            
            });
           
           
      

          //  await _referenceService.InsertReferenceConfigurationAsync(settings);            
            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task UninstallAsync()
        {
            //locales
            await _localizationService.DeleteLocaleResourcesAsync("NopValley.Plugin.Misc");
            await base.UninstallAsync();
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {          
            return Task.FromResult<IList<string>>(new List<string> {});
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
        /// </summary>
        public bool HideInWidgetList => false;



            
        //public Task<IList<string>> GetWidgetZonesAsync()
        //{
        //    return Task.FromResult<IList<string>>(new List<string> {              
        //        PublicWidgetZones.HomepageBottom,
        //        PublicWidgetZones.BlogPostPageBeforeBody,
        //        AdminWidgetZones.BlogPostDetailsBlock  
        //    });
        //}

        //public Type GetWidgetViewComponent(string widgetZone)
        //{
        //    if(widgetZone == PublicWidgetZones.HomepageBottom)
        //        return typeof(BlogViewComponent);

        //    if (widgetZone == PublicWidgetZones.BlogPostPageBeforeBody)
        //        return typeof(BlogPictureViewComponent);

        //    else if (widgetZone == AdminWidgetZones.BlogPostDetailsBlock)
        //        return typeof(BlogAdminViewComponent);

        //    return null;
        //}
    }
}
