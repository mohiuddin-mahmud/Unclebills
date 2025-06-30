using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopValley.Plugins.Misc.Areas.Admin.Models
{
    /// <summary>
    /// Represents a category search model
    /// </summary>
    public partial record ServicePostCategorySearchModel : BaseSearchModel
    {
        #region Ctor

        public ServicePostCategorySearchModel()
        {
           
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.Catalog.Categories.List.SearchCategoryName")]
        public string SearchServicePostCategoryName { get; set; }
        

        #endregion
    }
}