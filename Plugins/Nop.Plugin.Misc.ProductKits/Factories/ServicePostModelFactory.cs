using System.Threading.Tasks;
using NopValley.Plugins.Misc.Areas.Admin.Services;
using NopValley.Plugins.Misc.Models;
using System.Linq;
using Nop.Core.Domain.Blogs;
using NopValley.Plugins.Misc.Domain;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Core.Domain.Media;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Services.Media;
using Nop.Web.Models.Media;
using Nop.Services.Localization;
using Nop.Web.Infrastructure.Cache;
using Nop.Services.Seo;
using NopValley.Plugins.Misc.Areas.Admin.Models.ServicePost;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace NopValley.Plugins.Misc.Factories;

/// <summary>
/// Represents the common models factory
/// </summary>
public partial class ServicePostModelFactory : IServicePostModelFactory
{
    #region Fields

    protected readonly IServicePostService _servicePostService;
    protected readonly IStoreContext _storeContext;
    protected readonly IWorkContext _workContext;
    protected readonly MediaSettings _mediaSettings;
    protected readonly IWebHelper _webHelper;
    protected readonly IStaticCacheManager _staticCacheManager;
    protected readonly IPictureService _pictureService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IUrlRecordService _urlRecordService;
    #endregion

    #region Ctor

    public ServicePostModelFactory(IServicePostService servicePostService,
        IStoreContext storeContext,
        IWorkContext workContext,
        MediaSettings mediaSettings,
        IWebHelper webHelper,
        IStaticCacheManager staticCacheManager,
        ILocalizationService localizationService,
        IUrlRecordService urlRecordService,
        IPictureService pictureService
        )
    {
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



    #region Methods

    /// <summary>
    /// Prepare the logo model
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the logo model
    /// </returns>
    public virtual async Task PrepareServicePostModelAsync(PublicServicePostModel servicePostModel, ServicePost servicePost)
    {
        await Task.Yield();
        servicePostModel = servicePost.ToModel<PublicServicePostModel>();
        
        //return model;
    }
    public virtual async Task<PublicServicePostListModel> PrepareServicePostListModelAsync()
    {
        var servicePosts = await _servicePostService.GetAllServicePostsAsync();

        IList<SelectListItem> categories = new List<SelectListItem>();
        var model = new PublicServicePostListModel
        {


            //Categories.Add( Id: servicePostModel.CategoryId );
            
           

            ServicePosts = await servicePosts.SelectAwait(async servicePost =>
            {
                var servicePostModel = new PublicServicePostModel();
                //await PrepareServicePostModelAsync(servicePostModel, servicePost);

                servicePostModel = servicePost.ToModel<PublicServicePostModel>();

                //if(categories.Where(x=>x.Value != servicePost.ServicePostCategoryId.ToString()).Count() == 0)
                //{
                //    var category = await _servicePostService.GetServicePostCategoryByIdAsync(servicePost.ServicePostCategoryId);
                //    categories.Add(new SelectListItem() { Text= category.Name, Value = category.Id.ToString() });
                //}


                servicePostModel.SeName = await _urlRecordService.GetSeNameAsync(servicePost);

                var currentStore = await _storeContext.GetCurrentStoreAsync();
                var pictureSize = _mediaSettings.CategoryThumbPictureSize;

                var categoryPictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.CategoryPictureModelKey, servicePostModel,
                    pictureSize, true, await _workContext.GetWorkingLanguageAsync(), _webHelper.IsCurrentConnectionSecured(),
                    currentStore);

                servicePostModel.PictureModel = await _staticCacheManager.GetAsync(categoryPictureCacheKey, async () =>
                {
                    var picture = await _pictureService.GetPictureByIdAsync(servicePostModel.PictureId);
                    string fullSizeImageUrl, imageUrl;

                    (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                    (imageUrl, _) = await _pictureService.GetPictureUrlAsync(picture, pictureSize);

                    var pictureModel = new PictureModel
                    {
                        FullSizeImageUrl = fullSizeImageUrl,
                        ImageUrl = imageUrl,
                        Title = string.Format(await _localizationService
                            .GetResourceAsync("Media.Category.ImageLinkTitleFormat"), servicePostModel.Name),
                        AlternateText = string.Format(await _localizationService
                            .GetResourceAsync("Media.Category.ImageAlternateTextFormat"), servicePostModel.Name)
                    };

                    return pictureModel;
                });

                await Task.Yield();
                return servicePostModel;
            }).ToListAsync()
        };      
        model.Categories = categories;     

        return model;
    }

    
    #endregion
}