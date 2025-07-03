using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;


namespace Nop.Plugin.Misc.ProductKits.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public ConfigurationModel()
        {
            CategoryId = 0;
            AvailableCategories = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.Categories")]
        public int CategoryId { get; set; }
        public IList<SelectListItem> AvailableCategories { get; set; }
    }
}
