using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Seo;

namespace NopValley.Plugins.Misc.Domain
{
    /// <summary>
    /// Represents a tax rate
    /// </summary>
    public partial class ServicePost : BaseEntity, ILocalizedEntity, ISlugSupported
    {            
  
        public int PictureId { get; set; }
        public bool IsTopService { get; set; }

        public string Name { get; set; }
        public string Body { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public int ServicePostCategoryId { get; set; }
        public int LanguageId { get; set; }


    }
}