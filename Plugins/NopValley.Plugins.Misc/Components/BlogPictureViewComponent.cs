using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Services.Blogs;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Blogs;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using NopValley.Plugins.Misc.Areas.Admin.Models;
using NopValley.Plugins.Misc.Domain;
using NopValley.Plugins.Misc.Services;


namespace NopValley.Plugins.Misc.Components
{
    public partial class BlogPictureViewComponent : NopViewComponent
    {
        private readonly CatalogSettings _catalogSettings;
        private readonly IBlogModelFactory _blogModelFactory;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IBlogAdditionalService _blogPostAdditionalService;
        private readonly IStoreContext _storeContext;
        private readonly MediaSettings _mediaSettings;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly IBlogService _blogService;
        private readonly ICustomerService _customerService;

        public string viewPath = "~/Plugins/NopValley.Misc/Views/Components/Blog/";

        public BlogPictureViewComponent(CatalogSettings catalogSettings, IBlogModelFactory blogModelFactory,
            IStaticCacheManager staticCacheManager,
            IWebHelper webHelper,
            IWorkContext workContext,
            IBlogAdditionalService blogPostAdditionalService,
            IStoreContext storeContext,
            MediaSettings mediaSettings,
            IPictureService pictureService,
            ILocalizationService localizationService,
            IBlogService blogService,
            ICustomerService customerService
            )
        {
            _catalogSettings = catalogSettings;
            _blogModelFactory = blogModelFactory;
            _staticCacheManager = staticCacheManager;
            _webHelper = webHelper;
            _workContext = workContext;
            _blogPostAdditionalService = blogPostAdditionalService;
            _storeContext = storeContext;
            _mediaSettings = mediaSettings;
            _pictureService = pictureService;
            _localizationService = localizationService;
            _blogService = blogService;
            _customerService = customerService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var blogId = additionalData is BlogPostModel model ? model.Id : 0;

            BlogPagingFilteringModel command = new BlogPagingFilteringModel();
            var blogList = await _blogModelFactory.PrepareBlogPostListModelAsync(command);

            var blogPictureModel = new BlogAdditionalModel();

            var blog = await _blogService.GetBlogPostByIdAsync(blogId);
            var blogPostAdditional = await _blogPostAdditionalService.GetBlogPostAdditionalByBlogPostIdAsync(blog.Id);


            var currentStore = await _storeContext.GetCurrentStoreAsync();
            var pictureSize = _mediaSettings.CategoryThumbPictureSize;

            //prepare picture model
            var categoryPictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.CategoryPictureModelKey, blog,
                pictureSize, true, await _workContext.GetWorkingLanguageAsync(), _webHelper.IsCurrentConnectionSecured(),
            currentStore);

            if (blogPostAdditional !=null && blogPostAdditional.PictureId > 0 && blog != null)
            {
                var picture = await _pictureService.GetPictureByIdAsync(blogPostAdditional.PictureId);
                string fullSizeImageUrl, imageUrl;

                (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                (imageUrl, _) = await _pictureService.GetPictureUrlAsync(picture, pictureSize);

                var pictureModel = new PictureModel
                {
                    FullSizeImageUrl = fullSizeImageUrl,
                    ImageUrl = imageUrl,
                    Title = string.Format(await _localizationService
                        .GetResourceAsync("Media.Category.ImageLinkTitleFormat"), blog.Title),
                    AlternateText = string.Format(await _localizationService
                        .GetResourceAsync("Media.Category.ImageAlternateTextFormat"), blog.Title)
                };


                blogPictureModel.PictureModel = pictureModel;
            }


            return View(viewPath + "Picture.cshtml", blogPictureModel);
        }
    }
}
