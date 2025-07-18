﻿using Nop.Core.Domain.Orders;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Common;

namespace Nop.Web.Models.Checkout;

public partial record CheckoutShippingAddressModel : BaseNopModel
{
    public CheckoutShippingAddressModel()
    {
        ExistingAddresses = new List<AddressModel>();
        InvalidExistingAddresses = new List<AddressModel>();
        ShippingNewAddress = new AddressModel();
    }

    public IList<AddressModel> ExistingAddresses { get; set; }
    public IList<AddressModel> InvalidExistingAddresses { get; set; }
    public AddressModel ShippingNewAddress { get; set; }
    public bool NewAddressPreselected { get; set; }

    public int SelectedBillingAddress { get; set; }

    public bool DisplayPickupInStore { get; set; }
    public bool IsRecurringOrder { get; set; }

    public RecurringOrderPeriod RecurringOrderPeriod { get; set; }
    public string RecurringOrderName { get; set; }
    public bool IsGuest { get; set; }
    public CheckoutPickupPointsModel PickupPointsModel { get; set; }

    public bool BillToSameAddress { get; set; }


}