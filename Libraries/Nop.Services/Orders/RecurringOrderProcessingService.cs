using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers;
using AuthorizeNet.Api.Controllers.Bases;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Configuration;
using Nop.Services.Payments;
using System;
using System.Linq;

namespace Nop.Services.Orders
{
	public class RecurringOrderProcessingService
	{
		public static ARBCreateSubscriptionResponse CreateSubscription(RecurringOrder recurringOrder, IOrderService _orderService, ProcessPaymentRequest processPaymentRequest, ISettingService _settingService)
		{
			if(_settingService.GetSetting("authorizenetpaymentsettings.usesandbox").Value == "True")	
			{
				ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.SANDBOX;
			}
			else
			{
				ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.PRODUCTION;
			}

			ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType
			{
				name = _settingService.GetSetting("authorizenetpaymentsettings.loginid").Value,
				ItemElementName = ItemChoiceType.transactionKey,
				Item = _settingService.GetSetting("authorizenetpaymentsettings.transactionkey").Value
			};

			paymentScheduleType schedule = new paymentScheduleType()
			{
				interval = new paymentScheduleTypeInterval()
			};

			switch (recurringOrder.RecurringOrderPeriod)
			{
				case RecurringOrderPeriod.Every1Week:
					schedule.interval.length = 7;
					schedule.interval.unit = ARBSubscriptionUnitEnum.days;
					break;
				case RecurringOrderPeriod.Every2Weeks:
					schedule.interval.length = 7 * 2;
					schedule.interval.unit = ARBSubscriptionUnitEnum.days;
					break;
				case RecurringOrderPeriod.Every3Weeks:
					schedule.interval.length = 7 * 3;
					schedule.interval.unit = ARBSubscriptionUnitEnum.days;
					break;
				case RecurringOrderPeriod.Every4Weeks:
					schedule.interval.length = 7 * 4;
					schedule.interval.unit = ARBSubscriptionUnitEnum.days;
					break;
				case RecurringOrderPeriod.Every5Weeks:
					schedule.interval.length = 7 * 5;
					schedule.interval.unit = ARBSubscriptionUnitEnum.days;
					break;
				case RecurringOrderPeriod.Every6Weeks:
					schedule.interval.length = 7 * 6;
					schedule.interval.unit = ARBSubscriptionUnitEnum.days;
					break;
				case RecurringOrderPeriod.Every7Weeks:
					schedule.interval.length = 7 * 7;
					schedule.interval.unit = ARBSubscriptionUnitEnum.days;
					break;
				case RecurringOrderPeriod.Every8Weeks:
					schedule.interval.length = 7 * 8;
					schedule.interval.unit = ARBSubscriptionUnitEnum.days;
					break;
				case RecurringOrderPeriod.Every3Months:
					schedule.interval.length = 3;
					schedule.interval.unit = ARBSubscriptionUnitEnum.months;
					break;
				case RecurringOrderPeriod.Every4Months:
					schedule.interval.length = 4;
					schedule.interval.unit = ARBSubscriptionUnitEnum.months;
					break;
				case RecurringOrderPeriod.Every5Months:
					schedule.interval.length = 5;
					schedule.interval.unit = ARBSubscriptionUnitEnum.months;
					break;
				case RecurringOrderPeriod.Every6Months:
					schedule.interval.length = 6;
					schedule.interval.unit = ARBSubscriptionUnitEnum.months;
					break;
				default:
					throw new Exception("Error with interval");
			}

			schedule.startDate = recurringOrder.NextOrderUtc;
			schedule.totalOccurrences = 9999; //set to 9999 for forever https://developer.authorize.net/api/reference/index.html#recurring-billing
			schedule.trialOccurrences = 0;

			#region Payment Information
			var initialOrder = _orderService.GetOrderByIdAsync(recurringOrder.InitialOrderId).Result;

			string expireMonth = processPaymentRequest.CreditCardExpireMonth.ToString();
			if (expireMonth.Length == 1)
				expireMonth = expireMonth.Insert(0, "0"); // add a leading zero if single digit month
			string expireYear = processPaymentRequest.CreditCardExpireYear.ToString().Substring(2);
			if (expireYear.Length > 2)
				expireYear = expireYear.Substring(expireYear.Length - 2);

			var creditCard = new creditCardType
			{
				cardNumber = processPaymentRequest.CreditCardNumber,
				expirationDate = expireMonth + expireYear,
				cardCode = processPaymentRequest.CreditCardCvv2
			};

			//standard api call to retrieve response
			paymentType cc = new paymentType { Item = creditCard };
			#endregion

			nameAndAddressType billingInfo = new nameAndAddressType()
			{
				firstName = initialOrder.BillingAddress.FirstName,
				lastName = initialOrder.BillingAddress.LastName,
				address = initialOrder.BillingAddress.Address1 + " " + initialOrder.BillingAddress.Address2,
				city = initialOrder.BillingAddress.City,
				state = initialOrder.BillingAddress.StateProvince.Name,
                zip = initialOrder.BillingAddress.ZipPostalCode,
				country = initialOrder.BillingAddress.Country.Name,
				company = initialOrder.BillingAddress.Company
			};

			nameAndAddressType shippingInfo = new nameAndAddressType()
			{
				firstName = initialOrder.ShippingAddress.FirstName,
				lastName = initialOrder.ShippingAddress.LastName,
				address = initialOrder.ShippingAddress.Address1 + " " + initialOrder.ShippingAddress.Address2,
				city = initialOrder.ShippingAddress.City,
				state = initialOrder.ShippingAddress.StateProvince.Name,
				zip = initialOrder.ShippingAddress.ZipPostalCode,
				country = initialOrder.ShippingAddress.Country.Name,
				company = initialOrder.ShippingAddress.Company
			};

			ARBSubscriptionType subscriptionType = new ARBSubscriptionType()
			{
				amount = processPaymentRequest.OrderTotal,
				trialAmount = 0.00m,
				paymentSchedule = schedule,
				billTo = billingInfo,
				shipTo = shippingInfo,
				payment = cc
			};

			var request = new ARBCreateSubscriptionRequest { subscription = subscriptionType };

			var controller = new ARBCreateSubscriptionController(request);          // instantiate the controller that will call the service
			controller.Execute();

			ARBCreateSubscriptionResponse response = controller.GetApiResponse();   // get the response from the service (errors contained if any)

			// validate response
			if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
			{
				if (response != null && response.messages.message != null)
				{
					Console.WriteLine("Success, Subscription ID : " + response.subscriptionId.ToString());
				}
			}
			else if (response != null)
			{
				Console.WriteLine("Error: " + response.messages.message[0].code + "  " + response.messages.message[0].text);
			}
			else if (response == null)
			{
				//throw new Exception("The API response is null");
				ANetApiResponse errorResponse = controller.GetErrorResponse();
				if (errorResponse.messages.message.Length > 0)
				{
					throw new Exception(errorResponse.messages.message.FirstOrDefault().text.ToString());
				}
				else 
				{
					throw new Exception("Error response null");
				}
			}

			return response;
		}

