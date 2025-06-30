using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;
using NopValley.Plugins.Misc.Domain;

namespace NopValley.Plugins.Misc.Areas.Admin.Models
{
    /// <summary>
    /// Represents a service category model
    /// </summary>
    public partial record ServicePostCategoryModel : BaseNopEntityModel
    {
        #region Ctor

        public ServicePostCategoryModel()
        {
          
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("NopValley.Plugin.Misc.ServicePost.CategoryName")]
        public string Name { get; set; }

        public string SeName { get; set; }
        #endregion
    }
}