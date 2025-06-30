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
using Nop.Services.Catalog;

namespace NopValley.Plugins.Misc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class CategoryApiController : BasePublicController
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

        

        #endregion

        private readonly ICategoryService _categoryService;

      
        public CategoryApiController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync(showHidden: true);
            var result = categories.Select(c => new
            {
                c.Id,
                c.Name,
                c.Description,
                c.ParentCategoryId
            });

            return Ok(result);
        }

    }
}