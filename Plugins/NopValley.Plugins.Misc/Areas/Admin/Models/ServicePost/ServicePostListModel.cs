using Nop.Web.Framework.Models;

namespace NopValley.Plugins.Misc.Areas.Admin.Models.ServicePost;

/// <summary>
/// Represents a blog post list model
/// </summary>
public partial record ServicePostListModel : BasePagedListModel<ServicePostModel>
{
}