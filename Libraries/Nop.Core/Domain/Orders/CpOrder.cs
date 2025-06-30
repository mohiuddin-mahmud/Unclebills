namespace Nop.Core.Domain.Orders;
public partial class CpOrder : BaseEntity
{
    #region Properties

    public string OrderId { get; set; }
    public string CustomerId { get; set; }
    public string StoreId { get; set; }
    public DateTime PurchaseDate { get; set; }
    public double OrderTotal { get; set; }

    #endregion
}

public partial class CpOrderLine : BaseEntity
{
    #region Properties

    public string OrderId { get; set; }
    public string LineNumber { get; set; }
    public string ProductId { get; set; }
    public string ProductDescription { get; set; }
    public string Quantity { get; set; }

    #endregion
}