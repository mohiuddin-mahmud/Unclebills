using AuthorizeNet.Api.Contracts.V1;
using Nop.Core;

using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Shipping;
using Nop.Services.Configuration;
using Nop.Data;

namespace Nop.Services.Orders
{
	public partial class RecurringOrderService : IRecurringOrderService
	{
		private readonly IRepository<RecurringOrder> _roRepository;
		private readonly IWorkContext _workContext;
		private readonly IProductService _productService;
		private readonly IOrderService _orderService;
		private readonly IShoppingCartService _shoppingCartService;
		private readonly ICustomerService _customerService;
		private readonly IRepository<ShoppingCartItem> _sciRepository;
		private readonly IOrderTotalCalculationService _orderTotalCalculationService;
		private readonly IWorkflowMessageService _workflowMessageService;
		private readonly LocalizationSettings _localizationSettings;
		private readonly ILogger _logger;
		private readonly ISettingService _settingService;

		public RecurringOrderService(IRepository<RecurringOrder> roRepository,
			IWorkContext workContext,
			IProductService productService,
			IOrderService orderService,
			IShoppingCartService shoppingCartService,
			ICustomerService customerService,
			IRepository<ShoppingCartItem> sciRepository,
			IOrderTotalCalculationService orderTotalCalculationService,
			IWorkflowMessageService workflowMessageService,
			LocalizationSettings localizationSettings,
			ILogger logger,
			ISettingService settingService
			)
		{
			this._roRepository = roRepository;
			this._workContext = workContext;
			this._productService = productService;
			this._orderService = orderService;
			this._shoppingCartService = shoppingCartService;
			this._customerService = customerService;
			this._sciRepository = sciRepository;
			this._orderTotalCalculationService = orderTotalCalculationService;
			this._workflowMessageService = workflowMessageService;
			this._localizationSettings = localizationSettings;
			this._logger = logger;
			this._settingService = settingService;
			_shoppingCartService.UpdateRecurringOrderSubscription += new MyEventHandler(HandleUpdateRecurringOrderSubscription);
		}

		public async void CreateRecurringOrder(int orderId, RecurringOrderContext cxt, ProcessPaymentRequest processPaymentRequest)
		{
			var now = DateTime.UtcNow;
			RecurringOrder ro = new RecurringOrder();
			ro.CustomerId = _workContext.GetCurrentCustomerAsync().Result.Id;
			ro.InitialOrderId = orderId;
			ro.RecurringOrderPeriod = cxt.RecurringOrderPeriod;
			ro.CreatedOnUtc = now;
			ro.UpdatedOnUtc = now;
			ro.NextOrderUtc = now;
			ro.NextOrderUtc = GetNextOrderUtc(ro);
			ro.Name = (String.IsNullOrEmpty(cxt.Name) ? ro.CreatedOnUtc.ToString() : cxt.Name);

			// create the subscription
			ARBCreateSubscriptionResponse response = new ARBCreateSubscriptionResponse();
			try 
			{
				response = RecurringOrderProcessingService.CreateSubscription(ro, _orderService, processPaymentRequest, _settingService);

				if (response.subscriptionId == null)
				{
					_logger.Error("Create subscription failed: " + response.messages.message.FirstOrDefault().text);
					throw new Exception("Your order was placed, but the recurring order was not created. Error message: " + response.messages.message.FirstOrDefault().text);
				}
			}
			catch
			{
				_logger.Error("Create subscription failed: " + response.messages.message.FirstOrDefault().text);
				throw new Exception("Your order was placed, but the recurring order was not created. Error message: " + response.messages.message.FirstOrDefault().text);
			}


			ro.SubscriptionId = response.subscriptionId;
			_roRepository.Insert(ro);

			Console.WriteLine(response.messages.resultCode);
			Console.WriteLine(response.messages.message);

			// create the shopping cart items
			var order = await _orderService.GetOrderByIdAsync(orderId);
			foreach (var item in order.OrderItems)
			{
				await _shoppingCartService.AddToCartAsync(await _workContext.GetCurrentCustomerAsync(),
					item.Product,
					ShoppingCartType.Recurring,
					order.StoreId,
					item.AttributesXml,
					0,
					item.RentalStartDateUtc,
					item.RentalEndDateUtc,
					item.Quantity, 
					true,
					GetRecurringOrderByInitialOrderId(orderId), 
					true);
			}
		}

