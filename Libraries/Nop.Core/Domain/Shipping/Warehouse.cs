namespace Nop.Core.Domain.Shipping;

/// <summary>
/// Represents a shipment
/// </summary>
public partial class Warehouse : BaseEntity
{
    /// <summary>
    /// Gets or sets the warehouse name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the admin comment
    /// </summary>
    public string AdminComment { get; set; }

    /// <summary>
    /// Gets or sets the address identifier of the warehouse
    /// </summary>
    public int AddressId { get; set; }


    /// <summary>
    /// Gets or sets the Uncle Bill's Pet Center Store Identifier
    /// </summary>
    public int UbpcStoreId { get; set; }

    /// <summary>
    /// Gets or sets the Location Url - for linking on Product Detail page
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Gets or sets the Email address for this location
    /// </summary>
    public string Email { get; set; }


    /// <summary>
    /// Stores Longitude
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// Stores Longitude
    /// </summary>
    public double? Longitude { get; set; }
}