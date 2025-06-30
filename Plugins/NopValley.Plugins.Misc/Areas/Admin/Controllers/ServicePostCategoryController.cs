using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Events;
using NopValley.Plugins.Misc.Areas.Admin.Models;
using NopValley.Plugins.Misc.Domain;
using Nop.Services.Blogs;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using NopValley.Plugins.Misc.Areas.Admin.Services;
using NopValley.Plugins.Misc.Areas.Admin.Factories;
using Nop.Web.Framework;
using Nop.Web.Areas.Admin.Controllers;

namespace NopValley.Plugins.Misc.Areas.Admin.Controllers
{
    //[AuthorizeAdmin]
    //[Area(AreaNames.ADMIN)]
    //[AutoValidateAntiforgeryToken]
    public class ServicePostCategoryController : BaseAdminController
    {
        #region Fields

        private readonly IBlogModelFactory _blogModelFactory;
        private readonly IServicePostAdminModelFactory _servicePostAdminModelFactory;
        private readonly IBlogService _blogService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreService _storeService;
        private readonly IUrlRecordService _urlRecordService;        
        private readonly IServicePostService _servicePostService;
        private readonly IWorkContext _workContext;
      
        #endregion

        #region Ctor

        public ServicePostCategoryController(
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
            IServicePostService servicePostService,    
            IWorkContext workContext,
            IServicePostAdminModelFactory servicePostAdminModelFactory) 
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
            _servicePostService = servicePostService;
            _workContext = workContext;
            _servicePostAdminModelFactory = servicePostAdminModelFactory;
        }

        #endregion
        
        

        #region ServicePostCategory      

        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> Create()
        {          
            //prepare model
            var model = await _servicePostAdminModelFactory.PrepareServicePostCategoryModelAsync(new ServicePostCategoryModel(), null);

            return View($"{Constants.PLUGIN_ADMIN_VIEW_PATH}/ServicePostCategory/Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> Create(ServicePostCategoryModel model, bool continueEditing)
        {            
            if (ModelState.IsValid)
            {
                var servicePostCategory = new ServicePostCategory();
                servicePostCategory.Name = model.Name;

                await _servicePostService.InsertServicePostCategoryAsync(servicePostCategory);
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.ContentManagement.Blog.BlogCategory.Added"));

                if (!continueEditing)
                    return RedirectToAction("Categories");

                return RedirectToAction("Edit", new { id = servicePostCategory.Id });
            }

            //prepare model
            model = await _servicePostAdminModelFactory.PrepareServicePostCategoryModelAsync(model, null);

            //if we got this far, something failed, redisplay form
            return View($"{Constants.PLUGIN_ADMIN_VIEW_PATH}/ServicePostCategory/Create.cshtml", model);
        }

        ///// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> Edit(int id)
        {

            //try to get a servicepost category with the specified id
            var servicePostCategory = await _servicePostService.GetServicePostCategoryByIdAsync(id);
            if (servicePostCategory == null)
                return RedirectToAction("Categories");

            //prepare model
            var model = await _servicePostAdminModelFactory.PrepareServicePostCategoryModelAsync(null, servicePostCategory);

            return View($"{Constants.PLUGIN_ADMIN_VIEW_PATH}/ServicePostCategory/Edit.cshtml", model);
        }


        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> Edit(ServicePostCategoryModel model, bool continueEditing)
        {            
            //try to get a blog Category with the specified id
            var servicePostCategory = await _servicePostService.GetServicePostCategoryByIdAsync(model.Id);
            if (servicePostCategory == null)
                return RedirectToAction("Categories");

            if (ModelState.IsValid)
            {
                servicePostCategory = new ServicePostCategory();
                servicePostCategory.Id = model.Id;
                servicePostCategory.Name = model.Name;
                await _servicePostService.UpdateServicePostCategoryAsync(servicePostCategory);               

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync(ResourceNames.FIELDNAME_SERVICEPOST_CATEGORYUPDATE));

                if (!continueEditing)
                    return RedirectToAction("Categories");

                return RedirectToAction("ServicePostCategoryEdit", new { id = servicePostCategory.Id });
            }

            //prepare model
            model = await _servicePostAdminModelFactory.PrepareServicePostCategoryModelAsync(model, servicePostCategory);

            //if we got this far, something failed, redisplay form
            return View($"{Constants.PLUGIN_ADMIN_VIEW_PATH}/ServicePostCategory/Edit.cshtml", model);
        }

        #region List


        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> Categories()
        {           
            //prepare model
            var model = await _servicePostAdminModelFactory.PrepareServicePostCategorySearchModelAsync(new ServicePostCategorySearchModel());

            return View($"{Constants.PLUGIN_ADMIN_VIEW_PATH}/ServicePostCategory/Categories.cshtml", model);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> Categories(ServicePostCategorySearchModel searchModel)
        {            
            //prepare model
            var model = await _servicePostAdminModelFactory.PrepareServicePostCategoryListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {           
            if (selectedIds != null)
            {
                await _servicePostService.DeleteServicePostCategoriesAsync(await (await _servicePostService.GetServicePostCategoriesByIdsAsync(selectedIds.ToArray())).WhereAwait(async p => await _workContext.GetCurrentVendorAsync() == null).ToListAsync());
            }

            return Json(new { Result = true });
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> Delete(int id)
        {            
            //try to get a servicepost category with the specified id
            var servicePostCategory = await _servicePostService.GetServicePostCategoryByIdAsync(id);
            if (servicePostCategory == null)
                return RedirectToAction("Categories");
            

            await _servicePostService.DeleteServicePostCategoryAsync(servicePostCategory);

           
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync(ResourceNames.SERVICEPOSTCATEGORY_DELETE));

            return RedirectToAction("Categories");
        }

       

       
        #endregion

        #endregion
    }
}