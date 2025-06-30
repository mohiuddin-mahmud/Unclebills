using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Events;
using NopValley.Plugins.Misc.Domain;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using NopValley.Plugins.Misc.Areas.Admin.Services;
using NopValley.Plugins.Misc.Areas.Admin.Factories;
using NopValley.Plugins.Misc.Areas.Admin.Models.ServicePost;
using Nop.Services.Media;
using Nop.Web.Framework;
using Nop.Web.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Http;
using Nop.Core.Domain.Catalog;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Mvc;

namespace NopValley.Plugins.Misc.Areas.Admin.Controllers
{
    //[AuthorizeAdmin]
    //[Area(AreaNames.ADMIN)]
    //[AutoValidateAntiforgeryToken]
    public class ServicePostController : BaseAdminController
    {
        #region Fields

        private readonly IBlogModelFactory _blogModelFactory;
        private readonly IServicePostAdminModelFactory _servicePostAdminModelFactory;
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
        protected readonly ILocalizedEntityService _localizedEntityService;
        protected readonly IPictureService _pictureService;
        public string viewPath = "~/Plugins/NopValley.Misc/Areas/Admin/Views/ServicePost/";

        #endregion

        #region Ctor

        public ServicePostController(           
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
            IServicePostAdminModelFactory servicePostAdminModelFactory,
            ILocalizedEntityService localizedEntityService,
            IPictureService pictureService) 
        {           
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
            _localizedEntityService = localizedEntityService;
            _pictureService = pictureService;
        }

        #endregion



        #region Service posts

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            //prepare model
            var model = await _servicePostAdminModelFactory.PrepareServicePostSearchModelAsync(new ServicePostSearchModel());

