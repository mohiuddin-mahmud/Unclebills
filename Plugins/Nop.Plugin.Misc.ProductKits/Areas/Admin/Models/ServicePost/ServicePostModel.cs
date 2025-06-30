using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopValley.Plugins.Misc.Areas.Admin.Models.ServicePost;

/// <summary>
/// Represents a blog post model
/// </summary>
public partial record ServicePostModel : BaseNopEntityModel, ILocalizedModel<ServicePostLocalizedModel>
{
    #region Ctor

    public ServicePostModel()
    {
        if (PageSize < 1)
        {
            PageSize = 5;
        }

        Locales = new List<ServicePostLocalizedModel>();
        AvailableLanguages = new List<SelectListItem>();
        AvailableCategories = new List<SelectListItem>();
        AddPictureModel = new ServicePostPictureModel();
        ServicePostPictureModels = new List<ServicePostPictureModel>();
        ServicePostPictureSearchModel = new ServicePostPictureSearchModel();
    }
    
    #endregion

    #region Properties

    [NopResourceDisplayName("Admin.ContentManagement.ServicePost.Fields.CategoryId")]
    public int CategoryId { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.ServicePost.Fields.Language")]
    public int LanguageId { get; set; }
    public IList<SelectListItem> AvailableLanguages { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.ServicePost.Fields.Language")]
    public string LanguageName { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.ServicePost.Fields.Title")]
    public string Name { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.ServicePost.Fields.Body")]
    public string Body { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.ServicePost.Fields.BodyOverview")]
    public string BodyOverview { get; set; }
    
    public DateTime? EndDateUtc { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.ServicePost.Fields.MetaKeywords")]
    public string MetaKeywords { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.ServicePost.Fields.MetaDescription")]
    public string MetaDescription { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.ServicePost.Fields.MetaTitle")]
    public string MetaTitle { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.ServicePost.Fields.SeName")]
    public string SeName { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.ServicePost.Fields.CreatedOn")]
    public DateTime CreatedOn { get; set; }


    [NopResourceDisplayName("Admin.Catalog.Categories.Fields.PageSize")]
    public int PageSize { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Categories.Fields.AllowCustomersToSelectPageSize")]
    public bool AllowCustomersToSelectPageSize { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Categories.Fields.PageSizeOptions")]
    public string PageSizeOptions { get; set; }

    public IList<ServicePostLocalizedModel> Locales { get; set; }
    public IList<SelectListItem> AvailableCategories { get; set; }

    [UIHint("Picture")]
    [NopResourceDisplayName("Admin.Catalog.Categories.Fields.Picture")]
    public int PictureId { get; set; }


    public ServicePostPictureSearchModel ServicePostPictureSearchModel { get; set; }

    //pictures
    public ServicePostPictureModel AddPictureModel { get; set; }
    public IList<ServicePostPictureModel> ServicePostPictureModels { get; set; }

    #endregion
}

public partial record ServicePostLocalizedModel : ILocalizedLocaleModel
{
    public int LanguageId { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Categories.Fields.Name")]
    public string Name { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Categories.Fields.Description")]
    public string Body { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Categories.Fields.MetaKeywords")]
    public string MetaKeywords { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Categories.Fields.MetaDescription")]
    public string MetaDescription { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Categories.Fields.MetaTitle")]
    public string MetaTitle { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Categories.Fields.SeName")]
    public string SeName { get; set; }
}