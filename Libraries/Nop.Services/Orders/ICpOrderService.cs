using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;

namespace Nop.Services.Orders
{
    public partial interface ICpOrderService
    {
        #region Orders

        /// <summary>
        /// Inserts an order
        /// </summary>
        /// <param name="order">Order</param>
        Task InsertCpOrder(CpOrder order);

        /// <summary>
        /// Updates the order
        /// </summary>
        /// <param name="order">The order</param>
        Task UpdateCpOrder(CpOrder order);

        /// <summary>
        /// Deletes an order
        /// </summary>
        /// <param name="order">The order</param>
        Task DeleteCpOrder(CpOrder order);

        /// <summary>
        /// Gets an order
        /// </summary>
        /// <param name="orderId">The CounterPoint order Id</param>
        /// <returns>Order</returns>
        Task<CpOrder> GetCpOrderByOrderId(string orderId);

        /// <summary>
        /// Gets all Counter Point orders associated with customer
        /// </summary>
        /// <param name="customerId">The CounterPoint customer Id</param>
        /// <returns>Order</returns>
        Task<IList<CpOrder>> GetCpOrdersByCustomerId(string customerId);

        #endregion

        #region Order Lines

        /// <summary>
        /// Inserts an order line
        /// </summary>
        /// <param name="order">Order Line</param>
        Task InsertCpOrderLine(CpOrderLine orderLine);

        /// <summary>
        /// Updates the order line
        /// </summary>
        /// <param name="order">The order line</param>
        Task UpdateCpOrderLine(CpOrderLine orderLine);

        /// <summary>
        /// Deletes an order line
        /// </summary>
        /// <param name="order">The order line</param>
        Task DeleteCpOrderLine(CpOrderLine orderLine);

        /// <summary>
        /// Gets specific order line for given order
        /// </summary>
        /// <param name="orderId">The CounterPoint order Id</param>
        /// <param name="lineNumber">The CounterPoint order line number</param>
        /// <returns>Order</returns>
        Task<CpOrderLine> GetCpOrderLine(string orderId, string lineNumber);

        /// <summary>
        /// Gets all order lines for given order
        /// </summary>
        /// <param name="orderId">The CounterPoint order Id</param>
        /// <returns>Order</returns>
        Task<IList<CpOrderLine>> GetCpOrderLines(string orderId);

        #endregion
    }
}
