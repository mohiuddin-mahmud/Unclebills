namespace Nop.Core.Domain.Customers;
public partial class CpDiscount : BaseEntity
{
    public string CustomerId { get; set; }
    public string LoyaltyCode { get; set; }
    public string LoyaltyName { get; set; }
    public string PurchaseGoal { get; set; }
    public string PurchaseStatus { get; set; }
}

public partial class CpDiscountProduct : BaseEntity
{
    public string LoyaltyCode { get; set; }
    public string ProductSku { get; set; }
}