using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;

namespace NopValley.Plugins.Misc.Models
{
    public partial record PublicServicePostListModel : BaseNopModel
    {
        public PublicServicePostListModel()
        {
            ServicePosts = new List<PublicServicePostModel>();
            Categories = new List<SelectListItem>();
        }
        public IList<PublicServicePostModel> ServicePosts { get; set; }

        public IList<SelectListItem> Categories { get; set; }
    }
}