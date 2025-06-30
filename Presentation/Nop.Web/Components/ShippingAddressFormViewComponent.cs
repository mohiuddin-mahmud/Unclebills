using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Orders;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Checkout;

namespace Nop.Web.Components;

public partial class ShippingAddressFormViewComponent : NopViewComponent
{
    protected readonly ICustomerService _customerService;
    protected readonly IShoppingCartService _shoppingCartService;
    protected readonly AddressSettings _addressSettings;
    protected readonly IGenericAttributeService _genericAttributeService;
    protected readonly ShippingSettings _shippingSettings;
    protected readonly ICheckoutModelFactory _checkoutModelFactory;
    protected readonly IWorkContext _workContext;
    protected readonly IStoreContext _storeContext;
    protected readonly ICountryService _countryService;
    public ShippingAddressFormViewComponent(
        IShoppingCartService shoppingCartService,
        ICustomerService customerService,
        AddressSettings addressSettings, IGenericAttributeService genericAttributeService,
        ShippingSettings shippingSettings,
        ICheckoutModelFactory checkoutModelFactory,
        IWorkContext workContext,
        IStoreContext storeContext,
        ICountryService countryService
        )
    {
        _shoppingCartService = shoppingCartService;
        _addressSettings = addressSettings;
        _genericAttributeService = genericAttributeService;
        _shippingSettings = shippingSettings;
        _checkoutModelFactory = checkoutModelFactory;
        _workContext = workContext;
        _storeContext = storeContext;
        _countryService = countryService;
    }

    public async Task<IViewComponentResult> InvokeAsync(CheckoutProgressStep step)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

        var shippingAddressModel = new CheckoutShippingAddressModel();
        await _checkoutModelFactory.PrepareShippingAddressModelAsync(shippingAddressModel, cart, prePopulateNewAddressWithCustomerFields: true);

        bool billToSameAddress = await _genericAttributeService.GetAttributeAsync<bool>(customer,
            SystemCustomerAttributeNames.BillToSameAddress, store.Id);
        
        shippingAddressModel.BillToSameAddress = billToSameAddress;

        //if (await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
        //{
        //    //shipping is required
        //    var address = await _customerService.GetCustomerBillingAddressAsync(customer);

        //    //by default Shipping is available if the country is not specified
        //    var shippingAllowed = !_addressSettings.CountryEnabled || ((await _countryService.GetCountryByAddressAsync(address))?.AllowsShipping ?? false);
        //    if (_shippingSettings.ShipToSameAddress && shippingAddressModel.ShipToSameAddress && shippingAllowed)
        //    {
        //        //ship to the same address
        //        customer.ShippingAddressId = address.Id;
        //        await _customerService.UpdateCustomerAsync(customer);
        //        //reset selected shipping method (in case if "pick up in store" was selected)
        //        await _genericAttributeService.SaveAttributeAsync<ShippingOption>(customer, NopCustomerDefaults.SelectedShippingOptionAttribute, null, store.Id);
        //        await _genericAttributeService.SaveAttributeAsync<PickupPoint>(customer, NopCustomerDefaults.SelectedPickupPointAttribute, null, store.Id);
        //        //limitation - "Ship to the same address" doesn't properly work in "pick up in store only" case (when no shipping plugins are available) 
        //        return await OpcLoadStepAfterShippingAddress(cart);
        //    }

        //    //do not ship to the same address
        //    var shippingAddressModel = new CheckoutShippingAddressModel();
        //    await _checkoutModelFactory.PrepareShippingAddressModelAsync(shippingAddressModel, cart, prePopulateNewAddressWithCustomerFields: true);

            

        //    return View("OpcShippingAddress", shippingAddressModel);
        //}

        return View(shippingAddressModel);
    }
}