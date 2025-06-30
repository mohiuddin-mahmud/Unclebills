using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Nop.Plugin.Shipping.CourierNow.Models
{
    public class CourierNowShippingModel
    {
        //public double Radius { get; set; }
        //public double FeePerShipment { get; set; }
        //public double MaxWeightPerShipment { get; set; }
        public List<SelectListItem> Warehouses { get; set; }
        public string SupportedZipCodes { get; set; }

        public double OrderSubtotalEvalLine { get; set; }
        public double OrderWeightEvalLine { get; set; }
        public double UnderSubtotalEvalShippingFee { get; set; }
        public double OverSubtotalEvalShippingFee { get; set; }
        public double UnderWeightEvalShippingDiscount { get; set; }
    }
}
