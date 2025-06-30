using System.ComponentModel.DataAnnotations;

namespace NopValley.Plugins.Misc.Areas.Admin.Models
{
    public record AdminServiceEditModel
    {
        public AdminServiceEditModel()
        {          
        }

        public int ServiceId { get; set; }

        [UIHint("Picture")]
        //[NopResourceDisplayName()]
        public int PictureId { get; set; }

        //[NopResourceDisplayName(ResourceNames.Blog.Fields.ISTOPBLOGPOST)]
        public bool IsTopService { get; set; }
     
    }
}