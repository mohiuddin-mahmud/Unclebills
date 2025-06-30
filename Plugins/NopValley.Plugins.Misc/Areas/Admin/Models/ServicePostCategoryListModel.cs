using Nop.Web.Framework.Models;
using NopValley.Plugins.Misc.Areas.Admin.Models;

namespace NopValley.Plugins.Misc.Areas.Admin.Models
{
    /// <summary>
    /// Represents a blogcategory list model
    /// </summary>
    public partial record ServicePostCategoryListModel : BasePagedListModel<ServicePostCategoryModel>
    {
    }
}