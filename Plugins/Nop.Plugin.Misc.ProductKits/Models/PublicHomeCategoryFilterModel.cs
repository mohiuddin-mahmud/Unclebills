using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Models.Media;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NopValley.Plugins.Misc.Models
{
    public partial record PublicHomeCategoryFilterModel : BaseNopEntityModel
    {        
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string SeName { get; set; }
    }

}
