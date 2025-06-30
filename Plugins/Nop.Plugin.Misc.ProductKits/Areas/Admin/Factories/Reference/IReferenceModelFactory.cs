using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Topics;
using Nop.Web.Areas.Admin.Models.Topics;
using NopValley.Plugins.Misc.Areas.Admin.Models;
using NopValley.Plugins.Misc.Domain;
using Nop.Web.Models.Common;
using Nop.Core.Domain.Localization;

namespace NopValley.Plugins.Misc.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the topic model factory
    /// </summary>
    public partial interface IReferenceModelFactory
    {
        
        /// </returns>
        Task<ReferenceModel> PrepareReferenceModelAsync(ReferenceModel model, Reference reference);
        Task<ReferenceSearchModel> PrepareReferenceSearchModelAsync(ReferenceSearchModel searchModel);
        Task<ReferenceListModel> PrepareReferenceListModelAsync(ReferenceSearchModel searchModel);
        Task<LanguageSelectorModel> PrepareLanguageSelectorModelAsync();

        //Task<LanguageModel> PrepareLanguageModelAsync(LanguageModel model, Language language, bool excludeProperties = false);
    }
}