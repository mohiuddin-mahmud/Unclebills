using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;

namespace Nop.Services.Catalog
{
    public partial interface ICpProductPriceService
    {
        /// <summary>
        /// Inserts a productPrice
        /// </summary>
        /// <param name="productPrice">productPrice</param>
        void InsertCpProductPrice(CpProductPrice productPrice);

        /// <summary>
        /// Updates the productPrice
        /// </summary>
        /// <param name="productPrice">The productPrice</param>
        void UpdateCpProductPrice(CpProductPrice productPrice);

        /// <summary>
        /// Deletes a productPrice
        /// </summary>
        /// <param name="productPrice">The productPrice</param>
        void DeleteCpProductPrice(CpProductPrice productPrice);

        /// <summary>
        /// Gets specific productPrice associated with customer
        /// </summary>
        /// <param name="sku">The CounterPoint customer Id</param>
        /// <returns>CpProductPrice</returns>
        CpProductPrice GetProductPrice(string sku);
    }
}
