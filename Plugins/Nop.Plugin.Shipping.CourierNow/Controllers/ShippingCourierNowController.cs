using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Shipping.CourierNow.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Shipping;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System.Text;


namespace Nop.Plugin.Shipping.CourierNow.Controllers;

[Area(AreaNames.ADMIN)]
[AuthorizeAdmin]
[AutoValidateAntiforgeryToken]
public class ShippingCourierNowController : BasePluginController
{
    private readonly CourierNowSettings _courierNowSettings;
    private readonly ISettingService _settingService;
    private readonly ILocalizationService _localizationService;
    private readonly IShippingService _shippingService;
    protected readonly INotificationService _notificationService;

    public ShippingCourierNowController(CourierNowSettings courierNowSettings,
        ISettingService settingService,
        ILocalizationService localizationService,
        IShippingService shippingService,
        INotificationService notificationService
        )
    {
        this._courierNowSettings = courierNowSettings;
        this._settingService = settingService;
        this._localizationService = localizationService;
        this._shippingService = shippingService;
        _notificationService = notificationService;

    }


    public async Task<IActionResult> Configure()
    {
        CourierNowShippingModel model = null;
        try
        {

            model = new CourierNowShippingModel
            {
                //FeePerShipment = this._courierNowSettings.FeePerShipment,
                //MaxWeightPerShipment = this._courierNowSettings.MaxWeightPerShipment,
                SupportedZipCodes = string.Join(",", this._courierNowSettings.SupportedZipCodes),
                //Radius = this._courierNowSettings.Radius
                OrderSubtotalEvalLine = this._courierNowSettings.OrderSubtotalEvalLine,
                OrderWeightEvalLine = this._courierNowSettings.OrderWeightEvalLine,
                UnderSubtotalEvalShippingFee = this._courierNowSettings.UnderSubtotalEvalShippingFee,
                OverSubtotalEvalShippingFee = this._courierNowSettings.OverSubtotalEvalShippingFee,
                UnderWeightEvalShippingDiscount = this._courierNowSettings.UnderWeightEvalShippingDiscount
            };
        }
        catch
        {
            model = new CourierNowShippingModel
            {
                //FeePerShipment = this._courierNowSettings.FeePerShipment,
                //MaxWeightPerShipment = this._courierNowSettings.MaxWeightPerShipment,
                SupportedZipCodes = string.Empty,
                //Radius = this._courierNowSettings.Radius
                OrderSubtotalEvalLine = 0.0,
                OrderWeightEvalLine = 0.0,
                UnderSubtotalEvalShippingFee = 0.0,
                OverSubtotalEvalShippingFee = 0.0,
                UnderWeightEvalShippingDiscount = 0.0
            };
        }

        //var allWarehousesAsync = ;
        model.Warehouses = await _shippingService.GetAllWarehousesAsync().Result.Select(p => new SelectListItem { Text = p.Name, Value = p.Id.ToString() }).OrderBy(p => p.Text).ToListAsync();
        SetSelectedWarehouses(model.Warehouses, this._courierNowSettings.Warehouses);

        return View("~/Plugins/Shipping.CourierNow/Views/Configure.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> Configure(CourierNowShippingModel model)
    {
        if (!ModelState.IsValid)
        {
            return await Configure();
        }

        //this._courierNowSettings.Radius = model.Radius;
        //this._courierNowSettings.MaxWeightPerShipment = model.MaxWeightPerShipment;
        //this._courierNowSettings.FeePerShipment = model.FeePerShipment;
        this._courierNowSettings.Warehouses = GetWarehouseIds(Request.Form["Warehouse"]);
        this._courierNowSettings.SupportedZipCodes = model.SupportedZipCodes.Split(',').ToList();

        this._courierNowSettings.OrderSubtotalEvalLine = model.OrderSubtotalEvalLine;
        this._courierNowSettings.OrderWeightEvalLine = model.OrderWeightEvalLine;
        this._courierNowSettings.UnderSubtotalEvalShippingFee = model.UnderSubtotalEvalShippingFee;
        this._courierNowSettings.OverSubtotalEvalShippingFee = model.OverSubtotalEvalShippingFee;
        this._courierNowSettings.UnderWeightEvalShippingDiscount = model.UnderWeightEvalShippingDiscount;

        this._settingService.SaveSetting(this._courierNowSettings);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));


        return await Configure();
    }

    private string GetWarehouseIds(string form)
    {


        if (form == null)
            return String.Empty;

        var sb = new StringBuilder();

        var wareHouses = form.Split(',');
        foreach (var warehouse in wareHouses)
        {
            if (!String.IsNullOrEmpty(warehouse))
                sb.AppendFormat("{0}:", warehouse);
        }
        return sb.ToString();
    }

    private void SetSelectedWarehouses(List<SelectListItem> warehouses, string warehousesAsString)
    {
        if (warehousesAsString == null)
            return;

        var selected = warehousesAsString.Split(':');
        foreach (var id in selected)
        {
            var warehouse = warehouses.Where(p => p.Value == id).FirstOrDefault();
            if (warehouse != null)
                warehouse.Selected = true;
        }
    }
}