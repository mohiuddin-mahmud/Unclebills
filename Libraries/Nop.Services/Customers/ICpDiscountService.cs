using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;

namespace Nop.Services.Customers
{
    public partial interface ICpDiscountService
    {
        #region CpDiscount

        /// <summary>
        /// Inserts a discount
        /// </summary>
        /// <param name="discount">discount</param>
        Task InsertCpDiscount(CpDiscount discount);

        /// <summary>
        /// Updates the discount
        /// </summary>
        /// <param name="discount">The discount</param>
        Task UpdateCpDiscount(CpDiscount discount);

        /// <summary>
        /// Deletes a discount
        /// </summary>
        /// <param name="discount">The discount</param>
        Task DeleteCpDiscount(CpDiscount discount);

        /// <summary>
        /// Gets specific discount associated with customer
        /// </summary>
        /// <param name="customerId">The CounterPoint customer Id</param>
        /// <param name="loyaltyId">The CounterPoint loyalty code</param>
        /// <returns>Order</returns>
        Task<CpDiscount> GetCpDiscount(string customerId, string loyaltyId);

        /// <summary>
        /// Gets all Counter Point discounts associated with customer
        /// </summary>
        /// <param name="customerId">The CounterPoint customer Id</param>
        /// <returns>Order</returns>
        Task<IList<CpDiscount>> GetCpDiscounts(string customerId);

        #endregion

        #region CpDiscountProduct

        /// <summary>
        /// Inserts a discount product
        /// </summary>
        /// <param name="discountProduct">The discount product</param>
        Task InsertCpDiscountProduct(CpDiscountProduct discountProduct);

        /// <summary>
        /// Updates the discount product
        /// </summary>
        /// <param name="discountProduct">The discount product</param>
        Task UpdateCpDiscountProduct(CpDiscountProduct discountProduct);

        /// <summary>
        /// Deletes a discount
        /// </summary>
        /// <param name="discountProduct">The discount product</param>
        Task DeleteCpDiscountProduct(CpDiscountProduct discountProduct);

        /// <summary>
        /// Gets program record for loyalty code and product sku combination
        /// </summary>
        /// <param name="loyaltyCode">The CounterPoint loyalty code</param>
        /// <param name="productSku">The CounterPoint product sku</param>
        /// <returns>list of all product skus in loyaltyCode group</returns>
        Task<CpDiscountProduct> GetCpDiscountProduct(string loyaltyCode, string productSku);

        /// <summary>
        /// Gets products associated with loyalty code
        /// </summary>
        /// <param name="loyaltyCode">The CounterPoint loyalty code</param>
        /// <returns>list of all product skus in loyaltyCode group</returns>
        Task<IList<CpDiscountProduct>> GetCpDiscountProducts(string loyaltyCode);

        /// <summary>
        /// Gets products associated with loyalty code
        /// </summary>
        /// <param name="sku">The CounterPoint Product Sku</param>
        /// <returns>list of all product skus in loyaltyCode group</returns>
        Task<CpDiscountProduct> GetCpDiscountProduct(string sku);
        #endregion
    }
}
