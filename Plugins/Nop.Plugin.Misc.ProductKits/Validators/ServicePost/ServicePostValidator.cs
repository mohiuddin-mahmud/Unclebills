using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using Nop.Web.Models.Blogs;
using NopValley.Plugins.Misc.Areas.Admin.Models;
using NopValley.Plugins.Misc.Areas.Admin.Models.ServicePost;

namespace NopValley.Plugins.Misc.Validators.ServicePost;

public partial class ServicePostValidator : BaseNopValidator<ServicePostModel>
{
    public ServicePostValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.Name).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("ServicePost.Name.Required")).When(x => x.Name == null);
    }
}