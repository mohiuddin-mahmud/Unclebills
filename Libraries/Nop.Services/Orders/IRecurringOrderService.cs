using AuthorizeNet.APICore;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Services.Payments;
using System;
using System.Collections.Generic;

namespace Nop.Services.Orders
{
	public partial interface IRecurringOrderService
	{
		void CreateRecurringOrder(int orderId, RecurringOrderContext cxt, ProcessPaymentRequest processPaymentRequest);
		List<RecurringOrder> GetRecurringOrdersByCustomerGuid(Guid customerGuid, bool filterUpcomingOrders = false);
		List<ShoppingCartItem> GetRecurringOrderShoppingCartItems(int recurringOrderId);
		RecurringOrder GetRecurringOrderById(int id);
		RecurringOrder GetRecurringOrderByShoppingCartItem(ShoppingCartItem item);
		void DeleteRecurringOrder(int id);
		Order GetInitialOrderByRecurringOrderId(int recurringOrderId);
		Task<bool> UpdateSubscriptionTotal(int recurringOrderId);
		string GetSubscriptionId(int recurringOrderId);
		Task HandleUpdateRecurringOrderSubscription(int recurringOrderId);
        Task UpdateRecurringOrdersForTomorrow();
        Task<List<RecurringOrder>> GetAllRecurringOrders();
        Task<RecurringOrderSubscription> GetSubscription(string subscriptionId);
        Task<IPagedList<RecurringOrder>> SearchRecurringOrders(int recurringOrderId = 0, int customerId = 0, string customerFirstName = null, string customerLastName = null,
			DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int pageIndex = 0, int pageSize = int.MaxValue);
	}
}
