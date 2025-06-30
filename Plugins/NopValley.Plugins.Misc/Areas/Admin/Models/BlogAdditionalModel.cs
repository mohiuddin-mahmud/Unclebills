using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Models.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NopValley.Plugins.Misc.Areas.Admin.Models
{
    public partial record BlogAdditionalModel : BaseNopEntityModel
    {
        public BlogAdditionalModel() {
            PictureModel = new PictureModel();
        }
        public int BlogPostId { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.Picture")]
        public int PictureId { get; set; }

        public PictureModel PictureModel { get; set; }

        public string Title { get; set; }
        public string Body { get; set; }
        public string SeName { get; set; }

        public DateTime CreatedOn { get; set; }

    }

}
