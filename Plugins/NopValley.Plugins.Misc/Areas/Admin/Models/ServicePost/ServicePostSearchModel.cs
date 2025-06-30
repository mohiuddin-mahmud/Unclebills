using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace NopValley.Plugins.Misc.Areas.Admin.Models.ServicePost;

/// <summary>
/// Represents a blog post search model
/// </summary>
public partial record ServicePostSearchModel : BaseSearchModel
{
    #region Ctor

    public ServicePostSearchModel()
    {
       
    }

    #endregion

    #region Properties
    public string SearchTitle { get; set; }

    #endregion
}