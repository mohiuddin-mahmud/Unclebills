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
using Nop.Core.Domain.Localization;
using Nop.Web.Models.Common;

namespace NopValley.Plugins.Misc.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the topic model factory implementation
    /// </summary>
    public partial class ReferenceModelFactory : IReferenceModelFactory
    {
        #region Fields

    
        private readonly IReferenceService _referenceService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
        private readonly IReferenceCategoryService _referenceCategoryService;
        private readonly IStoreContext _storeContext;
        private readonly ILanguageService _languageService;
        private readonly IWorkContext _workContext;
        #endregion

        #region Ctor

        public ReferenceModelFactory(      
            IReferenceService referenceService,
            ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
            IReferenceCategoryService referenceCategoryService,
            IStoreContext storeContext,
            ILanguageService languageService,
            IWorkContext workContext
            )
        {           
            _referenceService = referenceService;
            _localizationService = localizationService;
            _localizedModelFactory = localizedModelFactory;
            _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
            _referenceCategoryService = referenceCategoryService;
            _storeContext = storeContext;
            _languageService = languageService;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare topic search model
        /// </summary>
        /// <param name="searchModel">Topic search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the opic search model
        /// </returns>
        //public virtual async Task<MegaMenuTopicSearchModel> PrepareTopicSearchModelAsync(MegaMenuTopicSearchModel searchModel)
        //{
        //    if (searchModel == null)
        //        throw new ArgumentNullException(nameof(searchModel));
                        
        //    //prepare page parameters
        //    searchModel.SetGridPageSize();
        //    await Task.Yield();
        //    return searchModel;
        //}

        ///// <summary>
        ///// Prepare paged topic list model
        ///// </summary>
        ///// <param name="searchModel">Topic search model</param>
        ///// <returns>
        ///// A task that represents the asynchronous operation
        ///// The task result contains the opic list model
        ///// </returns>
        //public virtual async Task<MegaMenuTopicListModel> PrepareTopicListModelAsync(MegaMenuTopicSearchModel searchModel)
        //{
        //    if (searchModel == null)
        //        throw new ArgumentNullException(nameof(searchModel));

        //    var megaMenuTopics = await _megaMenuTopicService.GetAllMegaMenuTopicsAsync(
        //        topicName: searchModel.SearchKeywords,
        //        pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            
        //    //prepare grid model
        //    var model = await new MegaMenuTopicListModel().PrepareToGridAsync(searchModel, megaMenuTopics, () =>
        //    {
        //        return megaMenuTopics.SelectAwait(async topic =>
        //        {
        //            //fill in model values from the entity
        //            var megaMenuTopicModel = topic.ToModel<MegaMenuTopicModel>();

        //            await Task.Yield();
        //            return megaMenuTopicModel;
        //        });
        //    });

        //    return model;
        //}

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
        public virtual async Task<ReferenceModel> PrepareReferenceModelAsync(ReferenceModel model, Reference reference)
        {
            Func<ReferenceLocalizedModel, int, Task> localizedModelConfiguration = null;
            if (reference != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = reference.ToModel<ReferenceModel>();

                   
                }
                //define localized model configuration action
                localizedModelConfiguration = async (locale, languageId) =>
                {
                    locale.Name = await _localizationService.GetLocalizedAsync(reference, entity => entity.Name, languageId, false, false);
                    locale.Description = await _localizationService.GetLocalizedAsync(reference, entity => entity.Description, languageId, false, false);

                };
            }           

            //prepare localized models
            model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

            //prepare model stores
            await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, reference, false);


            //prepare model manufacturers
            //await _baseAdminModelFactory.PrepareManufacturersAsync(model.AvailableManufacturers, false);

            //foreach (var manufacturerItem in model.AvailableCategories)
            //{
            //    manufacturerItem.Selected = int.TryParse(manufacturerItem.Value, out var manufacturerId)
            //        && model.SelectedManufacturerIds.Contains(manufacturerId);
            //}

            //model.AvailableCategories.Add(new SelectListItem() { Text = "4D Wand", Value = "1" });
            //model.AvailableCategories.Add(new SelectListItem() { Text = "cloud APP server", Value = "2" });
            //model.AvailableCategories.Add(new SelectListItem() { Text = "mail server", Value = "3" });
            //model.AvailableCategories.Add(new SelectListItem() { Text = "web hosting", Value = "4" });
            //model.AvailableCategories.Add(new SelectListItem() { Text = "e-commercewebshop", Value = "5" });

            if (reference != null)
            {
                var refCategories = await _referenceCategoryService.GetAllRefCategoriesAsync(reference.Id);

                if (refCategories != null)
                    foreach (var refCategory in refCategories)
                    {
                        model.AvailableCategories.Add(new SelectListItem() { Text = refCategory.Name, Value = refCategory.Id.ToString() });
                    }

                model.SelectedCategoryIds = (await _referenceService.GetRefCategoriesByReferenceIdAsync(reference.Id))
                  .Select(refCat => refCat.RefCategoryId).ToList();

                foreach (var categoryItem in model.AvailableCategories)
                {
                    categoryItem.Selected = int.TryParse(categoryItem.Value, out var categoryId)
                        && model.SelectedCategoryIds.Contains(categoryId);
                }
            }
          
            else
            {
                var refCategories = await _referenceCategoryService.GetAllRefCategoriesAsync();
                if (refCategories != null)
                    foreach (var refCategory in refCategories)
                    {
                        model.AvailableCategories.Add(new SelectListItem() { Text = refCategory.Name, Value = refCategory.Id.ToString() });
                    }
            }
            


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
        public virtual async Task<ReferenceSearchModel> PrepareReferenceSearchModelAsync(ReferenceSearchModel searchModel)
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
        public virtual async Task<ReferenceListModel> PrepareReferenceListModelAsync(ReferenceSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var caseStudies = await _referenceService.GetAllReferencesAsync(
                name: searchModel.SearchKeywords,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);


            //prepare grid model
            var model = await new ReferenceListModel().PrepareToGridAsync(searchModel, caseStudies, () =>
            {
                return caseStudies.SelectAwait(async caseStudy =>
                {
                    //fill in model values from the entity
                    var referenceModel = caseStudy.ToModel<ReferenceModel>();

                    await Task.Yield();
                    return referenceModel;
                });
            });

            return model;
        }

        #endregion

        #region
        public virtual async Task<LanguageSelectorModel> PrepareLanguageSelectorModelAsync()
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var availableLanguages = (await _languageService
                    .GetAllLanguagesAsync(storeId: store.Id))
                    .Select(x => new LanguageModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        FlagImageFileName = x.FlagImageFileName,
                    }).ToList();

            var model = new LanguageSelectorModel
            {
                CurrentLanguageId = (await _workContext.GetWorkingLanguageAsync()).Id,
                AvailableLanguages = availableLanguages               
            };

            return model;
        }



        #endregion
    }
}