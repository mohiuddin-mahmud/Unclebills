using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Data;
using Nop.Services.Plugins;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Tracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Custom;
using Microsoft.AspNetCore.Routing;
using Nop.Core;

namespace Nop.Plugin.Shipping.CourierNow
{
    public class CourierNowComputationMethod : BasePlugin, IShippingRateComputationMethod, IRestrictedWarehouses
    {
        private readonly CourierNowSettings _courierNowSettings;
        private readonly IShippingService _shippingService;
        private readonly IRepository<Warehouse> _warehouseRepository;
   
        protected readonly IWebHelper _webHelper;

        private bool initialRequest = false;
        private Guid lastRequest;
        private int closestWarehouse;
        private int currentRequest=0;
        private int totalRequests = 0;
        private decimal currentRequestWeight;

        public CourierNowComputationMethod(CourierNowSettings courierNowSettings, 
            IRepository<Warehouse> warehouseRepository,
          
            IShippingService shippingService,
            IWebHelper webHelper)
        {
            _courierNowSettings = courierNowSettings;
            _warehouseRepository = warehouseRepository;
      
            _shippingService = shippingService;
            _webHelper = webHelper;
        }

       
        public IShipmentTracker ShipmentTracker => throw new NotImplementedException();

        public IList<Warehouse> GetAllowedWarehouses(IList<Warehouse> allWarehouses)
        {
            if (this._courierNowSettings.Warehouses == null)
                return new List<Warehouse>();

            var ids = this._courierNowSettings.Warehouses.Split(':');
            List<int> ints = new List<int>();
            foreach(var id in ids)
            {
                int x = 0;
                if (int.TryParse(id, out x))
                    ints.Add(x);
            }
            return allWarehouses.Where(p => ints.Contains(p.Id)).ToList();
        }

       
        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/ShippingCourierNow/Configure";
        }

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the widget zones
        /// </returns>
        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> { });
        }

        public async Task<GetShippingOptionResponse> GetShippingOptionsAsync(GetShippingOptionRequest getShippingOptionRequest)
        {
            if (getShippingOptionRequest == null)
                throw new ArgumentNullException("getShippingOptionRequest");

            var response = new GetShippingOptionResponse();

            if (getShippingOptionRequest.Items == null)
            {
                response.AddError("No shipment items");
                return response;
            }

            if (getShippingOptionRequest.ShippingAddress == null)
            {
                response.AddError("Shipping address is not set");
                return response;
            }

            if (lastRequest != getShippingOptionRequest.RequestId)
            {
                lastRequest = getShippingOptionRequest.RequestId;
                initialRequest = true;
                currentRequestWeight = 0;
                currentRequest = 1;
            }
            else
            {
                currentRequest++;
                initialRequest = false;
            }

            totalRequests = 1;
            //totalRequests = getShippingOptionRequest.UniqueRequests;

            // Check if 2-day option is available
            ShippingOption twoDay = await GetTwoDayOption(getShippingOptionRequest);
            if (twoDay != null)
            {
                response.ShippingOptions.Add(twoDay);
                return response; // if two-day option is available, we will never offer standard
            }

            // Check if standard option is available
            ShippingOption standard = await GetStandardOption(getShippingOptionRequest);
            if (standard != null)
                response.ShippingOptions.Add(standard);

            return response;
        }

        private async Task<ShippingOption> GetStandardOption(GetShippingOptionRequest getShippingOptionRequest)
        {
            await Task.Yield();
            // can only return standard option if we're within the radius
            //if (_locationService.GetDistanceInMiles(getShippingOptionRequest.ShippingAddress, getShippingOptionRequest.WarehouseFrom) <= this._courierNowSettings.Radius)
            if (this._courierNowSettings.SupportedZipCodes.Contains(getShippingOptionRequest.ShippingAddress.ZipPostalCode.Trim()))
            {
                ShippingOption shippingOption = new ShippingOption();
                shippingOption.Name = "Courier Now Standard Shipping";
                shippingOption.Rate = CalculateRate(getShippingOptionRequest);
                shippingOption.ShippingRateComputationMethodSystemName = "Nop.Plugin.Shipping.CourierNow.CourierNowComputationMethod";
                shippingOption.WarehouseId = getShippingOptionRequest.WarehouseFrom.Id;
                return shippingOption;
            }
            else
                return null;

           
        }

        private async Task<ShippingOption> GetTwoDayOption(GetShippingOptionRequest getShippingOptionRequest)
        {
            await Task.Yield();

            if (!IsTwoDayAllowed())
                return null;

            // Check if the warehouse on the shipping request has all items in stock and is within the radius
            List<ShoppingCartItem> cartItems = getShippingOptionRequest.Items.Select(x => x.ShoppingCartItem).ToList();
            //if (_shippingService.AllItemsInStockAtWarehouse(cartItems, getShippingOptionRequest.WarehouseFrom.Id)
            //    && _locationService.GetDistanceInMiles(getShippingOptionRequest.ShippingAddress, getShippingOptionRequest.WarehouseFrom) <= this._courierNowSettings.Radius)
            if (_shippingService.AllItemsInStockAtWarehouse(cartItems, getShippingOptionRequest.WarehouseFrom.Id) && this._courierNowSettings.SupportedZipCodes.Contains(getShippingOptionRequest.ShippingAddress.ZipPostalCode.Trim()))
            {
                ShippingOption shippingOption = new ShippingOption();
                shippingOption.Name = "Courier Now 2-Day Shipping";
                shippingOption.Rate = CalculateRate(getShippingOptionRequest);
                shippingOption.ShippingRateComputationMethodSystemName = "Nop.Plugin.Shipping.CourierNow.CourierNowComputationMethod";
                shippingOption.WarehouseId = getShippingOptionRequest.WarehouseFrom.Id;
                return shippingOption;
            }
            else
                return null;
        }

        private decimal CalculateRate(GetShippingOptionRequest getShippingOptionRequest)
        {
            decimal shippingCost = 10 * Math.Ceiling(_shippingService.GetTotalWeightAsync(getShippingOptionRequest, true).Result / 51);
            if (getShippingOptionRequest.OrderSubTotal >= 60)
                shippingCost -= 5;
            return shippingCost;
            //if(getShippingOptionRequest.OrderSubTotal < Convert.ToDecimal(this._courierNowSettings.OrderSubtotalEvalLine))
            //{
            //    return Convert.ToDecimal(this._courierNowSettings.UnderSubtotalEvalShippingFee);
            //}
            //else if(getShippingOptionRequest.OrderSubTotal >= Convert.ToDecimal(this._courierNowSettings.OrderSubtotalEvalLine)
            //    && this._shippingService.GetTotalWeight(getShippingOptionRequest, true) < Convert.ToDecimal(this._courierNowSettings.OrderWeightEvalLine))
            //{
            //    return Convert.ToDecimal(this._courierNowSettings.OverSubtotalEvalShippingFee - this._courierNowSettings.UnderWeightEvalShippingDiscount);
            //}
            //else
            //{
            //    return Convert.ToDecimal(this._courierNowSettings.OverSubtotalEvalShippingFee);
            //}
            //currentRequestWeight = this._shippingService.GetTotalWeight(getShippingOptionRequest, true);
            //int numShipments = (int)Math.Ceiling(currentRequestWeight / Convert.ToDecimal(this._courierNowSettings.MaxWeightPerShipment));
            //return numShipments * Convert.ToDecimal(this._courierNowSettings.FeePerShipment);
        }

        private bool IsTwoDayAllowed()
        {
            DateTime localDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
            switch (localDateTime.DayOfWeek)
            {
                case DayOfWeek.Friday:
                    if (localDateTime.Hour >= 9)
                        return false;
                    break;
                case DayOfWeek.Saturday:
                    if (localDateTime.Hour < 12)
                        return false;
                    break;
            }
            return true;
        }

        public override async Task InstallAsync()
        {
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await base.UninstallAsync();
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            throw new NotImplementedException();
        }

        public Task<decimal?> GetFixedRateAsync(GetShippingOptionRequest getShippingOptionRequest)
        {
            return null;
        }

        public Task<IShipmentTracker> GetShipmentTrackerAsync()
        {
            return Task.FromResult<IShipmentTracker>(null);
        }

        /// <summary>
        /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
        /// </summary>
        public bool HideInWidgetList => false;
    }
}
