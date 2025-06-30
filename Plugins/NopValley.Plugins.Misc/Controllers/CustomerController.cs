using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Core.Http;
using Nop.Core.Http.Extensions;
using Nop.Services.Attributes;
using Nop.Services.Authentication;
using Nop.Services.Authentication.External;
using Nop.Services.Authentication.MultiFactor;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.ExportImport;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Tax;
using Nop.Web.Controllers;
using Nop.Web.Factories;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Validators;
using Nop.Web.Models.Customer;
using NopValley.Plugins.Misc.Models.Customer;
using ILogger = Nop.Services.Logging.ILogger;

namespace NopValley.Plugins.Misc.Controllers;

[AutoValidateAntiforgeryToken]
public partial class CustomerController : BasePublicController
{
    #region Fields

    protected readonly AddressSettings _addressSettings;
    protected readonly CaptchaSettings _captchaSettings;
    protected readonly CustomerSettings _customerSettings;
    protected readonly DateTimeSettings _dateTimeSettings;
    protected readonly ForumSettings _forumSettings;
    protected readonly GdprSettings _gdprSettings;
    protected readonly HtmlEncoder _htmlEncoder;
    protected readonly IAddressModelFactory _addressModelFactory;
    protected readonly IAddressService _addressService;
    protected readonly IAttributeParser<AddressAttribute, AddressAttributeValue> _addressAttributeParser;
    protected readonly IAttributeParser<CustomerAttribute, CustomerAttributeValue> _customerAttributeParser;
    protected readonly IAttributeService<CustomerAttribute, CustomerAttributeValue> _customerAttributeService;
    protected readonly IAuthenticationService _authenticationService;
    protected readonly ICountryService _countryService;
    protected readonly ICurrencyService _currencyService;
    protected readonly ICustomerActivityService _customerActivityService;
    protected readonly ICustomerModelFactory _customerModelFactory;
    protected readonly ICustomerRegistrationService _customerRegistrationService;
    protected readonly ICustomerService _customerService;
    protected readonly IDownloadService _downloadService;
    protected readonly IEventPublisher _eventPublisher;
    protected readonly IExportManager _exportManager;
    protected readonly IExternalAuthenticationService _externalAuthenticationService;
    protected readonly IGdprService _gdprService;
    protected readonly IGenericAttributeService _genericAttributeService;
    protected readonly IGiftCardService _giftCardService;
    protected readonly ILocalizationService _localizationService;
    protected readonly ILogger _logger;
    protected readonly IMultiFactorAuthenticationPluginManager _multiFactorAuthenticationPluginManager;
    protected readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
    protected readonly INotificationService _notificationService;
    protected readonly IOrderService _orderService;
    protected readonly IPermissionService _permissionService;
    protected readonly IPictureService _pictureService;
    protected readonly IPriceFormatter _priceFormatter;
    protected readonly IProductService _productService;
    protected readonly IStateProvinceService _stateProvinceService;
    protected readonly IStoreContext _storeContext;
    protected readonly ITaxService _taxService;
    protected readonly IWorkContext _workContext;
    protected readonly IWorkflowMessageService _workflowMessageService;
    protected readonly LocalizationSettings _localizationSettings;
    protected readonly MediaSettings _mediaSettings;
    protected readonly MultiFactorAuthenticationSettings _multiFactorAuthenticationSettings;
    protected readonly StoreInformationSettings _storeInformationSettings;
    protected readonly TaxSettings _taxSettings;
    private static readonly char[] _separator = [','];

    protected readonly IHttpContextAccessor _httpContextAccessor;


    #endregion

    #region Ctor

