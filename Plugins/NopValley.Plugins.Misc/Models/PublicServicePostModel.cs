using Nop.Web.Framework.Models;
using Nop.Web.Models.Media;
using System.Collections.Generic;

namespace NopValley.Plugins.Misc.Models
{
    public partial record PublicServicePostModel : BaseNopEntityModel
    {
        public PublicServicePostModel()
        {
            PictureModel = new PictureModel();
            DefaultPictureModel = new PictureModel();
            PictureModels = new List<PictureModel>();
        }        
        
        public string Name { get; set; }

        public string Body { get; set; }
        public string SeName { get; set; }
        public int CategoryId { get; set; }
        public int PictureId { get; set; }

        public PictureModel PictureModel { get; set; }

        public PictureModel DefaultPictureModel { get; set; }
        public IList<PictureModel> PictureModels { get; set; }


    }
}