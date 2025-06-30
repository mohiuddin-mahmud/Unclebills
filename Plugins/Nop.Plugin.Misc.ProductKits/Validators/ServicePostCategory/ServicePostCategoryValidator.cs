using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using Nop.Web.Models.Blogs;
using NopValley.Plugins.Misc.Areas.Admin.Models;

namespace NopValley.Plugins.Misc.Validators.ServicePostCategory;

public partial class ServicePostCategoryValidator : BaseNopValidator<ServicePostCategoryModel>
{
    public ServicePostCategoryValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.Name).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("NopValley.Plugin.Misc.ServicePostCategory.Name.Required")).When(x => x.Name == null);
    }
}