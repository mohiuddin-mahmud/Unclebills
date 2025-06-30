
using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Stores;

namespace NopValley.Plugins.Misc.Domain
{
    /// <summary>
    /// Represents a tax rate
    /// </summary>
    public partial class BlogPostAdditional : BaseEntity
	{            
        public int BlogPostId { get; set; }
        public int PictureId { get; set; }
        //public bool IsTopBlogPost { get; set; }

       // public int BlogCategoryId { get; set; }

    }
}