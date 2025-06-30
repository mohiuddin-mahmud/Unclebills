using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Stores;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Media;

namespace NopValley.Plugins.Misc.Models
{
    /// <summary>
    /// Represents a tax rate
    /// </summary>
    public partial record HtmlWidgetPublicModel : BaseNopEntityModel
    {
        public HtmlWidgetPublicModel() {
            PictureModel = new PictureModel();
        }

        public string Title { get; set; }
    
        public string Description { get; set; }
        public string Link { get; set; }

        public int WidgetZoneId { get; set; }
        public bool ShowForGuest { get; set; }

        public int TemplateType { get; set; }
        public int PictureId { get; set; }

        public PictureModel PictureModel { get; set; }

    }
}