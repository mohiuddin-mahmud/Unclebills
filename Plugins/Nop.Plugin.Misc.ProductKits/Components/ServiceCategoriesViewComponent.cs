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
using NopValley.Plugins.Misc.Areas.Admin.Services;
using NopValley.Plugins.Misc.Services;


namespace NopValley.Plugins.Misc.Components
{
    public partial class ServiceCategoriesViewComponent : NopViewComponent
    {
        private readonly IServicePostService _servicePostService;

        public string viewPath = "~/Plugins/NopValley.Misc/Views/";

        public ServiceCategoriesViewComponent(IServicePostService servicePostService
            )
        {
            _servicePostService = servicePostService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {

            var servicePost = await _servicePostService.GetAllServicePostCategoriesAsync();
            return View(viewPath + "Categories.cshtml" ,  servicePost );
        }
    }
}
