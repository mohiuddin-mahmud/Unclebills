using System;
using Nop.Core;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Stores;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopValley.Plugins.Misc.Domain
{
    /// <summary>
    /// Represents a Service Category
    /// </summary>
    public partial class ServicePostCategory : BaseEntity, ISlugSupported
    {   
        public string Name { get; set; }
        public int LanguageId { get; set; }
    }
}