    public CustomerController(AddressSettings addressSettings,
        CaptchaSettings captchaSettings,
        CustomerSettings customerSettings,
        DateTimeSettings dateTimeSettings,
        ForumSettings forumSettings,
        GdprSettings gdprSettings,
        HtmlEncoder htmlEncoder,
        IAddressModelFactory addressModelFactory,
        IAddressService addressService,
        IAttributeParser<AddressAttribute, AddressAttributeValue> addressAttributeParser,
        IAttributeParser<CustomerAttribute, CustomerAttributeValue> customerAttributeParser,
        IAttributeService<CustomerAttribute, CustomerAttributeValue> customerAttributeService,
        IAuthenticationService authenticationService,
        ICountryService countryService,
        ICurrencyService currencyService,
        ICustomerActivityService customerActivityService,
        ICustomerModelFactory customerModelFactory,
        ICustomerRegistrationService customerRegistrationService,
        ICustomerService customerService,
        IDownloadService downloadService,
        IEventPublisher eventPublisher,
        IExportManager exportManager,
        IExternalAuthenticationService externalAuthenticationService,
        IGdprService gdprService,
        IGenericAttributeService genericAttributeService,
        IGiftCardService giftCardService,
        ILocalizationService localizationService,
        ILogger logger,
        IMultiFactorAuthenticationPluginManager multiFactorAuthenticationPluginManager,
        INewsLetterSubscriptionService newsLetterSubscriptionService,
        INotificationService notificationService,
        IOrderService orderService,
        IPermissionService permissionService,
        IPictureService pictureService,
        IPriceFormatter priceFormatter,
        IProductService productService,
        IStateProvinceService stateProvinceService,
        IStoreContext storeContext,
        ITaxService taxService,
        IWorkContext workContext,
        IWorkflowMessageService workflowMessageService,
        LocalizationSettings localizationSettings,
        MediaSettings mediaSettings,
        MultiFactorAuthenticationSettings multiFactorAuthenticationSettings,
        StoreInformationSettings storeInformationSettings,
        TaxSettings taxSettings,
        IHttpContextAccessor httpContextAccessor)
    {
        _addressSettings = addressSettings;
        _captchaSettings = captchaSettings;
        _customerSettings = customerSettings;
        _dateTimeSettings = dateTimeSettings;
        _forumSettings = forumSettings;
        _gdprSettings = gdprSettings;
        _htmlEncoder = htmlEncoder;
        _addressModelFactory = addressModelFactory;
        _addressService = addressService;
        _addressAttributeParser = addressAttributeParser;
        _customerAttributeParser = customerAttributeParser;
        _customerAttributeService = customerAttributeService;
        _authenticationService = authenticationService;
        _countryService = countryService;
        _currencyService = currencyService;
        _customerActivityService = customerActivityService;
        _customerModelFactory = customerModelFactory;
        _customerRegistrationService = customerRegistrationService;
        _customerService = customerService;
        _downloadService = downloadService;
        _eventPublisher = eventPublisher;
        _exportManager = exportManager;
        _externalAuthenticationService = externalAuthenticationService;
        _gdprService = gdprService;
        _genericAttributeService = genericAttributeService;
        _giftCardService = giftCardService;
        _localizationService = localizationService;
        _logger = logger;
        _multiFactorAuthenticationPluginManager = multiFactorAuthenticationPluginManager;
        _newsLetterSubscriptionService = newsLetterSubscriptionService;
        _notificationService = notificationService;
        _orderService = orderService;
        _permissionService = permissionService;
        _pictureService = pictureService;
        _priceFormatter = priceFormatter;
        _productService = productService;
        _stateProvinceService = stateProvinceService;
        _storeContext = storeContext;
        _taxService = taxService;
        _workContext = workContext;
        _workflowMessageService = workflowMessageService;
        _localizationSettings = localizationSettings;
        _mediaSettings = mediaSettings;
        _multiFactorAuthenticationSettings = multiFactorAuthenticationSettings;
        _storeInformationSettings = storeInformationSettings;
        _taxSettings = taxSettings;
        _httpContextAccessor = httpContextAccessor;
    }

    #endregion



    #region Methods




    #region Register

