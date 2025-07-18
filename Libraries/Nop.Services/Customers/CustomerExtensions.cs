﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Core.Infrastructure;
using Nop.Services.Common;
using Nop.Services.Localization;

namespace Nop.Services.Customers
{
    public static class CustomerExtensions
    {
        /// <summary>
        /// Get full name
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>Customer full name</returns>
        public static string GetFullName(this Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();
            var firstName = customer.FirstName; // customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName, genericAttributeService);
            var lastName = customer.LastName; // customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName, genericAttributeService);

            string fullName = "";
            if (!String.IsNullOrWhiteSpace(firstName) && !String.IsNullOrWhiteSpace(lastName))
                fullName = string.Format("{0} {1}", firstName, lastName);
            else
            {
                if (!String.IsNullOrWhiteSpace(firstName))
                    fullName = firstName;

                if (!String.IsNullOrWhiteSpace(lastName))
                    fullName = lastName;
            }
            return fullName;
        }

        /// <summary>
        /// Formats the customer name
        /// </summary>
        /// <param name="customer">Source</param>
        /// <param name="stripTooLong">Strip too long customer name</param>
        /// <param name="maxLength">Maximum customer name length</param>
        /// <returns>Formatted text</returns>
        public static string FormatUserName(this Customer customer, bool stripTooLong = false, int maxLength = 0)
        {
            if (customer == null)
                return string.Empty;
                        
            var customerService = EngineContext.Current.Resolve<ICustomerService>();
            if (customerService.IsGuestAsync(customer).Result)
            {
                return EngineContext.Current.Resolve<ILocalizationService>().GetResourceAsync("Customer.Guest").Result;
            }

            string result = string.Empty;
            var customerSettings = EngineContext.Current.Resolve<CustomerSettings>();
            var genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();

            switch (customerSettings.CustomerNameFormat)
            {
                case CustomerNameFormat.ShowEmails:
                    result = customer.Email;
                    break;
                case CustomerNameFormat.ShowUsernames:
                    result = customer.Username;
                    break;
                case CustomerNameFormat.ShowFullNames:
                    result = customer.GetFullName();
                    break;
                case CustomerNameFormat.ShowFirstName:
                    result = customer.FirstName;
                    break;
                default:
                    break;
            }

            if (stripTooLong && maxLength > 0)
            {
                result = CommonHelper.EnsureMaximumLength(result, maxLength);
            }

            return result;
        }

