using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Topics;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Services.Topics;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Topics;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Filters;
using NopValley.Plugins.Misc.Areas.Admin.Models;
using NopValley.Plugins.Misc.Domain;
using NopValley.Plugins.Misc.Services;
using NopValley.Plugins.Misc.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Mvc;
using DocumentFormat.OpenXml.Presentation;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Validators;
using Nop.Core.Http;
using System.Net.Http;

namespace NopValley.Plugins.Misc.Areas.Admin.Controllers
{
    public partial class SectionController : BaseAdminController
    {
        #region Fields

     
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;  
        private readonly IHtmlWidgetModelFactory _htmlWidgetModelFactory;     
        private readonly IHtmlWidgetService _htmlWidgetService;
        private readonly IPictureService _pictureService;
        private readonly ISectionService _sectionService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        public string viewPath = "~/Plugins/References/Areas/Admin/Views/HtmlWidgety/";

        #endregion Fields

        #region Ctor

        public SectionController(
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IHtmlWidgetModelFactory htmlWidgetModelFactory,
            ITopicService topicService,
            IHtmlWidgetService htmlWidgetService,
            IUrlRecordService urlRecordService,
            IGenericAttributeService genericAttributeService,
            IWorkContext workContext,
            IPictureService pictureService,
            ISectionService sectionService,
            IHttpClientFactory httpClientFactory,
            IWebHelper webHelper)
        {
          
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _htmlWidgetModelFactory = htmlWidgetModelFactory;
            _htmlWidgetService = htmlWidgetService;
            _pictureService = pictureService;
            _sectionService = sectionService;
            _httpClientFactory = httpClientFactory;
            _webHelper= webHelper;

        }

        #endregion

        #region Utilities
        protected virtual async Task UpdateLocalesAsync(Section section, HtmlWidgetSectionModel model)
        {
            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(section,
                    x => x.Title,
                    localized.Title,
                    localized.LanguageId);
            }
        }
        #endregion
        #region List

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {            

            //try to get a section with the specified id
            var section = await _sectionService.GetSectionByIdAsync(id);
            if (section == null)
                return RedirectToRoute("References.HtmlWidgety.List");

            if (section != null)
            {
                await _sectionService.DeleteSectionAsync(section);
                await _sectionService.ClearSectionCacheAsync();
            }          
          
            return RedirectToRoute("References.HtmlWidgety.List");
        }


