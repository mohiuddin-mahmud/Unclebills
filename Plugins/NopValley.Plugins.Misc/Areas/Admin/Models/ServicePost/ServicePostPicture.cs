﻿namespace Nop.Core.Domain.Catalog;

/// <summary>
/// Represents a product picture mapping
/// </summary>
public partial class ServicePostPicture : BaseEntity
{
    /// <summary>
    /// Gets or sets the product identifier
    /// </summary>
    public int ServicePostId { get; set; }

    /// <summary>
    /// Gets or sets the picture identifier
    /// </summary>
    public int PictureId { get; set; }

    /// <summary>
    /// Gets or sets the display order
    /// </summary>
    public int DisplayOrder { get; set; }
}