		private int GetRecurringOrderByInitialOrderId(int id)
		{
			var query = from ro in _roRepository.Table
						where ro.InitialOrderId == id && !ro.Deleted
						select ro;
			return query.ToList().FirstOrDefault().Id;
		}

		public List<RecurringOrder> GetRecurringOrdersByCustomerGuid(Guid customerGuid, bool filterUpcomingOrders = false)
		{
			int customerId = _customerService.GetCustomerByGuidAsync(customerGuid).Id;

			if (!filterUpcomingOrders)
			{
				var query = from ro in _roRepository.Table
							where ro.CustomerId == customerId && !ro.Deleted
							select ro;
				return query.ToList();
			}
			else {
				var today = DateTime.UtcNow;
				var tomorrow = today.AddDays(1);

				var query = from ro in _roRepository.Table
							where ro.CustomerId == customerId && !ro.Deleted && ro.NextOrderUtc.Date != today.Date && ro.NextOrderUtc.Date != tomorrow.Date
							select ro;
				return query.ToList();
			}
		}

		public RecurringOrder GetRecurringOrderById(int id)
		{
			var query = from ro in _roRepository.Table
						where ro.Id == id && !ro.Deleted
						select ro;
			return query.ToList().FirstOrDefault();
		}

		public List<ShoppingCartItem> GetRecurringOrderShoppingCartItems(int recurringOrderId)
		{
			var query = from sci in _sciRepository.Table
						where sci.RecurringOrderId == recurringOrderId
						select sci;
			return query.ToList();
		}

		public RecurringOrder GetRecurringOrderByShoppingCartItem(ShoppingCartItem item)
		{
			var query = from ro in _roRepository.Table
						where ro.Id == item.RecurringOrderId
						select ro;
			return query.ToList().FirstOrDefault();
		}

		public void DeleteRecurringOrder(int id)
		{
			var query = from ro in _roRepository.Table
						where ro.Id == id
						select ro;
			var recurringOrder = query.ToList().FirstOrDefault();

			ANetApiResponse response = RecurringOrderProcessingService.CancelSubscription(recurringOrder.SubscriptionId, _settingService);

			if (response.messages.resultCode == messageTypeEnum.Error)
				throw new Exception("Error deleting the subscription");

			var items = GetRecurringOrderShoppingCartItems(recurringOrder.Id);
			var initialOrder = GetInitialOrderByRecurringOrderId(recurringOrder.Id);
			_workflowMessageService.SendRecurringOrderCancelledCustomerNotification(recurringOrder, _localizationSettings.DefaultAdminLanguageId, items, initialOrder);
			_workflowMessageService.SendRecurringOrderCancelledStoreNotification(recurringOrder, _localizationSettings.DefaultAdminLanguageId, items, initialOrder);

			recurringOrder.Deleted = true;
			recurringOrder.UpdatedOnUtc = DateTime.UtcNow;
			_roRepository.Update(recurringOrder);

		}

		public Order GetInitialOrderByRecurringOrderId(int recurringOrderId)
		{  
			var ro = GetRecurringOrderById(recurringOrderId);
			return _orderService.GetOrderByIdAsync(ro.InitialOrderId).Result;
		}