            return View(viewPath + "ServicePosts.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(ServicePostSearchModel searchModel)
        {            
            //prepare model
            var model = await _servicePostAdminModelFactory.PrepareServicePostListModelAsync(searchModel);

            return Json(model);
        }

        #endregion

        #region Create / Edit / Delete

        public virtual async Task<IActionResult> Create()
        {           
            //prepare model
            var model = await _servicePostAdminModelFactory.PrepareServicePostModelAsync(new ServicePostModel(), null);

            return View(viewPath + "Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(ServicePostModel model, bool continueEditing)
        {
           
            if (ModelState.IsValid)
            {
                var servicepost = model.ToEntity<ServicePost>();
                servicepost.ServicePostCategoryId = model.CategoryId;
                servicepost.Body = model.Body;
                servicepost.Name = model.Name;
                await _servicePostService.InsertServicePostAsync(servicepost);

                //search engine name
                model.SeName = await _urlRecordService.ValidateSeNameAsync(servicepost, model.SeName, servicepost.Name, true);
                await _urlRecordService.SaveSlugAsync(servicepost, model.SeName, 0);

                //locales
                await UpdateLocalesAsync(servicepost, model);

             

                await _servicePostService.UpdateServicePostAsync(servicepost);

                //update picture seo file name
                await UpdatePictureSeoNamesAsync(servicepost);

     

                //activity log
                await _customerActivityService.InsertActivityAsync("AddNewCategory",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewCategory"), servicepost.Name), servicepost);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Categories.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = servicepost.Id });
            }

            //prepare model
            model = await _servicePostAdminModelFactory.PrepareServicePostModelAsync(model, null);

            //if we got this far, something failed, redisplay form
            return View(viewPath + "Create.cshtml", model);
        }
        //protected virtual async Task UpdatePictureSeoNamesAsync(ServicePost servicepost)
        //{
        //    var picture = await _pictureService.GetPictureByIdAsync(servicepost.PictureId);
        //    if (picture != null)
        //        await _pictureService.SetSeoFilenameAsync(picture.Id, await _pictureService.GetPictureSeNameAsync(servicepost.Name));
        //}


        protected virtual async Task UpdatePictureSeoNamesAsync(ServicePost servicepost)
        {
            foreach (var pp in await _servicePostService.GetServicePostPicturesByProductIdAsync(servicepost.Id))
                await _pictureService.SetSeoFilenameAsync(pp.PictureId, await _pictureService.GetPictureSeNameAsync(servicepost.Name));
        }



        protected virtual async Task UpdateLocalesAsync(ServicePost servicepost, ServicePostModel model)
        {
            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(servicepost,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(servicepost,
                    x => x.Body,
                    localized.Body,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(servicepost,
                    x => x.MetaKeywords,
                    localized.MetaKeywords,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(servicepost,
                    x => x.MetaDescription,
                    localized.MetaDescription,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(servicepost,
                    x => x.MetaTitle,
                    localized.MetaTitle,
                    localized.LanguageId);

                //search engine name
                var seName = await _urlRecordService.ValidateSeNameAsync(servicepost, localized.SeName, localized.Name, false);
                await _urlRecordService.SaveSlugAsync(servicepost, seName, localized.LanguageId);
            }
        }
        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            //try to get a category with the specified id
            var servicepost = await _servicePostService.GetServicePostByIdAsync(id);
            if (servicepost == null)
                return RedirectToAction("List");

            //prepare model
            var model = await _servicePostAdminModelFactory.PrepareServicePostModelAsync(null, servicepost);

            return View(viewPath + "Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(ServicePostModel model, bool continueEditing)
        {
            //try to get a servicepost with the specified id
            var servicepost = await _servicePostService.GetServicePostByIdAsync(model.Id);
            if (servicepost == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var prevPictureId = servicepost.PictureId;
                servicepost = model.ToEntity(servicepost);                
                await _servicePostService.UpdateServicePostAsync(servicepost);

                //search engine name
                model.SeName = await _urlRecordService.ValidateSeNameAsync(servicepost, model.SeName, servicepost.Name, true);
                await _urlRecordService.SaveSlugAsync(servicepost, model.SeName, 0);

                //locales
                await UpdateLocalesAsync(servicepost, model);               

                await _servicePostService.UpdateServicePostAsync(servicepost);

                //delete an old picture (if deleted or updated)
                if (prevPictureId > 0 && prevPictureId != servicepost.PictureId)
                {
                    var prevPicture = await _pictureService.GetPictureByIdAsync(prevPictureId);
                    if (prevPicture != null)
                        await _pictureService.DeletePictureAsync(prevPicture);
                }

                //update picture seo file name
                await UpdatePictureSeoNamesAsync(servicepost);
               
                //activity log
                await _customerActivityService.InsertActivityAsync("EditCategory",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditCategory"), servicepost.Name), servicepost);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Categories.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = servicepost.Id });
            }

            //prepare model
            model = await _servicePostAdminModelFactory.PrepareServicePostModelAsync(model, servicepost);

            //if we got this far, something failed, redisplay form
            return View(viewPath + "Edit.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            //try to get a servicepost with the specified id
            var servicepost = await _servicePostService.GetServicePostByIdAsync(id);
            if (servicepost == null)
                return RedirectToAction("List");

            await _servicePostService.DeleteServicePostAsync(servicepost);

            //activity log
            await _customerActivityService.InsertActivityAsync("DeleteCategory",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteCategory"), servicepost.Name), servicepost);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Categories.Deleted"));

            return RedirectToAction("List");
        }

        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return await AccessDeniedDataTablesJson();

            if (selectedIds == null || !selectedIds.Any())
                return NoContent();

            await _servicePostService.DeleteServicePostsAsync(await (await _servicePostService.GetServicePostsByIdsAsync(selectedIds.ToArray())).WhereAwait(async p => await _workContext.GetCurrentVendorAsync() == null).ToListAsync());

            return Json(new { Result = true });
        }

        #endregion


        #region Service pictures

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> ServicePostPictureAdd(int servicePostId, IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (servicePostId == 0)
                throw new ArgumentException();

            //try to get a product with the specified id
            var servicePost = await _servicePostService.GetServicePostByIdAsync(servicePostId)
                ?? throw new ArgumentException("No service found with the specified id");

            var files = form.Files.ToList();
            if (!files.Any())
                return Json(new { success = false });
           
            try
            {
                foreach (var file in files)
                {
                    //insert picture
                    var picture = await _pictureService.InsertPictureAsync(file);

                    await _pictureService.SetSeoFilenameAsync(picture.Id, await _pictureService.GetPictureSeNameAsync(servicePost.Name));

                    await _servicePostService.InsertServicePostPictureAsync(new ServicePostPicture
                    {
                        PictureId = picture.Id,
                        ServicePostId = servicePost.Id,
                        DisplayOrder = 0
                    });
                }
            }
            catch (Exception exc)
            {
                return Json(new
                {
                    success = false,
                    message = $"{await _localizationService.GetResourceAsync("Admin.Catalog.Products.Multimedia.Pictures.Alert.PictureAdd")} {exc.Message}",
                });
            }

            return Json(new { success = true });
        }

        [HttpPost]
        public virtual async Task<IActionResult> ServicePostPictureList(ServicePostPictureSearchModel searchModel)
        {
           

            //try to get a servicePost with the specified id
            var servicePost = await _servicePostService.GetServicePostByIdAsync(searchModel.ServicePostId)
                ?? throw new ArgumentException("No product found with the specified id");


            //prepare model
            var model = await _servicePostAdminModelFactory.PrepareServicePostPictureListModelAsync(searchModel, servicePost);

            return Json(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ServicePostPictureUpdate(ServicePostPictureModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return await AccessDeniedDataTablesJson();

            //try to get a servicepost picture with the specified id
            var servicePostPicture = await _servicePostService.GetServicePostPictureByIdAsync(model.Id)
                ?? throw new ArgumentException("No product picture found with the specified id");

           

            //try to get a picture with the specified id
            var picture = await _pictureService.GetPictureByIdAsync(servicePostPicture.PictureId)
                ?? throw new ArgumentException("No picture found with the specified id");

            await _pictureService.UpdatePictureAsync(picture.Id,
                await _pictureService.LoadPictureBinaryAsync(picture),
                picture.MimeType,
                picture.SeoFilename,
                model.OverrideAltAttribute,
                model.OverrideTitleAttribute);

            servicePostPicture.DisplayOrder = model.DisplayOrder;
            await _servicePostService.UpdateServicePostPictureAsync(servicePostPicture);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual async Task<IActionResult> ServicePostPictureDelete(int id)
        {
        
            //try to get a product picture with the specified id
            var servicePostPicture = await _servicePostService.GetServicePostPictureByIdAsync(id)
                ?? throw new ArgumentException("No product picture found with the specified id");
          

            var pictureId = servicePostPicture.PictureId;
            await _servicePostService.DeleteServicePostPictureAsync(servicePostPicture);

            //try to get a picture with the specified id
            var picture = await _pictureService.GetPictureByIdAsync(pictureId)
                ?? throw new ArgumentException("No picture found with the specified id");

            await _pictureService.DeletePictureAsync(picture);

            return new NullJsonResult();
        }

        #endregion
    }
}