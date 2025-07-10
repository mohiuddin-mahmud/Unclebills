using Nop.Core.Domain.Customers;
using Nop.Data;

namespace Nop.Services.Customers
{
    public partial class CustomerInfoChangeService : ICustomerInfoChangeService
    {
        #region Fields

        private readonly IRepository<CustomerInfoChange> _customerInfoChangeRepository;
        private readonly IRepository<CustomerInfoChangeData> _customerInfoChangeDataRepository;
       

        #endregion

        #region Ctor
        public CustomerInfoChangeService(IRepository<CustomerInfoChange> customerInfoChangeRepository,
            IRepository<CustomerInfoChangeData> customerInfoChangeDataRepository)
        {
            this._customerInfoChangeRepository = customerInfoChangeRepository;
            this._customerInfoChangeDataRepository = customerInfoChangeDataRepository;
        }

        #endregion

        #region CustomerInfoChange

        /// <summary>
        /// Inserts a CustomerInfoChange
        /// </summary>
        /// <param name="infoChange">Customer Info Change Master Record</param>
        public virtual async Task InsertCustomerInfoChange(CustomerInfoChange infoChange)
        {
            if (infoChange == null)
                throw new ArgumentNullException("infoChange");

            await _customerInfoChangeRepository.InsertAsync(infoChange);
        }

        /// <summary>
        /// Updates the CustomerInfoChange
        /// </summary>
        /// <param name="infoChange">Customer Info Change Master Record</param>
        public virtual async Task UpdateCustomerInfoChange(CustomerInfoChange infoChange)
        {
            if (infoChange == null)
                throw new ArgumentNullException("infoChange");

            await _customerInfoChangeRepository.UpdateAsync(infoChange);
        }

        /// <summary>
        /// Deletes a CustomerInfoChange
        /// </summary>
        /// <param name="infoChange">Customer Info Change Master Record</param>
        public virtual async Task DeleteCustomerInfoChange(CustomerInfoChange infoChange)
        {
            if (infoChange == null)
                throw new ArgumentNullException("infoChange");

            await _customerInfoChangeRepository.DeleteAsync(infoChange);
        }

        public virtual async Task<IList<CustomerInfoChange>> GetCustomerInfoChanges(DateTime changeDate)
        {
            var query = _customerInfoChangeRepository.Table;
            return await query.Where(o => o.ChangedOn.Date == changeDate.Date).ToListAsync();
        }

        #endregion

        #region CustomerInfoChangeData

        /// <summary>
        /// Inserts a CustomerInfoChangeData
        /// </summary>
        /// <param name="infoChangeData">Customer Info Change Data</param>
        public virtual async Task InsertCustomerInfoChangeData(CustomerInfoChangeData infoChangeData)
        {
            if (infoChangeData == null)
                throw new ArgumentNullException("infoChangeData");

            await _customerInfoChangeDataRepository.InsertAsync(infoChangeData);

        }

        /// <summary>
        /// Updates the CustomerInfoChangeData
        /// </summary>
        /// <param name="infoChangeData">Customer Info Change Data</param>
        public virtual async Task UpdateCustomerInfoChangeData(CustomerInfoChangeData infoChangeData)
        {
            if (infoChangeData == null)
                throw new ArgumentNullException("infoChangeData");

           await _customerInfoChangeDataRepository.UpdateAsync(infoChangeData);
        }

        /// <summary>
        /// Deletes a CustomerInfoChangeData
        /// </summary>
        /// <param name="infoChange">Customer Info Change Data</param>
        public virtual async Task DeleteCustomerInfoChangeData(CustomerInfoChangeData infoChangeData)
        {
            if (infoChangeData == null)
                throw new ArgumentNullException("infoChangeData");

           await _customerInfoChangeDataRepository.DeleteAsync(infoChangeData);
        }

        public virtual async Task<IList<CustomerInfoChangeData>> GetCustomerInfoChangeDatas(int customerChangeInfoId)
        {
            var query = _customerInfoChangeDataRepository.Table;
            return await query.Where(x => x.CustomerInfoChangeId == customerChangeInfoId).ToListAsync();
        }

        #endregion
    }
}