		public async Task<bool> UpdateSubscriptionTotal(int recurringOrderId)
		{
			// returns true if successful
			var cart = GetRecurringOrderShoppingCartItems(recurringOrderId);
			var initialOrder = GetInitialOrderByRecurringOrderId(recurringOrderId);
			decimal? amount = await _orderTotalCalculationService.CalculateRecurringOrderTotal(initialOrder, cart, null, true, true, recurringOrderId);

			if (amount.HasValue)
			{
				if (amount == decimal.Zero)
				{
					return false;
				}

				ANetApiResponse response = RecurringOrderProcessingService.UpdateSubscriptionTotal(GetSubscriptionId(recurringOrderId), (decimal)amount, _settingService);

				if (response.messages.resultCode == messageTypeEnum.Error)
				{
					_logger.Error("Error with AuthorizeNet subscription service: " + response.messages.message.FirstOrDefault().text);
					return false;
				}
				else if (response.messages.resultCode == messageTypeEnum.Ok)
				{
					return true;
				}
				return false;
			}
			else 
			{
				throw new Exception("Error getting amount");
			}
		}

		public string GetSubscriptionId(int recurringOrderId)
		{
			var query = from ro in _roRepository.Table
						where ro.Id == recurringOrderId && !ro.Deleted
						select ro;
			var recurringOrder = query.ToList().FirstOrDefault();
			return recurringOrder.SubscriptionId;
		}

		public async Task HandleUpdateRecurringOrderSubscription(int recurringOrderId)
		{
			await UpdateSubscriptionTotal(recurringOrderId);
		}

		public async Task UpdateRecurringOrdersForTomorrow()
		{
			var today = DateTime.UtcNow; //convert to PST here?
			var in1Day = today.AddDays(1);
			var in3Days = today.AddDays(3);

			// today
			var query = from ro in _roRepository.Table
						where ro.NextOrderUtc.Date == today.Date && !ro.Deleted
						select ro;

			var ordersToUpdate = query.ToList();

			foreach(var ro in ordersToUpdate)
			{
				ro.NextOrderUtc = GetNextOrderUtc(ro);
				ro.UpdatedOnUtc = today;
				_roRepository.Update(ro);
			}

			// in 1 day from now
			query = from ro in _roRepository.Table
						where ro.NextOrderUtc.Date == in1Day.Date && !ro.Deleted
						select ro;

			ordersToUpdate = query.ToList();
			foreach(var ro in ordersToUpdate)
			{
				var items = GetRecurringOrderShoppingCartItems(ro.Id);
				var initialOrder = GetInitialOrderByRecurringOrderId(ro.Id);

				bool success = await UpdateSubscriptionTotal(ro.Id);

				if (success)
				{
					_workflowMessageService.SendRecurringOrderStoreNotification(ro, _localizationSettings.DefaultAdminLanguageId, items, initialOrder);
				}
				else
				{
					DeleteRecurringOrder(ro.Id);
				}
			}

			// in 3 days from now
			query = from ro in _roRepository.Table
					where ro.NextOrderUtc == in3Days.Date && !ro.Deleted
					select ro;

			ordersToUpdate = query.ToList();
			foreach (var ro in ordersToUpdate)
			{
				UpdateSubscriptionTotal(ro.Id);
				var items = GetRecurringOrderShoppingCartItems(ro.Id);
				var initialOrder = GetInitialOrderByRecurringOrderId(ro.Id);
				_workflowMessageService.SendRecurringOrderCustomerNotification(ro, _localizationSettings.DefaultAdminLanguageId, items, initialOrder);
			}
		}

		public async Task<List<RecurringOrder>> GetAllRecurringOrders()
		{
			var query = from ro in _roRepository.Table
						where !ro.Deleted
						select ro;

			return await query.ToListAsync();
		}

