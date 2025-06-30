using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Caching;
using Nop.Core.Domain.Media;
using Nop.Core;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Controllers;
using NopValley.Plugins.Misc.Areas.Admin.Services;
using NopValley.Plugins.Misc.Factories;
using NopValley.Plugins.Misc.Models;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Media;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using System.Linq;
using System;
using NopValley.Plugins.Misc.Domain;
using NopValley.Plugins.Misc.Services;
using Nop.Data;

namespace NopValley.Plugins.Misc.Controllers
{
    public partial class ServicePostController : BasePublicController
    {
        #region Fields

        private readonly IServicePostModelFactory _servicePostModelFactory;
        private readonly IServicePostService _servicePostService;
        protected readonly IStoreContext _storeContext;
        protected readonly IWorkContext _workContext;
        protected readonly MediaSettings _mediaSettings;
        protected readonly IWebHelper _webHelper;
        protected readonly IStaticCacheManager _staticCacheManager;
        protected readonly IPictureService _pictureService;
        protected readonly ILocalizationService _localizationService;
        protected readonly IUrlRecordService _urlRecordService;       

        public string viewPath = "~/Plugins/NopValley.Misc/Views/ServicePosts/";

        #endregion

        #region Ctor

        public ServicePostController(IServicePostModelFactory servicePostModelFactory,
            IServicePostService servicePostService,
            IStoreContext storeContext,
        IWorkContext workContext,
        MediaSettings mediaSettings,
        IWebHelper webHelper,
        IStaticCacheManager staticCacheManager,
        ILocalizationService localizationService,
        IUrlRecordService urlRecordService,
        IPictureService pictureService)
        {
            _servicePostModelFactory = servicePostModelFactory;
            _servicePostService = servicePostService;
            _storeContext = storeContext;
            _workContext = workContext;
            _mediaSettings = mediaSettings;
            _webHelper = webHelper;
            _staticCacheManager = staticCacheManager;
            _localizationService = localizationService;
            _urlRecordService = urlRecordService;
            _pictureService = pictureService;
        }

        #endregion

        #region Service Posts

        public virtual async Task<IActionResult> ServicePosts()
        {           
            var model = await _servicePostModelFactory.PrepareServicePostListModelAsync();
            return View(viewPath + "List.cshtml", model);
        }

        public virtual async Task<IActionResult> ServicePost(int servicePostId)
        {            

            var servicePost = await _servicePostService.GetServicePostByIdAsync(servicePostId);
            if (servicePost == null)
                return InvokeHttp404();

            //var notAvailable =
            //    //availability dates
            //    !_servicePostService.BlogPostIsAvailable(servicePost)
           
        

            
            var model = new PublicServicePostModel();
            model = servicePost.ToModel<PublicServicePostModel>();

            var currentStore = await _storeContext.GetCurrentStoreAsync();
            var pictureSize = _mediaSettings.CategoryThumbPictureSize;

            var categoryPictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.CategoryPictureModelKey, model,
                pictureSize, true, await _workContext.GetWorkingLanguageAsync(), _webHelper.IsCurrentConnectionSecured(),
            currentStore);


            IList<PictureModel> allPictureModels;

            (model.DefaultPictureModel, allPictureModels) = await PrepareServicePostDetailsPictureModelAsync(servicePost);
            model.PictureModels = allPictureModels;

            model.PictureModel = await _staticCacheManager.GetAsync(categoryPictureCacheKey, async () =>
            {
                var picture = await _pictureService.GetPictureByIdAsync(model.PictureId);
                string fullSizeImageUrl, imageUrl;

                (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                (imageUrl, _) = await _pictureService.GetPictureUrlAsync(picture, pictureSize);

                var pictureModel = new PictureModel
                {
                    FullSizeImageUrl = fullSizeImageUrl,
                    ImageUrl = imageUrl,
                    Title = string.Format(await _localizationService
                        .GetResourceAsync("Media.Category.ImageLinkTitleFormat"), model.Name),
                    AlternateText = string.Format(await _localizationService
                        .GetResourceAsync("Media.Category.ImageAlternateTextFormat"), model.Name)
                };

                return pictureModel;
            });

            //await _servicePostModelFactory.PrepareServicePostModelAsync(model, servicePost);

            return View(viewPath + "ServicePost.cshtml", model);
        }

