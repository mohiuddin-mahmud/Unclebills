using Nop.Web.Framework.Models;

namespace NopValley.Plugins.Misc.Areas.Admin.Models.ServicePost;

/// <summary>
/// Represents a product picture search model
/// </summary>
public partial record ServicePostPictureSearchModel : BaseSearchModel
{
    #region Properties

    public int ServicePostId { get; set; }

    #endregion
}