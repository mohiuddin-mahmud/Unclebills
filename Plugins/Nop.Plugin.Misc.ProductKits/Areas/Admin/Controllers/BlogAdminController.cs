using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Events;
using Nop.Services.Blogs;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using NopValley.Plugins.Misc.Areas.Admin.Models;
using NopValley.Plugins.Misc.Domain;
using NopValley.Plugins.Misc.Services;

namespace NopValley.Plugins.Misc.Areas.Admin.Controllers
{
    public class BlogAdminController : BaseAdminController
    {
        #region Fields

        private readonly IBlogModelFactory _blogModelFactory;

        private readonly IBlogService _blogService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreService _storeService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IBlogAdditionalService _blogPostAdditionalService;
        private readonly IWorkContext _workContext;
        public string viewPath = "~/Plugins/NopValley.Misc/Areas/Admin/Views/BlogAdditional/";


        #endregion

        #region Ctor

        public BlogAdminController(
            IBlogModelFactory blogModelFactory,
            IBlogService blogService,
            ICustomerActivityService customerActivityService,
            IEventPublisher eventPublisher,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IUrlRecordService urlRecordService,
            IWorkContext workContext,
            IBlogAdditionalService blogPostAdditionalService)
        {
            _blogModelFactory = blogModelFactory;
            _blogService = blogService;
            _customerActivityService = customerActivityService;
            _eventPublisher = eventPublisher;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _storeMappingService = storeMappingService;
            _storeService = storeService;
            _urlRecordService = urlRecordService;
            _workContext = workContext;
            _blogPostAdditionalService = blogPostAdditionalService;
        }

        #endregion

        #region BlogPost edits
        [HttpPost]
        public async Task<IActionResult> SaveBlogPostAdditionsAsync(BlogPostAdditional model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageBlog))
                return AccessDeniedView();

            var blog = await _blogService.GetBlogPostByIdAsync(model.BlogPostId);

            if (blog == null)
                return new JsonResult(new { success = false });

            //TODO: Get the previous image id, and remove it if it's different...


            var blogPostAdditional = await _blogPostAdditionalService.GetBlogPostAdditionalByBlogPostIdAsync(blog.Id);

            if (blogPostAdditional != null)
            {
                blogPostAdditional.PictureId = model.PictureId;
                await _blogPostAdditionalService.UpdateBlogAdditional(blogPostAdditional);
            }

            else
            {
                blogPostAdditional = new BlogPostAdditional();
                blogPostAdditional.BlogPostId = blog.Id;
                blogPostAdditional.PictureId = model.PictureId;
                await _blogPostAdditionalService.InsertBlogAdditional(blogPostAdditional);
            }

            //await _genericAttributeService.SaveAttributeAsync(blog, GenericAttributeNames.Blog.IMAGE_ID, model.ImageId);
            //await _genericAttributeService.SaveAttributeAsync(blog, GenericAttributeNames.Blog.IS_TOP_BLOG_POST, model.IsTopBlogPost);
            //await _genericAttributeService.SaveAttributeAsync(blog, GenericAttributeNames.Blog.CATEGORY, model.Category);

            return new JsonResult(new { success = true, message = "Successfully Saved." });
        }
        #endregion

    }
}