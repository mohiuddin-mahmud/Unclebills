using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopValley.Plugins.Misc.Areas.Admin.Models.ServicePost;

/// <summary>
/// Represents a ServicePost picture model
/// </summary>
public partial record ServicePostPictureModel : BaseNopEntityModel
{
    #region Properties

    public int ProductId { get; set; }

    [UIHint("MultiPicture")]
    [NopResourceDisplayName("Admin.Catalog.Products.Multimedia.Pictures.Fields.Picture")]
    public int PictureId { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.Multimedia.Pictures.Fields.Picture")]
    public string PictureUrl { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.Multimedia.Pictures.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.Multimedia.Pictures.Fields.OverrideAltAttribute")]
    public string OverrideAltAttribute { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.Multimedia.Pictures.Fields.OverrideTitleAttribute")]
    public string OverrideTitleAttribute { get; set; }

    #endregion
}