        /// <summary>
        /// Gets coupon codes
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>Coupon codes</returns>
        public static string[] ParseAppliedDiscountCouponCodes(this Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();
            
            var existingCouponCodes = genericAttributeService.GetAttributeAsync<string>(
               customer,
               SystemCustomerAttributeNames.DiscountCouponCode).Result;


            var couponCodes = new List<string>();
            if (string.IsNullOrEmpty(existingCouponCodes))
                return couponCodes.ToArray();

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(existingCouponCodes);

                var nodeList1 = xmlDoc.SelectNodes(@"//DiscountCouponCodes/CouponCode");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes != null && node1.Attributes["Code"] != null)
                    {
                        string code = node1.Attributes["Code"].InnerText.Trim();
                        couponCodes.Add(code);
                    }
                }
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }
            return couponCodes.ToArray();
        }

        /// <summary>
        /// Adds a coupon code
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="couponCode">Coupon code</param>
        /// <returns>New coupon codes document</returns>
        public static void ApplyDiscountCouponCode(this Customer customer, string couponCode)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();
            string result = string.Empty;
            try
            {
                
                var existingCouponCodes = genericAttributeService.GetAttributeAsync<string>(
                   customer,
                   SystemCustomerAttributeNames.DiscountCouponCode).Result;

                couponCode = couponCode.Trim().ToLower();

                var xmlDoc = new XmlDocument();
                if (String.IsNullOrEmpty(existingCouponCodes))
                {
                    var element1 = xmlDoc.CreateElement("DiscountCouponCodes");
                    xmlDoc.AppendChild(element1);
                }
                else
                {
                    xmlDoc.LoadXml(existingCouponCodes);
                }
                var rootElement = (XmlElement)xmlDoc.SelectSingleNode(@"//DiscountCouponCodes");

                XmlElement gcElement = null;
                //find existing
                var nodeList1 = xmlDoc.SelectNodes(@"//DiscountCouponCodes/CouponCode");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes != null && node1.Attributes["Code"] != null)
                    {
                        string couponCodeAttribute = node1.Attributes["Code"].InnerText.Trim();
                        if (couponCodeAttribute.ToLower() == couponCode.ToLower())
                        {
                            gcElement = (XmlElement)node1;
                            break;
                        }
                    }
                }

                //create new one if not found
                if (gcElement == null)
                {
                    gcElement = xmlDoc.CreateElement("CouponCode");
                    gcElement.SetAttribute("Code", couponCode);
                    rootElement.AppendChild(gcElement);
                }

                result = xmlDoc.OuterXml;
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }

            //apply new value
            genericAttributeService.SaveAttributeAsync(customer, SystemCustomerAttributeNames.DiscountCouponCode, result);
        }

        /// <summary>
        /// Removes a coupon code
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="couponCode">Coupon code to remove</param>
        /// <returns>New coupon codes document</returns>
        public static void RemoveDiscountCouponCode(this Customer customer, string couponCode)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            //get applied coupon codes
            var existingCouponCodes = customer.ParseAppliedDiscountCouponCodes();

            //clear them
            var genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();
            genericAttributeService.SaveAttributeAsync<string>(customer, SystemCustomerAttributeNames.DiscountCouponCode, null);

            //save again except removed one
            foreach (string existingCouponCode in existingCouponCodes)
                if (!existingCouponCode.Equals(couponCode, StringComparison.InvariantCultureIgnoreCase))
                    customer.ApplyDiscountCouponCode(existingCouponCode);
        }

        /// <summary>
        /// Gets coupon codes
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>Coupon codes</returns>
        public static string[] ParseAppliedGiftCardCouponCodes(this Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();
         
            var existingCouponCodes = genericAttributeService.GetAttributeAsync<string>(
                  customer,
                  SystemCustomerAttributeNames.GiftCardCouponCodes).Result;


            var couponCodes = new List<string>();
            if (String.IsNullOrEmpty(existingCouponCodes))
                return couponCodes.ToArray();

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(existingCouponCodes);

                var nodeList1 = xmlDoc.SelectNodes(@"//GiftCardCouponCodes/CouponCode");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes != null && node1.Attributes["Code"] != null)
                    {
                        string code = node1.Attributes["Code"].InnerText.Trim();
                        couponCodes.Add(code);
                    }
                }
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }
            return couponCodes.ToArray();
        }

        /// <summary>
        /// Adds a coupon code
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="couponCode">Coupon code</param>
        /// <returns>New coupon codes document</returns>
        public static void ApplyGiftCardCouponCode(this Customer customer, string couponCode)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();
            string result = string.Empty;
            try
            {               
                var existingCouponCodes = genericAttributeService.GetAttributeAsync<string>(
                  customer,
                  SystemCustomerAttributeNames.GiftCardCouponCodes).Result;

                couponCode = couponCode.Trim().ToLower();

                var xmlDoc = new XmlDocument();
                if (String.IsNullOrEmpty(existingCouponCodes))
                {
                    var element1 = xmlDoc.CreateElement("GiftCardCouponCodes");
                    xmlDoc.AppendChild(element1);
                }
                else
                {
                    xmlDoc.LoadXml(existingCouponCodes);
                }
                var rootElement = (XmlElement)xmlDoc.SelectSingleNode(@"//GiftCardCouponCodes");

                XmlElement gcElement = null;
                //find existing
                var nodeList1 = xmlDoc.SelectNodes(@"//GiftCardCouponCodes/CouponCode");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes != null && node1.Attributes["Code"] != null)
                    {
                        string couponCodeAttribute = node1.Attributes["Code"].InnerText.Trim();
                        if (couponCodeAttribute.ToLower() == couponCode.ToLower())
                        {
                            gcElement = (XmlElement)node1;
                            break;
                        }
                    }
                }

                //create new one if not found
                if (gcElement == null)
                {
                    gcElement = xmlDoc.CreateElement("CouponCode");
                    gcElement.SetAttribute("Code", couponCode);
                    rootElement.AppendChild(gcElement);
                }

                result = xmlDoc.OuterXml;
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }

            //apply new value
            genericAttributeService.SaveAttributeAsync(customer, SystemCustomerAttributeNames.GiftCardCouponCodes, result);
        }

        /// <summary>
        /// Removes a coupon code
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="couponCode">Coupon code to remove</param>
        /// <returns>New coupon codes document</returns>
        public static void RemoveGiftCardCouponCode(this Customer customer, string couponCode)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            //get applied coupon codes
            var existingCouponCodes = customer.ParseAppliedGiftCardCouponCodes();

            //clear them
            var genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();
            genericAttributeService.SaveAttributeAsync<string>(customer, SystemCustomerAttributeNames.GiftCardCouponCodes, null);

            //save again except removed one
            foreach (string existingCouponCode in existingCouponCodes)
                if (!existingCouponCode.Equals(couponCode, StringComparison.InvariantCultureIgnoreCase))
                    customer.ApplyGiftCardCouponCode(existingCouponCode);
        }

        /// <summary>
        /// Check whether password recovery token is valid
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="token">Token to validate</param>
        /// <returns>Result</returns>
        public static bool IsPasswordRecoveryTokenValid(this Customer customer, string token)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();            

            var cPrt = genericAttributeService.GetAttributeAsync<string>(
                 customer,
                 SystemCustomerAttributeNames.PasswordRecoveryToken).Result;

            if (String.IsNullOrEmpty(cPrt))
                return false;

            if (!cPrt.Equals(token, StringComparison.InvariantCultureIgnoreCase))
                return false;

            return true;
        }

        /// <summary>
        /// Check whether password recovery link is expired
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="customerSettings">Customer settings</param>
        /// <returns>Result</returns>
        public static bool IsPasswordRecoveryLinkExpired(this Customer customer, CustomerSettings customerSettings)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (customerSettings == null)
                throw new ArgumentNullException(nameof(customerSettings));

            if (customerSettings.PasswordRecoveryLinkDaysValid == 0)
                return false;

            var genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();
            var geneatedDate = genericAttributeService.GetAttributeAsync<DateTime?>(
                 customer,
                 SystemCustomerAttributeNames.PasswordRecoveryTokenDateGenerated).Result;

            if (!geneatedDate.HasValue)
                return false;

            var daysPassed = (DateTime.UtcNow - geneatedDate.Value).TotalDays;
            if (daysPassed > customerSettings.PasswordRecoveryLinkDaysValid)
                return true;

            return false;
        }

        /// <summary>
        /// Get customer role identifiers
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="showHidden">A value indicating whether to load hidden records</param>
        /// <returns>Customer role identifiers</returns>
        public static int[] GetCustomerRoleIds(this Customer customer, bool showHidden = false)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var customerRolesIds = customer.CustomerRoles
               .Where(cr => showHidden || cr.Active)
               .Select(cr => cr.Id)
               .ToArray();

            return customerRolesIds;
        }

        /// <summary>
        /// Check whether customer password is expired 
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>True if password is expired; otherwise false</returns>
        public static bool PasswordIsExpired(this Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            //the guests don't have a password
            var customerService = EngineContext.Current.Resolve<ICustomerService>();
            if (customerService.IsGuestAsync(customer).Result)
            {
                return false;
            }

            //password lifetime is disabled for user
            if (!customer.CustomerRoles.Any(role => role.Active && role.EnablePasswordLifetime))
                return false;

            //setting disabled for all
            var customerSettings = EngineContext.Current.Resolve<CustomerSettings>();
            if (customerSettings.PasswordLifetime == 0)
                return false;

            var staticCacheManager = EngineContext.Current.Resolve<IStaticCacheManager>();
            
            var cacheKey = staticCacheManager.PrepareKeyForDefaultCache(NopCustomerServicesDefaults.CustomerPasswordLifetimeCacheKey, customer);

            //get current password usage time
            var currentLifetime = staticCacheManager.Get(cacheKey, () =>
            {
                var customerPassword = customerService.GetCurrentPasswordAsync(customer.Id).Result;
                //password is not found, so return max value to force customer to change password
                if (customerPassword == null)
                    return int.MaxValue;

                return (DateTime.UtcNow - customerPassword.CreatedOnUtc).Days;
            });

            return currentLifetime >= customerSettings.PasswordLifetime;
        }
    }
}