		public async Task<RecurringOrderSubscription> GetSubscription(string subscriptionId)
		{
			ARBGetSubscriptionResponse response = RecurringOrderProcessingService.GetSubscription(subscriptionId, _settingService);

			var subscription = new RecurringOrderSubscription
			{
				Name = response.subscription.name,
				Total = response.subscription.amount,
				ShippingStreetAddress = response.subscription.profile.shippingProfile.address,
				ShippingCity = response.subscription.profile.shippingProfile.city,
				ShippingState = response.subscription.profile.shippingProfile.state,
				ShippingZip = response.subscription.profile.shippingProfile.zip
			};
            await Task.Yield();
			return subscription;
		}

		public virtual async Task<IPagedList<RecurringOrder>> SearchRecurringOrders(int recurringOrderId = 0, int customerId = 0, string customerFirstName = null, string customerLastName = null,
			DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int pageIndex = 0, int pageSize = int.MaxValue)
		{
			var query = _roRepository.Table;

			query = query.Where(ro => !ro.Deleted);

			if (recurringOrderId > 0)
				query = query.Where(ro => ro.Id == recurringOrderId);
			if (customerId > 0)
				query = query.Where(ro => ro.CustomerId == customerId);
			if (createdFromUtc.HasValue)
				query = query.Where(ro => createdFromUtc.Value <= ro.CreatedOnUtc);
			if (createdToUtc.HasValue)
				query = query.Where(ro => createdToUtc.Value >= ro.CreatedOnUtc);

			if (!string.IsNullOrEmpty(customerFirstName) || !string.IsNullOrEmpty(customerLastName))
			{
				// search by customer name
				if (customerFirstName != null)
					customerFirstName = customerFirstName.Trim();
				if (customerLastName != null)
					customerLastName = customerLastName.Trim();

				var customers = await _customerService.GetAllCustomersAsync(null, null, null, null,0, 0, null, customerFirstName, customerLastName);
				
                if (customers.Any())
				{
					var query2 = query.AsEnumerable();
					query2 = from ro in query2
								join c in customers.AsQueryable()
								on ro.CustomerId equals c.Id
								select ro;
                  
                    return new PagedList<RecurringOrder>(query2.OrderByDescending(ro => ro.CreatedOnUtc).ToList(), pageIndex, pageSize);
                   
                }
			}

			return new PagedList<RecurringOrder>(query.OrderByDescending(ro => ro.CreatedOnUtc).ToList(), pageIndex, pageSize);
		}

		private DateTime GetNextOrderUtc(RecurringOrder ro)
		{
			switch (ro.RecurringOrderPeriod)
			{
				case RecurringOrderPeriod.Every1Week:
					return ro.NextOrderUtc.AddDays(7);
				case RecurringOrderPeriod.Every2Weeks:
					return ro.NextOrderUtc.AddDays(7 * 2);
				case RecurringOrderPeriod.Every3Weeks:
					return ro.NextOrderUtc.AddDays(7 * 3);
				case RecurringOrderPeriod.Every4Weeks:
					return ro.NextOrderUtc.AddDays(7 * 4);
				case RecurringOrderPeriod.Every5Weeks:
					return ro.NextOrderUtc.AddDays(7 * 5);
				case RecurringOrderPeriod.Every6Weeks:
					return ro.NextOrderUtc.AddDays(7 * 6);
				case RecurringOrderPeriod.Every7Weeks:
					return ro.NextOrderUtc.AddDays(7 * 7);
				case RecurringOrderPeriod.Every8Weeks:
					return ro.NextOrderUtc.AddDays(7 * 8);
				case RecurringOrderPeriod.Every3Months:
					return ro.NextOrderUtc.AddMonths(3);
				case RecurringOrderPeriod.Every4Months:
					return ro.NextOrderUtc.AddMonths(4);
				case RecurringOrderPeriod.Every5Months:
					return ro.NextOrderUtc.AddMonths(5);
				case RecurringOrderPeriod.Every6Months:
					return ro.NextOrderUtc.AddMonths(6);
				default:
					throw new Exception("Cannot update NextOrderUtc");
			}
		}
	}
}
