using System;
using System.Collections.Generic;
using Nop.Core.Domain.Customers;

namespace Nop.Services.Customers
{
    public partial interface ICustomerInfoChangeService
    {
        #region CustomerInfoChange

        /// <summary>
        /// Inserts a CustomerInfoChange
        /// </summary>
        /// <param name="infoChange">Customer Info Change Master Record</param>
        Task InsertCustomerInfoChange(CustomerInfoChange infoChange);

        /// <summary>
        /// Updates the CustomerInfoChange
        /// </summary>
        /// <param name="infoChange">Customer Info Change Master Record</param>
        Task UpdateCustomerInfoChange(CustomerInfoChange infoChange);

        /// <summary>
        /// Deletes a CustomerInfoChange
        /// </summary>
        /// <param name="infoChange">Customer Info Change Master Record</param>
        Task DeleteCustomerInfoChange(CustomerInfoChange infoChange);

        Task<IList<CustomerInfoChange>> GetCustomerInfoChanges(DateTime changeDate);

        #endregion

        #region CustomerInfoChangeData

        /// <summary>
        /// Inserts a CustomerInfoChangeData
        /// </summary>
        /// <param name="infoChangeData">Customer Info Change Data</param>
        Task InsertCustomerInfoChangeData(CustomerInfoChangeData infoChangeData);

        /// <summary>
        /// Updates the CustomerInfoChangeData
        /// </summary>
        /// <param name="infoChangeData">Customer Info Change Data</param>
        Task UpdateCustomerInfoChangeData(CustomerInfoChangeData infoChangeData);

        /// <summary>
        /// Deletes a CustomerInfoChangeData
        /// </summary>
        /// <param name="infoChange">Customer Info Change Data</param>
        Task DeleteCustomerInfoChangeData(CustomerInfoChangeData infoChangeData);

        Task<IList<CustomerInfoChangeData>> GetCustomerInfoChangeDatas(int customerChangeInfoId);

        #endregion
    }
}
