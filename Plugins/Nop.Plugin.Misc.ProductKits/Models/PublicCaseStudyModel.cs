using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using NopValley.Plugins.Misc.Areas.Admin.Models;
using System.Collections.Generic;

namespace NopValley.Plugins.Misc.Models
{
    public partial record PublicHomeTabModel : BaseNopEntityModel
    {
        public PublicHomeTabModel()
        {
            HomeTabCategoryModel = new List<PublicHomeTabCategoryModel>();
            
        }
        public IList<PublicHomeTabCategoryModel> HomeTabCategoryModel { get; set; }
       
    }


    public partial record PublicHomeTabCategoryModel : BaseNopEntityModel
    {
        public PublicHomeTabCategoryModel()
        {
            ProductOverviewModel = new List<ProductOverviewModel>();
        }
        public string Name { get; set; }

        public IList<ProductOverviewModel> ProductOverviewModel { get; set; }
    }
}