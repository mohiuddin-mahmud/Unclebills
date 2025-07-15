using Nop.Core.Configuration;

namespace Nop.Plugin.Shipping.CourierNow;

public class CourierNowSettings : ISettings
{
    //public double Radius { get; set; }
    //public double FeePerShipment { get; set; }
    //public double MaxWeightPerShipment { get; set; }
    public string Warehouses { get; set; }
    public List<string> SupportedZipCodes { get; set; }

    public double OrderSubtotalEvalLine { get; set; }
    public double OrderWeightEvalLine { get; set; }
    public double UnderSubtotalEvalShippingFee { get; set; }
    public double OverSubtotalEvalShippingFee { get; set; }
    public double UnderWeightEvalShippingDiscount { get; set; }
}
