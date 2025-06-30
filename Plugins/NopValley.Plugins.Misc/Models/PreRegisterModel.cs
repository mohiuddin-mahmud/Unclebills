using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
namespace NopValley.Plugins.Misc.Models.Customer;

public partial record PreRegisterModel : BaseNopModel
{
    public PreRegisterModel() { }

    [NopResourceDisplayName("Account.Fields.cpExtraValueCardNumber")]
    //[AllowHtml]
    public string cpExtraValueCardNumber { get; set; }

    [NopResourceDisplayName("Account.Fields.PhoneLast4")]
    //[AllowHtml]
    public string PhoneLast4 { get; set; }
}