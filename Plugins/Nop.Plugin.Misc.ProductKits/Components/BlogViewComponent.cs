using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
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
using Nop.Web.Models.Media;
using NopValley.Plugins.Misc.Areas.Admin.Models;
using NopValley.Plugins.Misc.Services;


namespace NopValley.Plugins.Misc.Components
{
    public partial class BlogViewComponent : NopViewComponent
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

        public string viewPath = "~/Plugins/NopValley.Misc/Views/";

        public BlogViewComponent(CatalogSettings catalogSettings, IBlogModelFactory blogModelFactory,
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

        public async Task<IViewComponentResult> InvokeAsync()
        {

            BlogPagingFilteringModel command = new BlogPagingFilteringModel();
            var blogList = await _blogModelFactory.PrepareBlogPostListModelAsync(command);

            IList<BlogAdditionalModel> additionalBlogList = new List<BlogAdditionalModel>();

            foreach(var blogPost in blogList.BlogPosts)
            {
                var model = new BlogAdditionalModel();

                var blogPostAdditional = await _blogPostAdditionalService.GetBlogPostAdditionalByBlogPostIdAsync(blogPost.Id);

                var blog = await _blogService.GetBlogPostByIdAsync(blogPost.Id);

                //var blogPost = await _blogService.GetBlogPostByIdAsync(blogPostId);
                var blogPostModel = new BlogPostModel();
                await _blogModelFactory.PrepareBlogPostModelAsync(blogPostModel, blog, true);

                model.Title = blogPost.Title;
                model.Body = blogPost.Body.Length < 200 ? blogPost.Body : blogPost.Body.Substring(0, 200);
                model.SeName = blogPost.SeName;
                model.CreatedOn = blogPost.CreatedOn;

                var currentStore = await _storeContext.GetCurrentStoreAsync();
                var pictureSize = _mediaSettings.CategoryThumbPictureSize;

                //prepare picture model
                var categoryPictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.CategoryPictureModelKey, blog,
                    pictureSize, true, await _workContext.GetWorkingLanguageAsync(), _webHelper.IsCurrentConnectionSecured(),
                    currentStore);

                if(blogPostAdditional != null) 
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


                    model.PictureModel = pictureModel;
                }
                
                additionalBlogList.Add(model);
            }

            var customer = await _workContext.GetCurrentCustomerAsync();
            //if (await _customerService.IsRegisteredAsync(customer))
            //{
            //    return Content("");
            //}

            return View(viewPath + "Blog.cshtml", additionalBlogList);
        }
    }
}
