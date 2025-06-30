using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopValley.Plugins.Misc.Domain;
namespace NopValley.Plugins.Misc.Data.Mapping
{
    /// <summary>
    /// Base instance of backward compatibility of table naming
    /// </summary>
    public partial class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(HtmlWidget), "NopValley_Misc_HtmlWidget" },
            //{ typeof(Reference), "4a_Misc_reference" },
            { typeof(Section), "NopValley_Misc_section" },
            //{ typeof(GalleryPicture), "n4a_Misc_gallerypicture" },
            //{ typeof(GalleryVideo), "n4a_Misc_galleryvideo" },
            //{ typeof(RefCategory), "n4a_Misc_refcategory" },
            //{ typeof(ReferenceCategoryMapping), "n4a_Misc_referencecategorymapping" },
            //{ typeof(ReferenceConfiguration), "n4a_Misc_referenceconfiguration" }
        };       
        

        public Dictionary<(Type, string), string> ColumnName =>
            new Dictionary<(Type, string), string>
            {
            };


    }
}