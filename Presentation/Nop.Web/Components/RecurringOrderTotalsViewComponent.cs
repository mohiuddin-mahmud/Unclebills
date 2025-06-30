using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Orders;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;

namespace Nop.Web.Components;

public partial class RecurringOrderTotalsViewComponent : NopViewComponent
{
    protected readonly IShoppingCartModelFactory _shoppingCartModelFactory;
    protected readonly IShoppingCartService _shoppingCartService;
    protected readonly IStoreContext _storeContext;
    protected readonly IWorkContext _workContext;

    public RecurringOrderTotalsViewComponent(IShoppingCartModelFactory shoppingCartModelFactory,
        IShoppingCartService shoppingCartService,
        IStoreContext storeContext,
        IWorkContext workContext)
    {
        _shoppingCartModelFactory = shoppingCartModelFactory;
        _shoppingCartService = shoppingCartService;
        _storeContext = storeContext;
        _workContext = workContext;
    }

    public async Task<IViewComponentResult> InvokeAsync(bool isEditable, int recurringOrderId)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(),
            ShoppingCartType.ShoppingCart, store.Id);
        cart = cart.Where(sci => sci.ShoppingCartType == ShoppingCartType.Recurring && sci.RecurringOrderId == recurringOrderId).ToList();
        

        var model = await _shoppingCartModelFactory.PrepareOrderTotalsModelAsync(cart, isEditable, recurringOrderId);
        return View(model);
    }
}