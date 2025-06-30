using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Services.Blogs;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Blogs;
using NopValley.Plugins.Misc.Areas.Admin.Models;
using NopValley.Plugins.Misc.Services;

namespace NopValley.Plugins.Misc.Areas.Admin.Components
{
    public partial class BlogAdminViewComponent : NopViewComponent
    {
        private readonly IBlogService _blogService;
        private readonly IBlogAdditionalService _blogAdditionalService;
        private readonly IBlogModelFactory _blogModelFactory;
        public string viewPath = "~/Plugins/NopValley.Misc/Areas/Admin/Views/BlogAdditional/";

        public BlogAdminViewComponent(IBlogService blogService, IBlogModelFactory blogModelFactory,
            IBlogAdditionalService blogAdditionalService)
        {
            _blogService = blogService;
            _blogModelFactory = blogModelFactory;
            _blogAdditionalService = blogAdditionalService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {

            // var blogPostModel = (BlogPostModel)additionalData;
            var routeData = Url.ActionContext.RouteData;
            var model = new BlogAdditionalModel();
            var id = Convert.ToInt32(routeData.Values["id"]);
            var blog = await _blogService.GetBlogPostByIdAsync(id);

            if(id == 0)
            {
                return Content("");
            }

            var blogPostAdvanceModel = await _blogAdditionalService.GetBlogPostAdditionalByBlogPostIdAsync(blog.Id);

            model.BlogPostId = blog.Id;
            if (blogPostAdvanceModel != null)
            {
                
                model.PictureId = blogPostAdvanceModel.PictureId;
                
            }      
            

            return View(viewPath + "Default.cshtml", model);
        }
    }
}
