using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Topics;
using Nop.Web.Areas.Admin.Models.Topics;
using NopValley.Plugins.Misc.Areas.Admin.Models;
using NopValley.Plugins.Misc.Domain;

namespace NopValley.Plugins.Misc.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the topic model factory
    /// </summary>
    public partial interface IReferenceCategoryModelFactory
    {
        
        /// </returns>
        Task<ReferenceCategorModel> PrepareReferenceCategoryModelAsync(ReferenceCategorModel model, RefCategory refCategory);

        Task<ReferenceCategorySearchModel> PrepareReferenceCategorySearchModelAsync(ReferenceCategorySearchModel searchModel);

        Task<ReferenceCategoryListModel> PrepareReferenceCategoryListModelAsync(ReferenceCategorySearchModel searchModel);
    }
}