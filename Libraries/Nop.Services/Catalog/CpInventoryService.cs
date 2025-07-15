
using Nop.Core.Domain.Catalog;
using Nop.Core.Events;
using Nop.Data;

namespace Nop.Services.Catalog;

public partial class CpInventoryService : ICpInventoryService
{
    #region Fields

    private readonly IRepository<CpInventory> _cpInventoryRepository;
    private readonly IEventPublisher _eventPublisher;

    #endregion

    #region Ctor

    public CpInventoryService(IRepository<CpInventory> cpInventoryRepository,
        IEventPublisher eventPublisher)
    {
        this._cpInventoryRepository = cpInventoryRepository;
        this._eventPublisher = eventPublisher;
    }

    #endregion

    #region Methods

    public virtual void InsertCpInventory(CpInventory inventory)
    {
        if (inventory == null)
            throw new ArgumentNullException("inventory");

        _cpInventoryRepository.Insert(inventory);

        _eventPublisher.EntityInserted(inventory);
    }

    public virtual void UpdateCpInventory(CpInventory inventory)
    {
        if (inventory == null)
            throw new ArgumentNullException("inventory");

        _cpInventoryRepository.Update(inventory);

        //event notification
        _eventPublisher.EntityUpdated(inventory);
    }

    public virtual void DeleteCpInventory(CpInventory inventory)
    {
        if (inventory == null)
            throw new ArgumentNullException("inventory");

        _cpInventoryRepository.Delete(inventory);

        _eventPublisher.EntityDeleted(inventory);
    }

    public virtual List<CpInventory> GetProductAvailability(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentNullException("sku");

        // return only records for stores with min/max > 0 and onhand qty > 0
        var query = _cpInventoryRepository.Table;
        return query.Where(o => o.Sku == sku && o.MinQty > 0 && o.MaxQty > 0 && o.OnHand > 0).ToList();
    }

    public virtual CpInventory GetProductAvailability(string sku, string storeId)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentNullException("sku");

        if (string.IsNullOrWhiteSpace(storeId))
            throw new ArgumentNullException("storeId");

        var query = _cpInventoryRepository.Table;
        return query.Where(o => o.Sku == sku && o.StoreId == storeId).FirstOrDefault();
    }

    #endregion
}
