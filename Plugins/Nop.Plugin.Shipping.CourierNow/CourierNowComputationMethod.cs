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

namespace Nop.Plugin.Shipping.CourierNow
{
    public class CourierNowComputationMethod : BasePlugin, IShippingRateComputationMethod, IRestrictedWarehouses
    {
        private readonly CourierNowSettings _courierNowSettings;
        private readonly IShippingService _shippingService;
        private readonly IRepository<Warehouse> _warehouseRepository;
        private readonly ILocationService _locationService;

        private bool initialRequest = false;
        private Guid lastRequest;
        private int closestWarehouse;
        private int currentRequest=0;
        private int totalRequests = 0;
        private decimal currentRequestWeight;

        public CourierNowComputationMethod(CourierNowSettings courierNowSettings, IRepository<Warehouse> warehouseRepository, ILocationService locationService, IShippingService shippingService)
        {
            this._courierNowSettings = courierNowSettings;
            this._warehouseRepository = warehouseRepository;
            this._locationService = locationService;
            this._shippingService = shippingService;
        }

        public ShippingRateComputationMethodType ShippingRateComputationMethodType
        {
            get
            {
                return ShippingRateComputationMethodType.Realtime;
            }
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

        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "ShippingCourierNow";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Shipping.CourierNow.Controllers" }, { "area", null } };
        }

        public decimal? GetFixedRate(GetShippingOptionRequest getShippingOptionRequest)
        {
            return null;
        }

        public GetShippingOptionResponse GetShippingOptions(GetShippingOptionRequest getShippingOptionRequest)
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
            ShippingOption twoDay = GetTwoDayOption(getShippingOptionRequest);
            if (twoDay != null)
            {
                response.ShippingOptions.Add(twoDay);
                return response; // if two-day option is available, we will never offer standard
            }

            // Check if standard option is available
            ShippingOption standard = GetStandardOption(getShippingOptionRequest);
            if (standard != null)
                response.ShippingOptions.Add(standard);

            return response;
        }

        private ShippingOption GetStandardOption(GetShippingOptionRequest getShippingOptionRequest)
        {
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

        private ShippingOption GetTwoDayOption(GetShippingOptionRequest getShippingOptionRequest)
        {
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
            decimal shippingCost = 10 * Math.Ceiling(this._shippingService.GetTotalWeight(getShippingOptionRequest, true) / 51);
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

        public override void Install()
        {
            base.Install();
        }

        public override void Uninstall()
        {
            base.Uninstall();
        }
    }
}