        protected virtual async Task<(PictureModel pictureModel, IList<PictureModel> allPictureModels)> PrepareServicePostDetailsPictureModelAsync(ServicePost servicePost)
        {
            ArgumentNullException.ThrowIfNull(servicePost);

            //default picture size
            var defaultPictureSize = 1600;

            //prepare picture models
            //var productPicturesCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.ProductDetailsPicturesModelKey
            //    , servicePost, defaultPictureSize,
            //    await _workContext.GetWorkingLanguageAsync(), _webHelper.IsCurrentConnectionSecured(), await _storeContext.GetCurrentStoreAsync());

            var productName = await _localizationService.GetLocalizedAsync(servicePost, x => x.Name);

            var pictures = await _servicePostService.GetPicturesByServicePostIdAsync(servicePost.Id);
            var defaultPicture = pictures.FirstOrDefault();

            (var fullSizeImageUrl, defaultPicture) = await _pictureService.GetPictureUrlAsync(defaultPicture, 0);
            (var imageUrl, defaultPicture) = await _pictureService.GetPictureUrlAsync(defaultPicture, defaultPictureSize);

            var defaultPictureModel = new PictureModel
            {
                ImageUrl = imageUrl,
                FullSizeImageUrl = fullSizeImageUrl,
                //"title" attribute
                Title = (defaultPicture != null && !string.IsNullOrEmpty(defaultPicture.TitleAttribute)) ?
                    defaultPicture.TitleAttribute :
                    string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName),
                //"alt" attribute
                AlternateText = (defaultPicture != null && !string.IsNullOrEmpty(defaultPicture.AltAttribute)) ?
                    defaultPicture.AltAttribute :
                    string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName)
            };

            //all pictures
            var pictureModels = new List<PictureModel>();
            for (var i = 0; i < pictures.Count; i++)
            {
                var picture = pictures[i];

                (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, defaultPictureSize);
                (var thumbImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage);

                var pictureModel = new PictureModel
                {
                    Id = picture.Id,
                    ImageUrl = imageUrl,
                    ThumbImageUrl = thumbImageUrl,
                    FullSizeImageUrl = fullSizeImageUrl,
                    Title = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName),
                    AlternateText = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName),
                };
                //"title" attribute
                pictureModel.Title = !string.IsNullOrEmpty(picture.TitleAttribute) ?
                    picture.TitleAttribute :
                    string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName);
                //"alt" attribute
                pictureModel.AlternateText = !string.IsNullOrEmpty(picture.AltAttribute) ?
                    picture.AltAttribute :
                    string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName);

                pictureModels.Add(pictureModel);
            }           

            var allPictureModels = pictureModels;           
           
            return (defaultPictureModel, allPictureModels);
        }

        //[HttpPost]
        //public virtual async Task<IActionResult> GetCatalogSubCategories(int id, int count = 0)
        //{
        //    var model = await _catalogModelFactory.PrepareSubCategoriesAsync(id);
        //    var selectedText = string.Empty;
        //    if(count == 0)
        //    {
        //        selectedText = await _localizationService.GetResourceAsync("NopValley.Plugins.Misc.VehicleBrand");
        //    }
        //    else if (count == 1)
        //    {
        //        selectedText = await _localizationService.GetResourceAsync("NopValley.Plugins.Misc.VehicleModel");
        //    }
        //    else if (count == 2)
        //    {
        //        selectedText = await _localizationService.GetResourceAsync("NopValley.Plugins.Misc.CamKonumu");
        //    }

        //    //return Json(model);
        //    TempData["selectedText"] = selectedText;
        //    TempData["subcat"] = count;
        //    return View(viewPath + "SubCategories.cshtml", model);
        //}

        #endregion

    }
}