namespace Nop.Core.Domain.Catalog;

public partial class CpInventory : BaseEntity
{
    public string Sku { get; set; }
    public string StoreId { get; set; }
    public decimal MinQty { get; set; }
    public decimal MaxQty { get; set; }
    public decimal OnHand { get; set; }
}
