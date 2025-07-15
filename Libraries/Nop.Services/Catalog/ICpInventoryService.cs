using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;

namespace Nop.Services.Catalog
{
    public partial interface ICpInventoryService
    {
        /// <summary>
        /// Inserts a inventory
        /// </summary>
        /// <param name="inventory">inventory</param>
        void InsertCpInventory(CpInventory inventory);

        /// <summary>
        /// Updates the inventory
        /// </summary>
        /// <param name="inventory">The inventory</param>
        void UpdateCpInventory(CpInventory inventory);

        /// <summary>
        /// Deletes a inventory
        /// </summary>
        /// <param name="inventory">The inventory</param>
        void DeleteCpInventory(CpInventory inventory);

        /// <summary>
        /// Gets specific inventory associated with sku
        /// </summary>
        /// <param name="sku">The product sku</param>
        /// <returns>Order</returns>
        List<CpInventory> GetProductAvailability(string sku);

        /// <summary>
        /// Product inventory at this location
        /// </summary>
        /// <param name="sku">The product sku</param>
        /// <param name="storeId">The CounterPoint store ID</param>
        /// <returns>Order</returns>
        CpInventory GetProductAvailability(string sku, string storeId);
    }
}
