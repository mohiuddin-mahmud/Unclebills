using Nop.Services.Common;
using Nop.Core.Configuration;
using Nop.Services.Plugins;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Models.Sitemap;
using Nop.Services.Localization;
using Nop.Services.Cms;
using Nop.Core;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Misc.ProductKits;

public class ProductKitsPlugin : BasePlugin, IMiscPlugin, IWidgetPlugin
{
    protected readonly ILocalizationService _localizationService;
    protected readonly IWebHelper _webHelper;
    public ProductKitsPlugin(
        ILocalizationService localizationService,
        IWebHelper webHelper)
    {       
        _localizationService = localizationService;      
        _webHelper = webHelper;
    }

    #region Methods

    public override string GetConfigurationPageUrl()
    {
        return _webHelper.GetStoreLocation() + "Admin/ProductKits/Configure";
    }


    /// <summary>
    /// Gets a name of a view component for displaying widget
    /// </summary>
    /// <param name="widgetZone">Name of the widget zone</param>
    /// <returns>View component name</returns>
 

    /// <summary>
    /// Gets widget zones where this widget should be rendered
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the widget zones
    /// </returns>
    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string> {  });
    }


    //public void ManageSiteMap(SiteMapNode rootNode)
    //{
    //    var AdminCatalogNode = rootNode.ChildNodes.Where(x => x.Title == "Catalog").FirstOrDefault();
    //    var menuItem = new SiteMapNode()
    //    {
    //        SystemName = "Nop.Plugin.Misc.ProductKits.List",
    //        Title = "Kits",
    //        Url = "~/Admin/Plugins/Misc/ProductKits/Index",
    //        Visible = true,
    //        IconClass = "fa fa-dot-circle-o",
    //        RouteValues = new RouteValueDictionary() { { "area", "admin" } },
    //    };
    //    AdminCatalogNode.ChildNodes.Insert(1, menuItem);
    //}

    public override async Task InstallAsync()
    {

        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Misc.ProductKits.Category"] = "Category that kits are assigned to",
            ["Plugins.Misc.ProductKits.EditLink"] = "Manage kit",
            ["Plugins.Misc.ProductKits.Edit"] = "Edit kit",
            ["Plugins.Misc.ProductKits.Edit.Add"] = "Add product to kit",
            ["Plugins.Misc.ProductKits.Edit.AddProduct"] = "Add product",
            ["Plugins.Misc.ProductKits.Edit.List"] = "Products in kit",
            ["Plugins.Misc.ProductKits.Edit.Price"] = "Price of kit",
            ["Plugins.Misc.ProductKits.Edit.UpdatePrice"] = "Update Price",
            ["Plugins.Misc.ProductKits.Edit.Attribute"] = "Product type",
            ["Nop.Plugin.Misc.ProductKits.Title"] = "Kits",
            ["Nop.Plugin.Misc.ProductKits.AddNew"] = "Add a new kit",
            ["Nop.Plugin.Misc.ProductKits.BackToList"] = "back to kit list",
            ["Nop.Plugin.Misc.ProductKits.Info"] = "Kit info",
            ["Nop.Plugin.Misc.ProductKits.Tabs.Products"] = "Products"
        });

        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {

        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Misc.ProductKits");
        await _localizationService.DeleteLocaleResourcesAsync("Nop.Plugin.Misc.ProductKits");


        await base.UninstallAsync();
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        throw new NotImplementedException();
    }

    #endregion

    //#region Licensing

    //private bool IsLicensed()
    //{
    //    bool licensed = false;
    //    // look for license file and process
    //    string licensefilePath = System.Web.Hosting.HostingEnvironment.MapPath("~/Plugins/Misc.ProductKits");
    //    licensefilePath = Directory.GetFiles(licensefilePath, "*.lic").FirstOrDefault();
    //    if (!string.IsNullOrWhiteSpace(licensefilePath))
    //    {
    //        licensed = BoxCrushLicense.IsValid(licensefilePath);
    //    }
    //    return licensed;
    //}

    //#endregion

    #region Properties

    /// <summary>
    /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
    /// </summary>
    public bool HideInWidgetList => false;

    #endregion
}

public class ProductKitsPluginSettings : ISettings
{
    public int CategoryId { get; set; }
}
