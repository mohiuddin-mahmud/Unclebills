using Nop.Core.Domain.Customers;
using Nop.Data;

namespace Nop.Services.Customers
{
    public partial class CpDiscountService : ICpDiscountService
    {
        #region Fields

        private readonly IRepository<CpDiscount> _cpDiscountRepository;
        private readonly IRepository<CpDiscountProduct> _cpDiscountProductRepository;


        #endregion

        #region Ctor
        public CpDiscountService(IRepository<CpDiscount> cpDiscountRepository, 
            IRepository<CpDiscountProduct> cpDiscountProductRepository
      )
        {
            _cpDiscountRepository = cpDiscountRepository;
            _cpDiscountProductRepository = cpDiscountProductRepository;
       
        }

        #endregion

        #region CpDiscount

        public virtual async Task InsertCpDiscount(CpDiscount discount)
        {
            if (discount == null)
                throw new ArgumentNullException("discount");

            await _cpDiscountRepository.InsertAsync(discount);
   
        }

        public virtual async Task UpdateCpDiscount(CpDiscount discount)
        {
            if (discount == null)
                throw new ArgumentNullException("discount");

            await _cpDiscountRepository.UpdateAsync(discount);
        }

        public virtual async Task DeleteCpDiscount(CpDiscount discount)
        {
            if (discount == null)
                throw new ArgumentNullException("discount");

            await _cpDiscountRepository.DeleteAsync(discount);
        }

        public virtual async Task<CpDiscount> GetCpDiscount(string customerId, string loyaltyId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                throw new ArgumentNullException("customerId");

            if (string.IsNullOrWhiteSpace(loyaltyId))
                throw new ArgumentNullException("loyaltyId");

            var query = _cpDiscountRepository.Table;
            await Task.Yield();
            return query.Where(o => o.CustomerId == customerId && o.LoyaltyCode == loyaltyId).FirstOrDefault();
        }

        public virtual async Task<IList<CpDiscount>> GetCpDiscounts(string customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                throw new ArgumentNullException("customerId");

            var query = _cpDiscountRepository.Table;
            await Task.Yield();

            return query.Where(o => o.CustomerId == customerId).ToList();
        }

        #endregion

        #region CpDiscountProduct

        public virtual async Task InsertCpDiscountProduct(CpDiscountProduct discountProduct)
        {
            if (discountProduct == null)
                throw new ArgumentNullException("discountProduct");

            await _cpDiscountProductRepository.InsertAsync(discountProduct);

        }

        public virtual async Task UpdateCpDiscountProduct(CpDiscountProduct discountProduct)
        {
            if (discountProduct == null)
                throw new ArgumentNullException("discountProduct");

            await _cpDiscountProductRepository.UpdateAsync(discountProduct);
        }

        public virtual async Task DeleteCpDiscountProduct(CpDiscountProduct discountProduct)
        {
            if (discountProduct == null)
                throw new ArgumentNullException("discountProduct");

            await _cpDiscountProductRepository.DeleteAsync(discountProduct);           
        }

        public virtual async Task<CpDiscountProduct> GetCpDiscountProduct(string loyaltyCode, string productSku)
        {
            if (string.IsNullOrWhiteSpace(loyaltyCode) || string.IsNullOrWhiteSpace(productSku))
                return null;

            var query = _cpDiscountProductRepository.Table;
            await Task.Yield();
            return query.FirstOrDefault(o => o.LoyaltyCode == loyaltyCode && o.ProductSku == productSku);
        }

        public virtual async Task<IList<CpDiscountProduct>> GetCpDiscountProducts(string loyaltyCode)
        {
            if (string.IsNullOrWhiteSpace(loyaltyCode))
                throw new ArgumentNullException("loyaltyCode");

            var query = _cpDiscountProductRepository.Table;
            await Task.Yield();
            return query.Where(o => o.LoyaltyCode == loyaltyCode).ToList();
        }

        public virtual async Task<CpDiscountProduct> GetCpDiscountProduct(string sku)
        {
            if (string.IsNullOrEmpty(sku))
                throw new ArgumentNullException("sku");

            var query = _cpDiscountProductRepository.Table;

            await Task.Yield();
            return query.FirstOrDefault(c => c.ProductSku.Equals(sku));
        }
        #endregion

    }
}
