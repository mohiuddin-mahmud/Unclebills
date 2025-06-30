using System;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Services.Blogs;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using Nop.Core;
using Nop.Web.Areas.Admin.Models.Blogs;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopValley.Plugins.Misc.Areas.Admin.Factories;
using NopValley.Plugins.Misc.Areas.Admin.Models;
using NopValley.Plugins.Misc.Domain;
using NopValley.Plugins.Misc.Areas.Admin.Services;
using NopValley.Plugins.Misc.Areas.Admin.Models.ServicePost;
using System.Collections.Generic;
using Nop.Core.Caching;
using NopValley.Plugins.Misc.Areas.Admin.Infrastructure.Cache;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Services.Media;

namespace NopValley.Plugins.Misc.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the blog model factory implementation
    /// </summary>
    public partial class ServicePostAdminModelFactory : IServicePostAdminModelFactory
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IBlogService _blogService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
        private readonly IStoreService _storeService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IServicePostService _servicePostService;
        private readonly IWorkContext _workContext;
        protected readonly IStaticCacheManager _staticCacheManager;
        protected readonly IPictureService _pictureService;

        #endregion

        #region Ctor

        public ServicePostAdminModelFactory(CatalogSettings catalogSettings,
            IBaseAdminModelFactory baseAdminModelFactory,
            IBlogService blogService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
            IStoreService storeService,
            IUrlRecordService urlRecordService,
            IServicePostService servicePostService,
            IWorkContext workContext,
            IStaticCacheManager staticCacheManager,
            IPictureService pictureService)
        {
            _catalogSettings = catalogSettings;
            _baseAdminModelFactory = baseAdminModelFactory;
            _blogService = blogService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _languageService = languageService;
            _localizationService = localizationService;
            _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
            _storeService = storeService;
            _urlRecordService = urlRecordService;
            _servicePostService = servicePostService;
            _workContext = workContext;
            _staticCacheManager = staticCacheManager;
            _pictureService = pictureService;
        }

        #endregion

        #region Methods



        /// <summary>
        /// Prepare blog post model
        /// </summary>
        /// <param name="model">Blog post model</param>
        /// <param name="blogPost">Blog post</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the blog post model
        /// </returns>
        public virtual async Task<ServicePostModel> PrepareServicePostModelAsync(ServicePostModel model, ServicePost servicePost)
        {
            //fill in model values from the entity
            if (servicePost != null)
            {
                if (model == null)
                {
                    model = servicePost.ToModel<ServicePostModel>();
                    model.SeName = await _urlRecordService.GetSeNameAsync(servicePost, servicePost.LanguageId, true, false);
                }
            }         


            //prepare available languages
            await _baseAdminModelFactory.PrepareLanguagesAsync(model.AvailableLanguages, false);

            //prepare available categories
            await PrepareCategoriesAsync(model.AvailableCategories,
                defaultItemText: await _localizationService.GetResourceAsync("Admin.Catalog.Categories.Fields.Parent.None"));


            PrepareServicePostPictureSearchModel(model.ServicePostPictureSearchModel, servicePost);
            return model;
        }


        public virtual async Task PrepareCategoriesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            ArgumentNullException.ThrowIfNull(items);

            //prepare available categories
            var availableCategoryItems = await GetCategoryListAsync();
            foreach (var categoryItem in availableCategoryItems)
            {
                items.Add(categoryItem);
            }

            //insert special item for the default value
            await PrepareDefaultItemAsync(items, withSpecialDefaultItem, defaultItemText);
        }

        protected virtual async Task<List<SelectListItem>> GetCategoryListAsync()
        {
            var listItems = await _staticCacheManager.GetAsync(NopModelCacheDefaults.CategoriesListKey, async () =>
            {
                var categories = await _servicePostService.GetAllServicePostCategoriesAsync(showHidden: true);
                return await categories.SelectAwait(async c => new SelectListItem
                {
                    Text = c.Name.ToString(),
                    Value = c.Id.ToString()
                }).ToListAsync();
            });

            var result = new List<SelectListItem>();
            //clone the list to ensure that "selected" property is not set
            foreach (var item in listItems)
            {
                result.Add(new SelectListItem
                {
                    Text = item.Text,
                    Value = item.Value
                });
            }

            return result;
        }

        protected virtual async Task PrepareDefaultItemAsync(IList<SelectListItem> items, bool withSpecialDefaultItem, string defaultItemText = null, string defaultItemValue = "0")
        {
            ArgumentNullException.ThrowIfNull(items);

            //whether to insert the first special item for the default value
            if (!withSpecialDefaultItem)
                return;

            //prepare item text
            defaultItemText ??= await _localizationService.GetResourceAsync("Admin.Common.All");

            //insert this default item at first
            items.Insert(0, new SelectListItem { Text = defaultItemText, Value = defaultItemValue });
        }

        public virtual async Task<ServicePostSearchModel> PrepareServicePostSearchModelAsync(ServicePostSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));


            await _localizationService.GetResourceAsync("Admin.Catalog.Categories.List.SearchPublished.All");

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged0blog category list model
        /// </summary>
        /// <param name="searchModel">Category search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category list model
        /// </returns>
        public virtual async Task<ServicePostListModel> PrepareServicePostListModelAsync(ServicePostSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            //get servicePosts
            var servicePosts = await _servicePostService.GetAllServicePostsAsync(servicePostName: searchModel.SearchTitle,
                showHidden: true,

                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new ServicePostListModel().PrepareToGridAsync(searchModel, servicePosts, () =>
            {

                return servicePosts.SelectAwait(async servicePost =>
                {
                    await _workContext.GetCurrentVendorAsync();

                    //fill in model values from the entity
                    var servicePostModel = new ServicePostModel();

                    servicePostModel.Id = servicePost.Id;
                    servicePostModel.Name = servicePost.Name;

                    //fill in additional values (not existing in the entity)
                    //categoryModel.Breadcrumb = await _categoryService.GetFormattedBreadCrumbAsync(category);
                    //categoryModel.SeName = await _urlRecordService.GetSeNameAsync(category, 0, true, false);

                    return servicePostModel;
                });
            });

            return model;
        }


        #endregion


        #region ServicePost Category

        public virtual async Task<ServicePostCategoryModel> PrepareServicePostCategoryModelAsync(ServicePostCategoryModel model, ServicePostCategory servicePostCategory)
        {
            if (servicePostCategory != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = new ServicePostCategoryModel();
                    model.Id = servicePostCategory.Id;
                    model.Name = servicePostCategory.Name;
                    model.SeName = await _urlRecordService.GetSeNameAsync(servicePostCategory, servicePostCategory.LanguageId, true, false);
                
                }
            }
            return model;
        }


        /// <summary>
        /// Prepare category search model
        /// </summary>
        /// <param name="searchModel">Category search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category search model
        /// </returns>
        public virtual async Task<ServicePostCategorySearchModel> PrepareServicePostCategorySearchModelAsync(ServicePostCategorySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));


            await _localizationService.GetResourceAsync("Admin.Catalog.Categories.List.SearchPublished.All");

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged0blog category list model
        /// </summary>
        /// <param name="searchModel">Category search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category list model
        /// </returns>
        public virtual async Task<ServicePostCategoryListModel> PrepareServicePostCategoryListModelAsync(ServicePostCategorySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            //get categories
            var servicePostCategories = await _servicePostService.GetAllServicePostCategoriesAsync(categoryName: searchModel.SearchServicePostCategoryName,
                showHidden: true,
                
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new ServicePostCategoryListModel().PrepareToGridAsync(searchModel, servicePostCategories, () =>
            {

                return servicePostCategories.SelectAwait(async blogCategory =>
                {
                    await _workContext.GetCurrentVendorAsync();

                    //fill in model values from the entity
                    var servicePostCategoryModel = new ServicePostCategoryModel();

                    servicePostCategoryModel.Id = blogCategory.Id;
                    servicePostCategoryModel.Name = blogCategory.Name;

                    //fill in additional values (not existing in the entity)
                    //categoryModel.Breadcrumb = await _categoryService.GetFormattedBreadCrumbAsync(category);
                    //categoryModel.SeName = await _urlRecordService.GetSeNameAsync(category, 0, true, false);

                    return servicePostCategoryModel;
                });
            });

            return model;
        }

        #endregion

        public virtual async Task<ServicePostPictureListModel> PrepareServicePostPictureListModelAsync(ServicePostPictureSearchModel searchModel, ServicePost servicePost)
        {
            ArgumentNullException.ThrowIfNull(searchModel);
            ArgumentNullException.ThrowIfNull(servicePost);

            //get product pictures
            var servicePostPictures = (await _servicePostService.GetServicePostPicturesByProductIdAsync(servicePost.Id)).ToPagedList(searchModel);

            //prepare grid model
            var model = await new ServicePostPictureListModel().PrepareToGridAsync(searchModel, servicePostPictures, () =>
            {
                return servicePostPictures.SelectAwait(async productPicture =>
                {
                    //fill in model values from the entity
                    var servicePostPictureModel = productPicture.ToModel<ServicePostPictureModel>();

                    //fill in additional values (not existing in the entity)
                    var picture = (await _pictureService.GetPictureByIdAsync(productPicture.PictureId))
                                  ?? throw new Exception("Picture cannot be loaded");

                    servicePostPictureModel.PictureUrl = (await _pictureService.GetPictureUrlAsync(picture)).Url;

                    servicePostPictureModel.OverrideAltAttribute = picture.AltAttribute;
                    servicePostPictureModel.OverrideTitleAttribute = picture.TitleAttribute;

                    return servicePostPictureModel;
                });
            });

            return model;
        }

        protected virtual ServicePostPictureSearchModel PrepareServicePostPictureSearchModel(ServicePostPictureSearchModel searchModel, ServicePost servicePost)
        {
            ArgumentNullException.ThrowIfNull(searchModel);
            ArgumentNullException.ThrowIfNull(servicePost);

            searchModel.ServicePostId = servicePost.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }
    }
}