		public static ANetApiResponse CancelSubscription(string subscriptionId, ISettingService _settingService)
		{
			if (_settingService.GetSetting("authorizenetpaymentsettings.usesandbox").Value == "True")
			{
				ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.SANDBOX;
			}
			else
			{
				ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.PRODUCTION;
			}
			ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType
			{
				name = _settingService.GetSetting("authorizenetpaymentsettings.loginid").Value,
				ItemElementName = ItemChoiceType.transactionKey,
				Item = _settingService.GetSetting("authorizenetpaymentsettings.transactionkey").Value
			};

			//Please change the subscriptionId according to your request
			var request = new ARBCancelSubscriptionRequest { subscriptionId = subscriptionId };
			var controller = new ARBCancelSubscriptionController(request);                          // instantiate the controller that will call the service
			controller.Execute();

			ARBCancelSubscriptionResponse response = controller.GetApiResponse();                   // get the response from the service (errors contained if any)

			// validate response
			if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
			{
				if (response != null && response.messages.message != null)
				{
					Console.WriteLine("Success, Subscription Cancelled With RefID : " + response.refId);
				}
			}
			else if (response != null)
			{
				Console.WriteLine("Error: " + response.messages.message[0].code + "  " + response.messages.message[0].text);
			}

			return response;
		}

		public static ANetApiResponse UpdateSubscriptionTotal(string subscriptionId, decimal amount, ISettingService _settingService)
		{
			if (_settingService.GetSetting("authorizenetpaymentsettings.usesandbox").Value == "True")
			{
				ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.SANDBOX;
			}
			else
			{
				ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.PRODUCTION;
			}
			ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType
			{
				name = _settingService.GetSetting("authorizenetpaymentsettings.loginid").Value,
				ItemElementName = ItemChoiceType.transactionKey,
				Item = _settingService.GetSetting("authorizenetpaymentsettings.transactionkey").Value
			};

			ARBSubscriptionType subscriptionType = new ARBSubscriptionType()
			{
				amount = amount
			};

			var request = new ARBUpdateSubscriptionRequest { subscription = subscriptionType, subscriptionId = subscriptionId };
			var controller = new ARBUpdateSubscriptionController(request);
			controller.Execute();

			ARBUpdateSubscriptionResponse response = controller.GetApiResponse();

			// validate response
			if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
			{
				if (response != null && response.messages.message != null)
				{
					Console.WriteLine("Success, RefID Code : " + response.refId);
				}
			}
			else if (response != null)
			{
				Console.WriteLine("Error: " + response.messages.message[0].code + "  " + response.messages.message[0].text);
			}

			return response;
		}

		public static ARBGetSubscriptionResponse GetSubscription(string subscriptionId, ISettingService _settingService)
		{
			if (_settingService.GetSetting("authorizenetpaymentsettings.usesandbox").Value == "True")
			{
				ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.SANDBOX;
			}
			else
			{
				ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.PRODUCTION;
			}
			ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType
			{
				name = _settingService.GetSetting("authorizenetpaymentsettings.loginid").Value,
				ItemElementName = ItemChoiceType.transactionKey,
				Item = _settingService.GetSetting("authorizenetpaymentsettings.transactionkey").Value
			};

			var request = new ARBGetSubscriptionRequest { subscriptionId = subscriptionId };

			var controller = new ARBGetSubscriptionController(request);          // instantiate the controller that will call the service
			controller.Execute();

			ARBGetSubscriptionResponse response = controller.GetApiResponse();   // get the response from the service (errors contained if any)

			// validate response
			if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
			{
				if (response.subscription != null)
				{
					Console.WriteLine("Subscription returned : " + response.subscription.name);
				}
			}
			else if (response != null)
			{
				if (response.messages.message.Length > 0)
				{
					Console.WriteLine("Error: " + response.messages.message[0].code + "  " +
									  response.messages.message[0].text);
				}
			}
			else
			{
				if (controller.GetErrorResponse().messages.message.Length > 0)
				{
					Console.WriteLine("Error: " + response.messages.message[0].code + "  " + response.messages.message[0].text);
				}
			}

			return response;
		}
	}
}