    //available even when navigation is not allowed
    [CheckAccessPublicStore(ignore: true)]
    public virtual async Task<IActionResult> PreRegisterAsync()
    {
        //check whether registration is allowed
        if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
            return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Disabled });

        var model = new PreRegisterModel();
        Task.Yield();   
        return View(model);
    }

    [HttpPost]
    [ValidateCaptcha]
    [ValidateHoneypot]
    //available even when navigation is not allowed
    [CheckAccessPublicStore(ignore: true)]
    public virtual async Task<IActionResult> PreRegister(PreRegisterModel model, string returnUrl, FormCollection form)
    {
        //check whether registration is allowed
        if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
            return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Disabled });

        // search for customer by rewards number
       // Customer customer = await _customerService.GetCustomerByRewardsCardAsync(model.cpExtraValueCardNumber, model.PhoneLast4);

        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer == null)
        {
            ModelState.AddModelError("", "No matching records for Extra Value Card Rewards Number and Last 4 of Phone Number. Please check numbers and try again or register for a new Extra Value Card.");
        }

        if (ModelState.IsValid)
        {
            _httpContextAccessor.HttpContext.Session.SetString("rewardsCustomerId", customer.Id.ToString());
            _httpContextAccessor.HttpContext.Session.SetString("rewardsCardNumber", model.cpExtraValueCardNumber);

           
            return RedirectToRoute("Register", new { returnUrl = returnUrl });
        }

        //If we got this far, something failed, redisplay form
        model = new PreRegisterModel() { cpExtraValueCardNumber = model.cpExtraValueCardNumber, PhoneLast4 = model.PhoneLast4 };
        return View(model);
    }


    //available even when navigation is not allowed
    [CheckAccessPublicStore(ignore: true)]
    public virtual async Task<IActionResult> RegisterResult(int resultId, string returnUrl)
    {
        if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
            returnUrl = Url.RouteUrl("Homepage");

        var model = await _customerModelFactory.PrepareRegisterResultModelAsync(resultId, returnUrl);
        return View(model);
    }

    [HttpPost]
    //available even when navigation is not allowed
    [CheckAccessPublicStore(ignore: true)]
    public virtual async Task<IActionResult> CheckUsernameAvailability(string username)
    {
        var usernameAvailable = false;
        var statusText = await _localizationService.GetResourceAsync("Account.CheckUsernameAvailability.NotAvailable");

        if (!UsernamePropertyValidator<string, string>.IsValid(username, _customerSettings))
        {
            statusText = await _localizationService.GetResourceAsync("Account.Fields.Username.NotValid");
        }
        else if (_customerSettings.UsernamesEnabled && !string.IsNullOrWhiteSpace(username))
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            if (currentCustomer != null &&
                currentCustomer.Username != null &&
                currentCustomer.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase))
            {
                statusText = await _localizationService.GetResourceAsync("Account.CheckUsernameAvailability.CurrentUsername");
            }
            else
            {
                var customer = await _customerService.GetCustomerByUsernameAsync(username);
                if (customer == null)
                {
                    statusText = await _localizationService.GetResourceAsync("Account.CheckUsernameAvailability.Available");
                    usernameAvailable = true;
                }
            }
        }

        return Json(new { Available = usernameAvailable, Text = statusText });
    }

    //available even when navigation is not allowed
    [CheckAccessPublicStore(ignore: true)]
    public virtual async Task<IActionResult> AccountActivation(string token, string email, Guid guid)
    {
        //For backward compatibility with previous versions where email was used as a parameter in the URL
        var customer = await _customerService.GetCustomerByEmailAsync(email)
                       ?? await _customerService.GetCustomerByGuidAsync(guid);

        if (customer == null)
            return RedirectToRoute("Homepage");

        var model = new AccountActivationModel { ReturnUrl = Url.RouteUrl("Homepage") };
        var cToken = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.AccountActivationTokenAttribute);
        if (string.IsNullOrEmpty(cToken))
        {
            model.Result = await _localizationService.GetResourceAsync("Account.AccountActivation.AlreadyActivated");
            return View(model);
        }

        if (!cToken.Equals(token, StringComparison.InvariantCultureIgnoreCase))
            return RedirectToRoute("Homepage");

        //activate user account
        customer.Active = true;
        await _customerService.UpdateCustomerAsync(customer);
        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.AccountActivationTokenAttribute, "");

        //send welcome message
        await _workflowMessageService.SendCustomerWelcomeMessageAsync(customer, (await _workContext.GetWorkingLanguageAsync()).Id);

        //raise event       
        await _eventPublisher.PublishAsync(new CustomerActivatedEvent(customer));

        //authenticate customer after activation
        await _customerRegistrationService.SignInCustomerAsync(customer, null, true);

        //activating newsletter if need
        var store = await _storeContext.GetCurrentStoreAsync();
        var newsletter = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(customer.Email, store.Id);
        if (newsletter != null && !newsletter.Active)
        {
            newsletter.Active = true;
            await _newsLetterSubscriptionService.UpdateNewsLetterSubscriptionAsync(newsletter);
        }

        model.Result = await _localizationService.GetResourceAsync("Account.AccountActivation.Activated");
        return View(model);
    }

    #endregion

   

    #endregion
}