        [HttpPost]
        public virtual async Task<IActionResult> SectionDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageActivityLog))
                return AccessDeniedView();


            //try to get a section with the specified id
            var section = await _sectionService.GetSectionByIdAsync(id);
            if (section == null)
                return RedirectToRoute("References.HtmlWidgety.List");

            if (section != null)
            {
                await _sectionService.DeleteSectionAsync(section);
                await _sectionService.ClearSectionCacheAsync();
            }            

            return new NullJsonResult();
        }
		#endregion


		[HttpPost]
		public virtual async Task<IActionResult> Update(HtmlWidgetSectionModel model)
		{
			var section = await _sectionService.GetSectionByIdAsync(model.Id);
			section.DisplayOrder = model.DisplayOrder;
			await _sectionService.UpdateSectionAsync(section);
			return new NullJsonResult();
		}

		#region Section
		public virtual async Task<IActionResult> HtmlWidgetySectionAddOrEdit(int htmlWidgetId, int sectionId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (await _htmlWidgetService.GetHtmlWidgetByIdAsync(htmlWidgetId) == null)
            {
                _notificationService.ErrorNotification("No case study found with the specified id");
                return RedirectToAction("List");
            }

            //try to get a section with the specified id
            try
            {
                var model = await _htmlWidgetModelFactory.PrepareAddSectionModelAsync(htmlWidgetId, sectionId);
                //var model = await _htmlWidgetModelFactory.PrepareHtmlWidgetySectionModelAsync();
                return View(viewPath + "HtmlWidgetySectionAddOrEdit.cshtml", model);
            }
            catch (Exception ex)
            {
                await _notificationService.ErrorNotificationAsync(ex);

                //select an appropriate card
                SaveSelectedCardName("product-specification-attributes");
                return RedirectToAction("Edit", new { id = htmlWidgetId });
            }
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> HtmlWidgetySectionAddOrEdit(HtmlWidgetSectionModel model, bool continueEditing)
        {
            var htmlWidgetSection = new Section();           


            if (model.Id >  0)
            {
                //try to get a topic with the specified id
                htmlWidgetSection = await _sectionService.GetSectionByIdAsync(model.Id);
                if (htmlWidgetSection == null)
                    return RedirectToRoute("References.HtmlWidgety.Edit", new { id = model.HtmlWidgetId });
            }
           

            if (ModelState.IsValid)
            {

                if (model.Id == 0)
                {
                    htmlWidgetSection = model.ToEntity(htmlWidgetSection);
                  
                    await _sectionService.InsertSectionAsync(htmlWidgetSection);

					_notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Widgets.References.HtmlWidgeties.Section.Added"));


					model.Id = htmlWidgetSection.Id;   
                    model.HtmlWidgetId = htmlWidgetSection.HtmlWidgetId;

                    await _sectionService.ClearSectionCacheAsync();

                    var sectionBackgroundId = htmlWidgetSection.BackgroundPictureId;
                   

                    //delete an old picture (if deleted or updated)
                    if (sectionBackgroundId > 0 && sectionBackgroundId != htmlWidgetSection.BackgroundPictureId)
                    {
                        var prevPicture = await _pictureService.GetPictureByIdAsync(sectionBackgroundId);
                        if (prevPicture != null)
                            await _pictureService.DeletePictureAsync(prevPicture);
                    }
                }              



                var prevBackgroundPictureId = htmlWidgetSection.BackgroundPictureId;

                htmlWidgetSection = model.ToEntity(htmlWidgetSection);

                //locales
                await UpdateLocalesAsync(htmlWidgetSection, model);

                await _sectionService.UpdateSectionAsync(htmlWidgetSection);

                //delete an old picture (if deleted or updated)
               

                if (prevBackgroundPictureId > 0 && prevBackgroundPictureId != htmlWidgetSection.BackgroundPictureId)
                {
                    var prevPicture = await _pictureService.GetPictureByIdAsync(prevBackgroundPictureId);
                    if (prevPicture != null)
                        await _pictureService.DeletePictureAsync(prevPicture);
                }
               

                //update picture seo file name
                await UpdateSectionPictureSeoNamesAsync(htmlWidgetSection);
                //if (!continueEditing)
                //    return RedirectToRoute("References.HtmlWidgety.List");

                return RedirectToRoute("References.HtmlWidgety.Section.Edit", new { htmlWidgetId = htmlWidgetSection.HtmlWidgetId, sectionId = htmlWidgetSection.Id });

                //return RedirectToRoute("References.HtmlWidgety.Edit", new { id = htmlWidgetSection.HtmlWidgetyId });

                //return RedirectToRoute("References.HtmlWidgety.Section.Edit", new { htmlWidgetId = htmlWidgetSection.HtmlWidgetyId, sectionId = htmlWidgetSection.Id });
            }

            //prepare model
            model = await _htmlWidgetModelFactory.PrepareHtmlWidgetSectionModelAsync(model, htmlWidgetSection);

            //if we got this far, something failed, redisplay form
            return View(viewPath + "HtmlWidgetySectionAddOrEdit.cshtml", model);
           
        }

        

        [HttpPost]
        public virtual async Task<IActionResult> HtmlWidgetySectionList(HtmlWidgetSectionSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return await AccessDeniedDataTablesJson();

            //try to get a product with the specified id
            var htmlWidget = await _htmlWidgetService.GetHtmlWidgetByIdAsync(searchModel.HtmlWidgetId)
                ?? throw new ArgumentException("No product found with the specified id");        

            //prepare model
            var model = await _htmlWidgetModelFactory.PrepareHtmlWidgetSectionListModelAsync(searchModel, htmlWidget);

            return Json(model);
        }
        protected virtual async Task UpdateSectionPictureSeoNamesAsync(Section section)
        {
            var logo = await _pictureService.GetPictureByIdAsync(section.BackgroundPictureId);
            if (logo != null)
                await _pictureService.SetSeoFilenameAsync(logo.Id, await _pictureService.GetPictureSeNameAsync(section.Title));


        }
        #endregion

         #region Product pictures

        public virtual async Task<IActionResult> GalleryPictureAdd(int pictureId, int displayOrder,
            string overrideAltAttribute, string overrideTitleAttribute, int sectionId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (pictureId == 0)
                throw new ArgumentException();

            //try to get a product with the specified id
            var section = await _sectionService.GetSectionByIdAsync(sectionId)
                ?? throw new ArgumentException("No section found with the specified id");

           

            if ((await _sectionService.GetGalleryPicturesBySectionIdAsync(sectionId)).Any(p => p.PictureId == pictureId))
                return Json(new { Result = false });

            //try to get a picture with the specified id
            var picture = await _pictureService.GetPictureByIdAsync(pictureId)
                ?? throw new ArgumentException("No picture found with the specified id");

            await _pictureService.UpdatePictureAsync(picture.Id,
                await _pictureService.LoadPictureBinaryAsync(picture),
                picture.MimeType,
                picture.SeoFilename,
                overrideAltAttribute,
                overrideTitleAttribute);

            await _pictureService.SetSeoFilenameAsync(pictureId, await _pictureService.GetPictureSeNameAsync(section.Title));

            await _sectionService.InsertGalleryPictureAsync(new GalleryPicture
            {
                PictureId = pictureId,
                SectionId = sectionId,
                DisplayOrder = displayOrder
            });

            return Json(new { Result = true });
        }

        [HttpPost]
        public virtual async Task<IActionResult> GalleryPictureList(GalleryPictureSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return await AccessDeniedDataTablesJson();

            //try to get a product with the specified id
            var section = await _sectionService.GetSectionByIdAsync(searchModel.SectionId)
                ?? throw new ArgumentException("No section found with the specified id");           

            //prepare model
            var model = await _htmlWidgetModelFactory.PrepareGalleryPictureListModelAsync(searchModel, section);

            return Json(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> GalleryPictureUpdate(GalleryPictureModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product picture with the specified id
            var GalleryPicture = await _sectionService.GetGalleryPictureByIdAsync(model.Id)
                ?? throw new ArgumentException("No product picture found with the specified id");



            if (GalleryPicture.PictureId > 0)
            {
                //try to get a picture with the specified id
                var picture = await _pictureService.GetPictureByIdAsync(GalleryPicture.PictureId)
                    ?? throw new ArgumentException("No picture found with the specified id");

                await _pictureService.UpdatePictureAsync(picture.Id,
                    await _pictureService.LoadPictureBinaryAsync(picture),
                    picture.MimeType,
                    picture.SeoFilename,
                    model.OverrideAltAttribute,
                    model.OverrideTitleAttribute);

                GalleryPicture.DisplayOrder = model.DisplayOrder;
                await _sectionService.UpdateGalleryPictureAsync(GalleryPicture);
            }
            else if (GalleryPicture.VideoId > 0)
            {
                var video = await _sectionService.GetGalleryVideoByIdAsync(GalleryPicture.VideoId);
                video.DisplayOrder = model.DisplayOrder;
                video.AltTitle = model.OverrideAltAttribute;                
                await _sectionService.UpdateVideoAsync(video);

                GalleryPicture.DisplayOrder = model.DisplayOrder;
                await _sectionService.UpdateGalleryPictureAsync(GalleryPicture);

            }

            

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual async Task<IActionResult> GalleryPictureDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a section picture with the specified id
            var GalleryPicture = await _sectionService.GetGalleryPictureByIdAsync(id)
                ?? throw new ArgumentException("No product picture found with the specified id");

            if(GalleryPicture.PictureId > 0)
            {
                var pictureId = GalleryPicture.PictureId;
                await _sectionService.DeleteGalleryPictureAsync(GalleryPicture);

                //try to get a picture with the specified id
                var picture = await _pictureService.GetPictureByIdAsync(pictureId)
                    ?? throw new ArgumentException("No picture found with the specified id");

                await _pictureService.DeletePictureAsync(picture);
            }
            else if (GalleryPicture.VideoId > 0)
            {
                var video = await _sectionService.GetGalleryVideoByIdAsync(GalleryPicture.VideoId);
                await _sectionService.DeleteVideoAsync(video);
                await _sectionService.DeleteGalleryPictureAsync(GalleryPicture);
            }

            return new NullJsonResult();
        }

        #endregion


        #region Product videos

        //[HttpPost]
        //public virtual async Task<IActionResult> ProductVideoAdd(int SectionId, [Validate] ProductVideoModel model)
        //{
        //    if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
        //        return AccessDeniedView();

        //    if (SectionId == 0)
        //        throw new ArgumentException();

        //    //try to get a product with the specified id
        //    var section = await _sectionService.GetSectionByIdAsync(SectionId)
        //        ?? throw new ArgumentException("No section found with the specified id");

        //    var videoUrl = model.VideoUrl.TrimStart('~');

        //    try
        //    {
        //        await PingVideoUrlAsync(videoUrl);
        //    }
        //    catch (Exception exc)
        //    {
        //        return Json(new
        //        {
        //            success = false,
        //            error = $"{await _localizationService.GetResourceAsync("Admin.Catalog.Products.Multimedia.Videos.Alert.VideoAdd")} {exc.Message}",
        //        });
        //    }

        //    if (!ModelState.IsValid)
        //        return ErrorJson(ModelState.SerializeErrors());
       
        //    try
        //    {
        //        var video = new GalleryVideo
        //        {
        //            VideoUrl = videoUrl,
        //            DisplayOrder = model.DisplayOrder,
        //            VideoTitle = model.VideoTitle,
        //            AltTitle = model.AltTitle
        //        };

        //        //insert video
        //        await _sectionService.InsertVideoAsync(video);              

        //        await _sectionService.InsertGalleryPictureAsync(new GalleryPicture
        //        {
        //            VideoId = video.Id,
        //            PictureId = 0,
        //            SectionId = section.Id,
        //            DisplayOrder = model.DisplayOrder
        //        });
        //    }
        //    catch (Exception exc)
        //    {
        //        return Json(new
        //        {
        //            success = false,
        //            error = $"{await _localizationService.GetResourceAsync("Admin.Catalog.Products.Multimedia.Videos.Alert.VideoAdd")} {exc.Message}",
        //        });
        //    }

        //    return Json(new { success = true });
        //}

        protected virtual async Task PingVideoUrlAsync(string videoUrl)
        {
            var path = videoUrl.StartsWith("/") ? $"{_webHelper.GetStoreLocation()}{videoUrl.TrimStart('/')}" : videoUrl;

            var client = _httpClientFactory.CreateClient(NopHttpDefaults.DefaultHttpClient);
            await client.GetStringAsync(path);
        }

        #endregion

        //[HttpPost]
        //public virtual async Task<IActionResult> ProductVideoUpdate([Validate] ProductVideoModel model)
        //{
        //    if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
        //        return AccessDeniedView();

        //    //try to get a product picture with the specified id
        //    var productVideo = await _productService.GetProductVideoByIdAsync(model.Id)
        //        ?? throw new ArgumentException("No product video found with the specified id");

        //    //a vendor should have access only to his products
        //    var currentVendor = await _workContext.GetCurrentVendorAsync();
        //    if (currentVendor != null)
        //    {
        //        var product = await _productService.GetProductByIdAsync(productVideo.ProductId);
        //        if (product != null && product.VendorId != currentVendor.Id)
        //            return Content("This is not your product");
        //    }

        //    //try to get a video with the specified id
        //    var video = await _videoService.GetVideoByIdAsync(productVideo.VideoId)
        //        ?? throw new ArgumentException("No video found with the specified id");

        //    var videoUrl = model.VideoUrl.TrimStart('~');

        //    try
        //    {
        //        await PingVideoUrlAsync(videoUrl);
        //    }
        //    catch (Exception exc)
        //    {
        //        return Json(new
        //        {
        //            success = false,
        //            error = $"{await _localizationService.GetResourceAsync("Admin.Catalog.Products.Multimedia.Videos.Alert.VideoUpdate")} {exc.Message}",
        //        });
        //    }

        //    video.VideoUrl = videoUrl;

        //    await _videoService.UpdateVideoAsync(video);

        //    productVideo.DisplayOrder = model.DisplayOrder;
        //    await _productService.UpdateProductVideoAsync(productVideo);

        //    return new NullJsonResult();
        //}
    }
}