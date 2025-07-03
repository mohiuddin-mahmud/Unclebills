namespace Nop.Core.Domain.Customers;

/// <summary>
/// Represents a reward point history entry
/// </summary>
public partial class RewardsCardLookupFlat : BaseEntity
{
    /// <summary>
    /// Gets or sets the customer identifier
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the store identifier in which these reward points were awarded or redeemed
    /// </summary>
    public string RewardsCard { get; set; }
    
}