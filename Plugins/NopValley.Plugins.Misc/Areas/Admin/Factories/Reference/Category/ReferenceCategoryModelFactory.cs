using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Topics;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Services.Topics;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Topics;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Factories;

using NopValley.Plugins.Misc.Areas.Admin.Models;
using NopValley.Plugins.Misc.Domain;
using NopValley.Plugins.Misc.Services;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;

namespace NopValley.Plugins.Misc.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the topic model factory implementation
    /// </summary>
    public partial class ReferenceCategoryModelFactory : IReferenceCategoryModelFactory
    {
        #region Fields

    
        private readonly IReferenceCategoryService _referenceCategoryService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
        #endregion

        #region Ctor

        public ReferenceCategoryModelFactory(      
            IReferenceCategoryService referenceCategoryService,
            ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory
            )
        {
            _referenceCategoryService = referenceCategoryService;
            _localizationService = localizationService;
            _localizedModelFactory = localizedModelFactory;
            _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory; 
        }

        #endregion

        #region Methods

        

        /// <summary>
        /// Prepare topic model
        /// </summary>
        /// <param name="model">Topic model</param>
        /// <param name="topic">Topic</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the opic model
        /// </returns>
        public virtual async Task<ReferenceCategoryModel> PrepareReferenceCategoryModelAsync(ReferenceCategoryModel model, RefCategory refCategory)
        {
            Func<ReferenceCategoryLocalizedModel, int, Task> localizedModelConfiguration = null;
            if (refCategory != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = refCategory.ToModel<ReferenceCategoryModel>();

                   
                }
                //define localized model configuration action
                localizedModelConfiguration = async (locale, languageId) =>
                {
                    locale.Name = await _localizationService.GetLocalizedAsync(refCategory, entity => entity.Name, languageId, false, false);
                    
                };
            }           

            //prepare localized models
            model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

            await Task.Yield();
            return model;
        }


        /// <summary>
        /// Prepare topic search model
        /// </summary>
        /// <param name="searchModel">Topic search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the opic search model
        /// </returns>
        public virtual async Task<ReferenceCategorySearchModel> PrepareReferenceCategorySearchModelAsync(ReferenceCategorySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();
            await Task.Yield();
            return searchModel;
        }

        /// <summary>
        /// Prepare paged topic list model
        /// </summary>
        /// <param name="searchModel">Topic search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the opic list model
        /// </returns>
        public virtual async Task<ReferenceCategoryListModel> PrepareReferenceCategoryListModelAsync(ReferenceCategorySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var caseStudies = await _referenceCategoryService.GetAllRefCategoriesAsync(
                name: searchModel.SearchKeywords,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);


            //prepare grid model
            var model = await new ReferenceCategoryListModel().PrepareToGridAsync(searchModel, caseStudies, () =>
            {
                return caseStudies.SelectAwait(async caseStudy =>
                {
                    //fill in model values from the entity
                    var referenceModel = caseStudy.ToModel<ReferenceCategoryModel>();

                    await Task.Yield();
                    return referenceModel;
                });
            });

            return model;
        }

        #endregion
    }
}