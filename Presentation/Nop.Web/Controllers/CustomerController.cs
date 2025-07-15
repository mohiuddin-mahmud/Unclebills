using System.Reflection.Metadata;
using System.Text.Encodings.Web;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
using Nop.Core.Domain.Orders;
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
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Validators;
using Nop.Web.Models.Boards;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Customer;
using ILogger = Nop.Services.Logging.ILogger;

namespace Nop.Web.Controllers;

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

    private readonly ICpDiscountService _cpDiscountService;
    private readonly ICpOrderService _cpOrderService;
    private readonly IVendorService _vendorService;
    private readonly IPetProfileService _petProfileService;

    private readonly ICustomerInfoChangeService _customerInfoChangeService;
    protected readonly IStoreMappingService _storeMappingService;

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
        ICpDiscountService cpDiscountService,
        ICpOrderService cpOrderService,
        IVendorService vendorService,
        IPetProfileService petProfileService,
        ICustomerInfoChangeService customerInfoChangeService,
        IStoreMappingService storeMappingService)
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
        _cpDiscountService = cpDiscountService;
        _cpOrderService = cpOrderService;
        _vendorService = vendorService;
        _petProfileService = petProfileService;
        _customerInfoChangeService = customerInfoChangeService;
        _storeMappingService = storeMappingService;
    }

    #endregion

    #region Utilities

    protected virtual void ValidateRequiredConsents(List<GdprConsent> consents, IFormCollection form)
    {
        foreach (var consent in consents)
        {
            var controlId = $"consent{consent.Id}";
            var cbConsent = form[controlId];
            if (StringValues.IsNullOrEmpty(cbConsent) || !cbConsent.ToString().Equals("on"))
            {
                ModelState.AddModelError("", consent.RequiredMessage);
            }
        }
    }

    protected virtual async Task<string> ParseSelectedProviderAsync(IFormCollection form)
    {
        ArgumentNullException.ThrowIfNull(form);

        var store = await _storeContext.GetCurrentStoreAsync();

        var multiFactorAuthenticationProviders = await _multiFactorAuthenticationPluginManager.LoadActivePluginsAsync(await _workContext.GetCurrentCustomerAsync(), store.Id);
        foreach (var provider in multiFactorAuthenticationProviders)
        {
            var controlId = $"provider_{provider.PluginDescriptor.SystemName}";

            var curProvider = form[controlId];
            if (!StringValues.IsNullOrEmpty(curProvider))
            {
                var selectedProvider = curProvider.ToString();
                if (!string.IsNullOrEmpty(selectedProvider))
                {
                    return selectedProvider;
                }
            }
        }
        return string.Empty;
    }

    protected virtual async Task<string> ParseCustomCustomerAttributesAsync(IFormCollection form)
    {
        ArgumentNullException.ThrowIfNull(form);

        var attributesXml = "";
        var attributes = await _customerAttributeService.GetAllAttributesAsync();
        foreach (var attribute in attributes)
        {
            var controlId = $"{NopCustomerServicesDefaults.CustomerAttributePrefix}{attribute.Id}";
            switch (attribute.AttributeControlType)
            {
                case AttributeControlType.DropdownList:
                case AttributeControlType.RadioList:
                {
                    var ctrlAttributes = form[controlId];
                    if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                    {
                        var selectedAttributeId = int.Parse(ctrlAttributes);
                        if (selectedAttributeId > 0)
                            attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                                attribute, selectedAttributeId.ToString());
                    }
                }
                    break;
                case AttributeControlType.Checkboxes:
                {
                    var cblAttributes = form[controlId];
                    if (!StringValues.IsNullOrEmpty(cblAttributes))
                    {
                        foreach (var item in cblAttributes.ToString().Split(_separator, StringSplitOptions.RemoveEmptyEntries))
                        {
                            var selectedAttributeId = int.Parse(item);
                            if (selectedAttributeId > 0)
                                attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                        }
                    }
                }
                    break;
                case AttributeControlType.ReadonlyCheckboxes:
                {
                    //load read-only (already server-side selected) values
                    var attributeValues = await _customerAttributeService.GetAttributeValuesAsync(attribute.Id);
                    foreach (var selectedAttributeId in attributeValues
                                 .Where(v => v.IsPreSelected)
                                 .Select(v => v.Id)
                                 .ToList())
                    {
                        attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                            attribute, selectedAttributeId.ToString());
                    }
                }
                    break;
                case AttributeControlType.TextBox:
                case AttributeControlType.MultilineTextbox:
                {
                    var ctrlAttributes = form[controlId];
                    if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                    {
                        var enteredText = ctrlAttributes.ToString().Trim();
                        attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                            attribute, enteredText);
                    }
                }
                    break;
                case AttributeControlType.Datepicker:
                case AttributeControlType.ColorSquares:
                case AttributeControlType.ImageSquares:
                case AttributeControlType.FileUpload:
                //not supported customer attributes
                default:
                    break;
            }
        }

        return attributesXml;
    }

    protected virtual async Task LogGdprAsync(Customer customer, CustomerInfoModel oldCustomerInfoModel,
        CustomerInfoModel newCustomerInfoModel, IFormCollection form)
    {
        try
        {
            //consents
            var consents = (await _gdprService.GetAllConsentsAsync()).Where(consent => consent.DisplayOnCustomerInfoPage).ToList();
            foreach (var consent in consents)
            {
                var previousConsentValue = await _gdprService.IsConsentAcceptedAsync(consent.Id, customer.Id);
                var controlId = $"consent{consent.Id}";
                var cbConsent = form[controlId];
                if (!StringValues.IsNullOrEmpty(cbConsent) && cbConsent.ToString().Equals("on"))
                {
                    //agree
                    if (!previousConsentValue.HasValue || !previousConsentValue.Value)
                    {
                        await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentAgree, consent.Message);
                    }
                }
                else
                {
                    //disagree
                    if (!previousConsentValue.HasValue || previousConsentValue.Value)
                    {
                        await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentDisagree, consent.Message);
                    }
                }
            }

            //newsletter subscriptions
            if (_gdprSettings.LogNewsletterConsent)
            {
                if (oldCustomerInfoModel.Newsletter && !newCustomerInfoModel.Newsletter)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentDisagree, await _localizationService.GetResourceAsync("Gdpr.Consent.Newsletter"));
                if (!oldCustomerInfoModel.Newsletter && newCustomerInfoModel.Newsletter)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree, await _localizationService.GetResourceAsync("Gdpr.Consent.Newsletter"));
            }

            //user profile changes
            if (!_gdprSettings.LogUserProfileChanges)
                return;

            if (oldCustomerInfoModel.Gender != newCustomerInfoModel.Gender)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.Gender")} = {newCustomerInfoModel.Gender}");

            if (oldCustomerInfoModel.FirstName != newCustomerInfoModel.FirstName)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.FirstName")} = {newCustomerInfoModel.FirstName}");

            if (oldCustomerInfoModel.LastName != newCustomerInfoModel.LastName)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.LastName")} = {newCustomerInfoModel.LastName}");

            if (oldCustomerInfoModel.ParseDateOfBirth() != newCustomerInfoModel.ParseDateOfBirth())
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.DateOfBirth")} = {newCustomerInfoModel.ParseDateOfBirth()}");

            if (oldCustomerInfoModel.Email != newCustomerInfoModel.Email)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.Email")} = {newCustomerInfoModel.Email}");

            if (oldCustomerInfoModel.Company != newCustomerInfoModel.Company)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.Company")} = {newCustomerInfoModel.Company}");

            if (oldCustomerInfoModel.StreetAddress != newCustomerInfoModel.StreetAddress)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.StreetAddress")} = {newCustomerInfoModel.StreetAddress}");

            if (oldCustomerInfoModel.StreetAddress2 != newCustomerInfoModel.StreetAddress2)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.StreetAddress2")} = {newCustomerInfoModel.StreetAddress2}");

            if (oldCustomerInfoModel.ZipPostalCode != newCustomerInfoModel.ZipPostalCode)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.ZipPostalCode")} = {newCustomerInfoModel.ZipPostalCode}");

            if (oldCustomerInfoModel.City != newCustomerInfoModel.City)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.City")} = {newCustomerInfoModel.City}");

            if (oldCustomerInfoModel.County != newCustomerInfoModel.County)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.County")} = {newCustomerInfoModel.County}");

            if (oldCustomerInfoModel.CountryId != newCustomerInfoModel.CountryId)
            {
                var countryName = (await _countryService.GetCountryByIdAsync(newCustomerInfoModel.CountryId))?.Name;
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.Country")} = {countryName}");
            }

            if (oldCustomerInfoModel.StateProvinceId != newCustomerInfoModel.StateProvinceId)
            {
                var stateProvinceName = (await _stateProvinceService.GetStateProvinceByIdAsync(newCustomerInfoModel.StateProvinceId))?.Name;
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.StateProvince")} = {stateProvinceName}");
            }
        }
        catch (Exception exception)
        {
            await _logger.ErrorAsync(exception.Message, exception, customer);
        }
    }

    #endregion

    #region Methods

    #region Login / logout

    //available even when a store is closed
    [CheckAccessClosedStore(ignore: true)]
    //available even when navigation is not allowed
    [CheckAccessPublicStore(ignore: true)]
    public virtual async Task<IActionResult> Login(bool? checkoutAsGuest)
    {
        var model = await _customerModelFactory.PrepareLoginModelAsync(checkoutAsGuest);
        var customer = await _workContext.GetCurrentCustomerAsync();

        if (await _customerService.IsRegisteredAsync(customer))
        {
            var fullName = await _customerService.GetCustomerFullNameAsync(customer);
            var message = await _localizationService.GetResourceAsync("Account.Login.AlreadyLogin");
            _notificationService.SuccessNotification(string.Format(message, _htmlEncoder.Encode(fullName)));
        }

        return View(model);
    }

    [HttpPost]
    [ValidateCaptcha]
    //available even when a store is closed
    [CheckAccessClosedStore(ignore: true)]
    //available even when navigation is not allowed
    [CheckAccessPublicStore(ignore: true)]
    public virtual async Task<IActionResult> Login(LoginModel model, string returnUrl, bool captchaValid)
    {
        //validate CAPTCHA
        if (_captchaSettings.Enabled && _captchaSettings.ShowOnLoginPage && !captchaValid)
        {
            ModelState.AddModelError("", await _localizationService.GetResourceAsync("Common.WrongCaptchaMessage"));
        }

        if (ModelState.IsValid)
        {
            var customerUserName = model.Username;
            var customerEmail = model.Email;
            var userNameOrEmail = _customerSettings.UsernamesEnabled ? customerUserName : customerEmail;

            var loginResult = await _customerRegistrationService.ValidateCustomerAsync(userNameOrEmail, model.Password);
            switch (loginResult)
            {
                case CustomerLoginResults.Successful:
                {
                    var customer = _customerSettings.UsernamesEnabled
                        ? await _customerService.GetCustomerByUsernameAsync(customerUserName)
                        : await _customerService.GetCustomerByEmailAsync(customerEmail);

                    return await _customerRegistrationService.SignInCustomerAsync(customer, returnUrl, model.RememberMe);
                }
                case CustomerLoginResults.MultiFactorAuthenticationRequired:
                {
                    var customerMultiFactorAuthenticationInfo = new CustomerMultiFactorAuthenticationInfo
                    {
                        UserName = userNameOrEmail,
                        RememberMe = model.RememberMe,
                        ReturnUrl = returnUrl
                    };
                    await HttpContext.Session.SetAsync(
                        NopCustomerDefaults.CustomerMultiFactorAuthenticationInfo,
                        customerMultiFactorAuthenticationInfo);
                    return RedirectToRoute("MultiFactorVerification");
                }
                case CustomerLoginResults.CustomerNotExist:
                    ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.CustomerNotExist"));
                    break;
                case CustomerLoginResults.Deleted:
                    ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.Deleted"));
                    break;
                case CustomerLoginResults.NotActive:
                    ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.NotActive"));
                    break;
                case CustomerLoginResults.NotRegistered:
                    ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.NotRegistered"));
                    break;
                case CustomerLoginResults.LockedOut:
                    ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.LockedOut"));
                    break;
                case CustomerLoginResults.WrongPassword:
                default:
                    ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials"));
                    break;
            }
        }

        //If we got this far, something failed, redisplay form
        model = await _customerModelFactory.PrepareLoginModelAsync(model.CheckoutAsGuest);
        return View(model);
    }

    /// <summary>
    /// The entry point for injecting a plugin component of type "MultiFactorAuth"
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the user verification page for Multi-factor authentication. Served by an authentication provider.
    /// </returns>
    public virtual async Task<IActionResult> MultiFactorVerification()
    {
        if (!await _multiFactorAuthenticationPluginManager.HasActivePluginsAsync())
            return RedirectToRoute("Login");

        var customerMultiFactorAuthenticationInfo = await HttpContext.Session.GetAsync<CustomerMultiFactorAuthenticationInfo>(
            NopCustomerDefaults.CustomerMultiFactorAuthenticationInfo);
        var userName = customerMultiFactorAuthenticationInfo?.UserName;
        if (string.IsNullOrEmpty(userName))
            return RedirectToRoute("Homepage");

        var customer = _customerSettings.UsernamesEnabled ? await _customerService.GetCustomerByUsernameAsync(userName) : await _customerService.GetCustomerByEmailAsync(userName);
        if (customer == null)
            return RedirectToRoute("Homepage");

        if (!await _permissionService.AuthorizeAsync(StandardPermission.Security.ENABLE_MULTI_FACTOR_AUTHENTICATION, customer))
            return RedirectToRoute("Homepage");

        var selectedProvider = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.SelectedMultiFactorAuthenticationProviderAttribute);
        if (string.IsNullOrEmpty(selectedProvider))
            return RedirectToRoute("Homepage");

        var model = new MultiFactorAuthenticationProviderModel();
        model = await _customerModelFactory.PrepareMultiFactorAuthenticationProviderModelAsync(model, selectedProvider, true);

        return View(model);
    }

    //available even when a store is closed
    [CheckAccessClosedStore(ignore: true)]
    //available even when navigation is not allowed
    [CheckAccessPublicStore(ignore: true)]
    public virtual async Task<IActionResult> Logout()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (_workContext.OriginalCustomerIfImpersonated != null)
        {
            //activity log
            await _customerActivityService.InsertActivityAsync(_workContext.OriginalCustomerIfImpersonated, "Impersonation.Finished",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.Impersonation.Finished.StoreOwner"),
                    customer.Email, customer.Id),
                customer);

            await _customerActivityService.InsertActivityAsync("Impersonation.Finished",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.Impersonation.Finished.Customer"),
                    _workContext.OriginalCustomerIfImpersonated.Email, _workContext.OriginalCustomerIfImpersonated.Id),
                _workContext.OriginalCustomerIfImpersonated);

            //logout impersonated customer
            await _genericAttributeService
                .SaveAttributeAsync<int?>(_workContext.OriginalCustomerIfImpersonated, NopCustomerDefaults.ImpersonatedCustomerIdAttribute, null);

            //redirect back to customer details page (admin area)
            return RedirectToAction("Edit", "Customer", new { id = customer.Id, area = AreaNames.ADMIN });
        }

        //activity log
        await _customerActivityService.InsertActivityAsync(customer, "PublicStore.Logout",
            await _localizationService.GetResourceAsync("ActivityLog.PublicStore.Logout"), customer);

        //standard logout 
        await _authenticationService.SignOutAsync();

        //raise logged out event       
        await _eventPublisher.PublishAsync(new CustomerLoggedOutEvent(customer));

        //EU Cookie
        if (_storeInformationSettings.DisplayEuCookieLawWarning)
        {
            //the cookie law message should not pop up immediately after logout.
            //otherwise, the user will have to click it again...
            //and thus next visitor will not click it... so violation for that cookie law..
            //the only good solution in this case is to store a temporary variable
            //indicating that the EU cookie popup window should not be displayed on the next page open (after logout redirection to homepage)
            //but it'll be displayed for further page loads
            TempData[$"{NopCookieDefaults.Prefix}{NopCookieDefaults.IgnoreEuCookieLawWarning}"] = true;
        }

        return RedirectToRoute("Homepage");
    }

    #endregion

    #region Password recovery

    //available even when navigation is not allowed
    [CheckAccessPublicStore(ignore: true)]
    //available even when a store is closed
    [CheckAccessClosedStore(ignore: true)]
    public virtual async Task<IActionResult> PasswordRecovery()
    {
        var model = new PasswordRecoveryModel();
        model = await _customerModelFactory.PreparePasswordRecoveryModelAsync(model);

        return View(model);
    }

    [ValidateCaptcha]
    [HttpPost, ActionName("PasswordRecovery")]
    [FormValueRequired("send-email")]
    //available even when navigation is not allowed
    [CheckAccessPublicStore(ignore: true)]
    //available even when a store is closed
    [CheckAccessClosedStore(ignore: true)]
    public virtual async Task<IActionResult> PasswordRecoverySend(PasswordRecoveryModel model, bool captchaValid)
    {
        // validate CAPTCHA
        if (_captchaSettings.Enabled && _captchaSettings.ShowOnForgotPasswordPage && !captchaValid)
        {
            ModelState.AddModelError("", await _localizationService.GetResourceAsync("Common.WrongCaptchaMessage"));
        }

        if (ModelState.IsValid)
        {
            var customer = await _customerService.GetCustomerByEmailAsync(model.Email);
            if (customer != null && customer.Active && !customer.Deleted)
            {
                //save token and current date
                var passwordRecoveryToken = Guid.NewGuid();
                await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.PasswordRecoveryTokenAttribute,
                    passwordRecoveryToken.ToString());
                DateTime? generatedDateTime = DateTime.UtcNow;
                await _genericAttributeService.SaveAttributeAsync(customer,
                    NopCustomerDefaults.PasswordRecoveryTokenDateGeneratedAttribute, generatedDateTime);

                //send email
                await _workflowMessageService.SendCustomerPasswordRecoveryMessageAsync(customer,
                    (await _workContext.GetWorkingLanguageAsync()).Id);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Account.PasswordRecovery.EmailHasBeenSent"));
            }
            else
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Account.PasswordRecovery.EmailNotFound"));
            }
        }

        model = await _customerModelFactory.PreparePasswordRecoveryModelAsync(model);

        return View(model);
    }

    //available even when navigation is not allowed
    [CheckAccessPublicStore(ignore: true)]
    //available even when a store is closed
    [CheckAccessClosedStore(ignore: true)]
    public virtual async Task<IActionResult> PasswordRecoveryConfirm(string token, string email, Guid guid)
    {
        //For backward compatibility with previous versions where email was used as a parameter in the URL
        var customer = await _customerService.GetCustomerByEmailAsync(email)
                       ?? await _customerService.GetCustomerByGuidAsync(guid);

        if (customer == null)
            return RedirectToRoute("Homepage");

        var model = new PasswordRecoveryConfirmModel { ReturnUrl = Url.RouteUrl("Homepage") };
        if (string.IsNullOrEmpty(await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.PasswordRecoveryTokenAttribute)))
        {
            model.DisablePasswordChanging = true;
            model.Result = await _localizationService.GetResourceAsync("Account.PasswordRecovery.PasswordAlreadyHasBeenChanged");
            return View(model);
        }

        //validate token
        if (!await _customerService.IsPasswordRecoveryTokenValidAsync(customer, token))
        {
            model.DisablePasswordChanging = true;
            model.Result = await _localizationService.GetResourceAsync("Account.PasswordRecovery.WrongToken");
            return View(model);
        }

        //validate token expiration date
        if (await _customerService.IsPasswordRecoveryLinkExpiredAsync(customer))
        {
            model.DisablePasswordChanging = true;
            model.Result = await _localizationService.GetResourceAsync("Account.PasswordRecovery.LinkExpired");
            return View(model);
        }

        return View(model);
    }

    [HttpPost, ActionName("PasswordRecoveryConfirm")]
    [FormValueRequired("set-password")]
    //available even when navigation is not allowed
    [CheckAccessPublicStore(ignore: true)]
    //available even when a store is closed
    [CheckAccessClosedStore(ignore: true)]
    public virtual async Task<IActionResult> PasswordRecoveryConfirmPOST(string token, string email, Guid guid, PasswordRecoveryConfirmModel model)
    {
        //For backward compatibility with previous versions where email was used as a parameter in the URL
        var customer = await _customerService.GetCustomerByEmailAsync(email)
                       ?? await _customerService.GetCustomerByGuidAsync(guid);

        if (customer == null)
            return RedirectToRoute("Homepage");

        model.ReturnUrl = Url.RouteUrl("Homepage");

        //validate token
        if (!await _customerService.IsPasswordRecoveryTokenValidAsync(customer, token))
        {
            model.DisablePasswordChanging = true;
            model.Result = await _localizationService.GetResourceAsync("Account.PasswordRecovery.WrongToken");
            return View(model);
        }

        //validate token expiration date
        if (await _customerService.IsPasswordRecoveryLinkExpiredAsync(customer))
        {
            model.DisablePasswordChanging = true;
            model.Result = await _localizationService.GetResourceAsync("Account.PasswordRecovery.LinkExpired");
            return View(model);
        }

        if (!ModelState.IsValid)
            return View(model);

        var response = await _customerRegistrationService
            .ChangePasswordAsync(new ChangePasswordRequest(customer.Email, false, _customerSettings.DefaultPasswordFormat, model.NewPassword));
        if (!response.Success)
        {
            model.Result = string.Join(';', response.Errors);
            return View(model);
        }

        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.PasswordRecoveryTokenAttribute, "");

        //authenticate customer after changing password
        await _customerRegistrationService.SignInCustomerAsync(customer, null, true);

        model.DisablePasswordChanging = true;
        model.Result = await _localizationService.GetResourceAsync("Account.PasswordRecovery.PasswordHasBeenChanged");
        return View(model);
    }

    #endregion     

    #region Register

    //available even when navigation is not allowed
    [CheckAccessPublicStore(ignore: true)]
    public virtual async Task<IActionResult> Register(string returnUrl)
    {
        //check whether registration is allowed
        if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
            return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Disabled, returnUrl });

        var model = new RegisterModel();
        model = await _customerModelFactory.PrepareRegisterModelAsync(model, false, setDefaultValues: true);

        return View(model);
    }

    [HttpPost]
    [ValidateCaptcha]
    [ValidateHoneypot]
    //available even when navigation is not allowed
    [CheckAccessPublicStore(ignore: true)]
    public virtual async Task<IActionResult> Register(RegisterModel model, string returnUrl, bool captchaValid, IFormCollection form)
    {
        //check whether registration is allowed
        if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
            return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Disabled, returnUrl });

        var customer = await _workContext.GetCurrentCustomerAsync();
        if (await _customerService.IsRegisteredAsync(customer))
        {
            //Already registered customer. 
            await _authenticationService.SignOutAsync();

            //raise logged out event       
            await _eventPublisher.PublishAsync(new CustomerLoggedOutEvent(customer));

            customer = await _customerService.InsertGuestCustomerAsync();

            //Save a new record
            await _workContext.SetCurrentCustomerAsync(customer);
        }

        var store = await _storeContext.GetCurrentStoreAsync();
        customer.RegisteredInStoreId = store.Id;

        //custom customer attributes
        var customerAttributesXml = await ParseCustomCustomerAttributesAsync(form);
        var customerAttributeWarnings = await _customerAttributeParser.GetAttributeWarningsAsync(customerAttributesXml);
        foreach (var error in customerAttributeWarnings)
        {
            ModelState.AddModelError("", error);
        }

        //validate CAPTCHA
        if (_captchaSettings.Enabled && _captchaSettings.ShowOnRegistrationPage && !captchaValid)
        {
            ModelState.AddModelError("", await _localizationService.GetResourceAsync("Common.WrongCaptchaMessage"));
        }

        //GDPR
        if (_gdprSettings.GdprEnabled)
        {
            var consents = (await _gdprService
                .GetAllConsentsAsync()).Where(consent => consent.DisplayDuringRegistration && consent.IsRequired).ToList();

            ValidateRequiredConsents(consents, form);
        }

        if (ModelState.IsValid)
        {
            var customerUserName = model.Username;
            var customerEmail = model.Email;

            var isApproved = _customerSettings.UserRegistrationType == UserRegistrationType.Standard;
            var registrationRequest = new CustomerRegistrationRequest(customer,
                customerEmail,
                _customerSettings.UsernamesEnabled ? customerUserName : customerEmail,
                model.Password,
                _customerSettings.DefaultPasswordFormat,
                store.Id,
                isApproved);
            var registrationResult = await _customerRegistrationService.RegisterCustomerAsync(registrationRequest);
            if (registrationResult.Success)
            {
                //properties
                if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                    customer.TimeZoneId = model.TimeZoneId;

                //VAT number
                if (_taxSettings.EuVatEnabled)
                {
                    customer.VatNumber = model.VatNumber;

                    var (vatNumberStatus, _, vatAddress) = await _taxService.GetVatNumberStatusAsync(model.VatNumber);
                    customer.VatNumberStatusId = (int)vatNumberStatus;
                    //send VAT number admin notification
                    if (!string.IsNullOrEmpty(model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                        await _workflowMessageService.SendNewVatSubmittedStoreOwnerNotificationAsync(customer, model.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);
                }

                //form fields
                if (_customerSettings.GenderEnabled)
                    customer.Gender = model.Gender;
                if (_customerSettings.FirstNameEnabled)
                    customer.FirstName = model.FirstName;
                if (_customerSettings.LastNameEnabled)
                    customer.LastName = model.LastName;
                if (_customerSettings.DateOfBirthEnabled)
                    customer.DateOfBirth = model.ParseDateOfBirth();
                if (_customerSettings.CompanyEnabled)
                    customer.Company = model.Company;
                if (_customerSettings.StreetAddressEnabled)
                    customer.StreetAddress = model.StreetAddress;
                if (_customerSettings.StreetAddress2Enabled)
                    customer.StreetAddress2 = model.StreetAddress2;
                if (_customerSettings.ZipPostalCodeEnabled)
                    customer.ZipPostalCode = model.ZipPostalCode;
                if (_customerSettings.CityEnabled)
                    customer.City = model.City;
                if (_customerSettings.CountyEnabled)
                    customer.County = model.County;
                if (_customerSettings.CountryEnabled)
                    customer.CountryId = model.CountryId;
                if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                    customer.StateProvinceId = model.StateProvinceId;
                if (_customerSettings.PhoneEnabled)
                    customer.Phone = model.Phone;
                if (_customerSettings.FaxEnabled)
                    customer.Fax = model.Fax;

                //save customer attributes
                customer.CustomCustomerAttributesXML = customerAttributesXml;
                await _customerService.UpdateCustomerAsync(customer);

                //newsletter
                if (_customerSettings.NewsletterEnabled)
                {
                    var isNewsletterActive = _customerSettings.UserRegistrationType != UserRegistrationType.EmailValidation;

                    //save newsletter value
                    var newsletter = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(customerEmail, store.Id);
                    if (newsletter != null)
                    {
                        if (model.Newsletter)
                        {
                            newsletter.Active = isNewsletterActive;
                            await _newsLetterSubscriptionService.UpdateNewsLetterSubscriptionAsync(newsletter);

                            //GDPR
                            if (_gdprSettings.GdprEnabled && _gdprSettings.LogNewsletterConsent)
                            {
                                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree, await _localizationService.GetResourceAsync("Gdpr.Consent.Newsletter"));
                            }
                        }
                        //else
                        //{
                        //When registering, not checking the newsletter check box should not take an existing email address off of the subscription list.
                        //_newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletter);
                        //}
                    }
                    else
                    {
                        if (model.Newsletter)
                        {
                            await _newsLetterSubscriptionService.InsertNewsLetterSubscriptionAsync(new NewsLetterSubscription
                            {
                                NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                Email = customerEmail,
                                Active = isNewsletterActive,
                                StoreId = store.Id,
                                LanguageId = customer.LanguageId ?? store.DefaultLanguageId,
                                CreatedOnUtc = DateTime.UtcNow
                            });

                            //GDPR
                            if (_gdprSettings.GdprEnabled && _gdprSettings.LogNewsletterConsent)
                            {
                                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree, await _localizationService.GetResourceAsync("Gdpr.Consent.Newsletter"));
                            }
                        }
                    }
                }

                if (_customerSettings.AcceptPrivacyPolicyEnabled)
                {
                    //privacy policy is required
                    //GDPR
                    if (_gdprSettings.GdprEnabled && _gdprSettings.LogPrivacyPolicyConsent)
                    {
                        await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree, await _localizationService.GetResourceAsync("Gdpr.Consent.PrivacyPolicy"));
                    }
                }

                //GDPR
                if (_gdprSettings.GdprEnabled)
                {
                    var consents = (await _gdprService.GetAllConsentsAsync()).Where(consent => consent.DisplayDuringRegistration).ToList();
                    foreach (var consent in consents)
                    {
                        var controlId = $"consent{consent.Id}";
                        var cbConsent = form[controlId];
                        if (!StringValues.IsNullOrEmpty(cbConsent) && cbConsent.ToString().Equals("on"))
                        {
                            //agree
                            await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentAgree, consent.Message);
                        }
                        else
                        {
                            //disagree
                            await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentDisagree, consent.Message);
                        }
                    }
                }

                //insert default address (if possible)
                var defaultAddress = new Address
                {
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    Email = customer.Email,
                    Company = customer.Company,
                    CountryId = customer.CountryId > 0
                        ? (int?)customer.CountryId
                        : null,
                    StateProvinceId = customer.StateProvinceId > 0
                        ? (int?)customer.StateProvinceId
                        : null,
                    County = customer.County,
                    City = customer.City,
                    Address1 = customer.StreetAddress,
                    Address2 = customer.StreetAddress2,
                    ZipPostalCode = customer.ZipPostalCode,
                    PhoneNumber = customer.Phone,
                    FaxNumber = customer.Fax,
                    CreatedOnUtc = customer.CreatedOnUtc
                };
                if (await _addressService.IsAddressValidAsync(defaultAddress))
                {
                    //some validation
                    if (defaultAddress.CountryId == 0)
                        defaultAddress.CountryId = null;
                    if (defaultAddress.StateProvinceId == 0)
                        defaultAddress.StateProvinceId = null;
                    //set default address
                    //customer.Addresses.Add(defaultAddress);

                    await _addressService.InsertAddressAsync(defaultAddress);

                    await _customerService.InsertCustomerAddressAsync(customer, defaultAddress);

                    customer.BillingAddressId = defaultAddress.Id;
                    customer.ShippingAddressId = defaultAddress.Id;

                    await _customerService.UpdateCustomerAsync(customer);
                }

                //notifications
                if (_customerSettings.NotifyNewCustomerRegistration)
                    await _workflowMessageService.SendCustomerRegisteredStoreOwnerNotificationMessageAsync(customer,
                        _localizationSettings.DefaultAdminLanguageId);

                //raise event       
                await _eventPublisher.PublishAsync(new CustomerRegisteredEvent(customer));
                var currentLanguage = await _workContext.GetWorkingLanguageAsync();

                switch (_customerSettings.UserRegistrationType)
                {
                    case UserRegistrationType.EmailValidation:
                        //email validation message
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.AccountActivationTokenAttribute, Guid.NewGuid().ToString());
                        await _workflowMessageService.SendCustomerEmailValidationMessageAsync(customer, currentLanguage.Id);

                        //result
                        return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.EmailValidation, returnUrl });

                    case UserRegistrationType.AdminApproval:
                        return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.AdminApproval, returnUrl });

                    case UserRegistrationType.Standard:
                        //send customer welcome message
                        await _workflowMessageService.SendCustomerWelcomeMessageAsync(customer, currentLanguage.Id);

                        //raise event       
                        await _eventPublisher.PublishAsync(new CustomerActivatedEvent(customer));

                        returnUrl = Url.RouteUrl("RegisterResult", new { resultId = (int)UserRegistrationType.Standard, returnUrl });
                        return await _customerRegistrationService.SignInCustomerAsync(customer, returnUrl, true);

                    default:
                        return RedirectToRoute("Homepage");
                }
            }

            //errors
            foreach (var error in registrationResult.Errors)
                ModelState.AddModelError("", error);
        }

        //If we got this far, something failed, redisplay form
        model = await _customerModelFactory.PrepareRegisterModelAsync(model, true, customerAttributesXml);

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

    #region My account / Info

    public virtual async Task<IActionResult> Info()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        var model = new CustomerInfoModel();
        model = await _customerModelFactory.PrepareCustomerInfoModelAsync(model, customer, false);

        var usaStates = await _stateProvinceService.GetStateProvincesByCountryIdAsync(1); // Country 1 = USA
        foreach (var s in usaStates)
        {
            model.AvailableStates.Add(new SelectListItem
            {
                Text = await _localizationService.GetLocalizedAsync(s, x => x.Name),
                Value = s.Id.ToString(),
                Selected = (s.Id == model.StateProvinceId)
            });
        }

        //var customAttrXml = await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.CustomCustomerAttributes);

        var customAttrXml = customer.CustomCustomerAttributesXML;
        foreach (CustomerAttributeModel attr in model.CustomerAttributes)
        {
            attr.DefaultValue = _customerAttributeParser.ParseValues(customer.CustomCustomerAttributesXML, attr.Id).FirstOrDefault();
        }

        // Counterpoint data - Rewards
        model.RewardsNumber = model.CustomerAttributes.Where(x => x.Name == "cpExtraValueCardNumber").FirstOrDefault().DefaultValue;
        model.RewardsPointsEarned = model.CustomerAttributes.Where(x => x.Name == "cpExtraValueCardRewardsPoints").FirstOrDefault().DefaultValue;
        if (model.RewardsPointsEarned == null)
        { model.RewardsPointsEarned = "0"; }
        model.RewardsPointsEarned = Convert.ToInt32(Math.Round(Convert.ToDouble(model.RewardsPointsEarned))).ToString();
        model.RewardsPointsGoal = "250";
        model.RewardsPointsToGo = (Convert.ToInt32(model.RewardsPointsGoal) - Convert.ToInt32(model.RewardsPointsEarned)).ToString();
        model.ReceiveRewardsMethod = model.CustomerAttributes.Where(x => x.Name == "Preferred Mode of Contact").FirstOrDefault().DefaultValue == "2" ? "Email" : "Mail";

        // Counterpoint data - Frequent Buyer Discounts
        model.CpDiscounts = await _cpDiscountService.GetCpDiscounts(model.CustomerAttributes.Where(x => x.Name == "cpCustomerId").FirstOrDefault().DefaultValue);
        if (model.CpDiscounts == null)
            model.CpDiscounts = new List<CpDiscount>();

        // Counterpoint data - Orders
        var cpOrders = await _cpOrderService.GetCpOrdersByCustomerId(model.CustomerAttributes.Where(x => x.Name == "cpCustomerId").FirstOrDefault().DefaultValue);
        if (cpOrders == null)
            cpOrders = new List<CpOrder>();
        var allVendors = await _vendorService.GetAllVendorsAsync();
        foreach (var cpOrder in cpOrders)
        {
            var cpOrderLines = await _cpOrderService.GetCpOrderLines(cpOrder.OrderId);
           // cpOrderLines = cpOrderLines.ToList();
            string points = Convert.ToInt32(Math.Round(cpOrder.OrderTotal)).ToString();
            string pointsPending = cpOrder.PurchaseDate.AddDays(30).Date > DateTime.Now.Date ? " Pending" : "";
            var vendor = allVendors.Where(x => x.AdminComment == cpOrder.StoreId).FirstOrDefault();
            string storeName = (vendor == null) ? "" : vendor.Name;
            CpOrderModel cpOrderModel = new CpOrderModel()
            {
                Order = cpOrder,
                OrderLines = cpOrderLines.ToList(),
                PointsText = points,
                PointsPendingText = pointsPending,
                StoreNameText = storeName
            };
            model.CpOrders.Add(cpOrderModel);
        }

        // Counterpoint data - preferrred store
        string prefStoreId = model.CustomerAttributes.Where(x => x.Name == "cpPreferredStoreId").FirstOrDefault().DefaultValue;
        var stores = await _vendorService.GetAllVendorsAsync();
        var store = stores.Where(x => x.AdminComment == prefStoreId).FirstOrDefault();

        if (store != null)
        {
            var address = await _addressService.GetAddressByIdAsync(store.AddressId);
            if (address == null)
                address = new Address();
            string storePhone = address.PhoneNumber.Replace("(", "").Replace(")", "").Replace("-", "");
            
            var stateProvinceId = address.StateProvinceId != null ? address.StateProvinceId??0 : 0;

            var stateProvinceName = await _stateProvinceService
            .GetStateProvinceByIdAsync(stateProvinceId);       

            model.CpPreferredStore = new CpPreferredStoreModel()
            {
                StoreName = store.Name,
                Description = store.Description,
                Address1 = address.Address1,
                Address2 = address.Address2,
                City = address.City,
                State = stateProvinceName.Abbreviation,
                Zip = address.ZipPostalCode,
                Phone = string.Format("({0}) {1}-{2}", storePhone.Substring(0, 3), storePhone.Substring(3, 3), storePhone.Substring(6, 4))
            };
        }
        else
        {
            model.CpPreferredStore = new CpPreferredStoreModel()
            {
                StoreName = "No Store Selected",
                Description = "",
                Address1 = "",
                Address2 = "",
                City = "",
                State = "",
                Zip = "",
                Phone = ""
            };
        }

        // Counterpoint data - preferred store options
        foreach (var vendor in await _vendorService.GetAllVendorsAsync())
        {
            try
            {
                string isSelected = (vendor.AdminComment == prefStoreId) ? "checked" : "";
                VendorModel v = new VendorModel()
                {
                    Id = int.Parse(vendor.AdminComment),
                    Name = vendor.Name,
                    Description = isSelected
                };
                model.Vendors.Add(v);
            }
            catch { }
        }

        // Pet Profiles
        model.PetProfiles = await _petProfileService.GetPetProfiles(model.RewardsNumber);

        // Interested In
        model.InterestedIn = model.CustomerAttributes.Where(x => x.Name == "Interested In").FirstOrDefault();

        // contact info text formatting
        var stateProvince = model.AvailableStates.Where(x => x.Value == model.StateProvinceId.ToString()).FirstOrDefault();
        model.StateProvinceText = stateProvince == null ? "" : stateProvince.Text;
        string phoneNumber = model.Phone.Replace("(", "").Replace(")", "").Replace("-", "");
        model.FormattedPhoneNumber = string.Format("({0}) {1}-{2}", phoneNumber.Substring(0, 3), phoneNumber.Substring(3, 3), phoneNumber.Substring(6, 4));


        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Info(CustomerInfoModel model, IFormCollection form)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        var oldCustomerModel = new CustomerInfoModel();

        //get customer info model before changes for gdpr log
        if (_gdprSettings.GdprEnabled & _gdprSettings.LogUserProfileChanges)
            oldCustomerModel = await _customerModelFactory.PrepareCustomerInfoModelAsync(oldCustomerModel, customer, false);

        //custom customer attributes
        var customerAttributesXml = await ParseCustomCustomerAttributesAsync(form);
        var customerAttributeWarnings = await _customerAttributeParser.GetAttributeWarningsAsync(customerAttributesXml);
        foreach (var error in customerAttributeWarnings)
        {
            ModelState.AddModelError("", error);
        }

        //GDPR
        if (_gdprSettings.GdprEnabled)
        {
            var consents = (await _gdprService
                .GetAllConsentsAsync()).Where(consent => consent.DisplayOnCustomerInfoPage && consent.IsRequired).ToList();

            ValidateRequiredConsents(consents, form);
        }

        try
        {
            if (ModelState.IsValid)
            {
                //username 
                if (_customerSettings.UsernamesEnabled && _customerSettings.AllowUsersToChangeUsernames)
                {
                    var userName = model.Username;
                    if (!customer.Username.Equals(userName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        //change username
                        await _customerRegistrationService.SetUsernameAsync(customer, userName);

                        //re-authenticate
                        //do not authenticate users in impersonation mode
                        if (_workContext.OriginalCustomerIfImpersonated == null)
                            await _authenticationService.SignInAsync(customer, true);
                    }
                }
                //email
                var email = model.Email;
                if (!customer.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase))
                {
                    //change email
                    var requireValidation = _customerSettings.UserRegistrationType == UserRegistrationType.EmailValidation;
                    await _customerRegistrationService.SetEmailAsync(customer, email, requireValidation);

                    //do not authenticate users in impersonation mode
                    if (_workContext.OriginalCustomerIfImpersonated == null)
                    {
                        //re-authenticate (if usernames are disabled)
                        if (!_customerSettings.UsernamesEnabled && !requireValidation)
                            await _authenticationService.SignInAsync(customer, true);
                    }
                }

                //properties
                if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                    customer.TimeZoneId = model.TimeZoneId;
                //VAT number
                if (_taxSettings.EuVatEnabled)
                {
                    var prevVatNumber = customer.VatNumber;
                    customer.VatNumber = model.VatNumber;

                    if (prevVatNumber != model.VatNumber)
                    {
                        var (vatNumberStatus, _, vatAddress) = await _taxService.GetVatNumberStatusAsync(model.VatNumber);
                        customer.VatNumberStatusId = (int)vatNumberStatus;

                        //send VAT number admin notification
                        if (!string.IsNullOrEmpty(model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                            await _workflowMessageService.SendNewVatSubmittedStoreOwnerNotificationAsync(customer,
                                model.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);
                    }
                }

                //form fields
                if (_customerSettings.GenderEnabled)
                    customer.Gender = model.Gender;
                if (_customerSettings.FirstNameEnabled)
                    customer.FirstName = model.FirstName;
                if (_customerSettings.LastNameEnabled)
                    customer.LastName = model.LastName;
                if (_customerSettings.DateOfBirthEnabled)
                    customer.DateOfBirth = model.ParseDateOfBirth();
                if (_customerSettings.CompanyEnabled)
                    customer.Company = model.Company;
                if (_customerSettings.StreetAddressEnabled)
                    customer.StreetAddress = model.StreetAddress;
                if (_customerSettings.StreetAddress2Enabled)
                    customer.StreetAddress2 = model.StreetAddress2;
                if (_customerSettings.ZipPostalCodeEnabled)
                    customer.ZipPostalCode = model.ZipPostalCode;
                if (_customerSettings.CityEnabled)
                    customer.City = model.City;
                if (_customerSettings.CountyEnabled)
                    customer.County = model.County;
                if (_customerSettings.CountryEnabled)
                    customer.CountryId = model.CountryId;
                if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                    customer.StateProvinceId = model.StateProvinceId;
                if (_customerSettings.PhoneEnabled)
                    customer.Phone = model.Phone;
                if (_customerSettings.FaxEnabled)
                    customer.Fax = model.Fax;

                customer.CustomCustomerAttributesXML = customerAttributesXml;
                await _customerService.UpdateCustomerAsync(customer);

                //newsletter
                if (_customerSettings.NewsletterEnabled)
                {
                    //save newsletter value
                    var store = await _storeContext.GetCurrentStoreAsync();
                    var newsletter = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(customer.Email, store.Id);
                    if (newsletter != null)
                    {
                        if (model.Newsletter)
                        {
                            newsletter.Active = true;
                            await _newsLetterSubscriptionService.UpdateNewsLetterSubscriptionAsync(newsletter);
                        }
                        else
                        {
                            await _newsLetterSubscriptionService.DeleteNewsLetterSubscriptionAsync(newsletter);
                        }
                    }
                    else
                    {
                        if (model.Newsletter)
                        {
                            await _newsLetterSubscriptionService.InsertNewsLetterSubscriptionAsync(new NewsLetterSubscription
                            {
                                NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                Email = customer.Email,
                                Active = true,
                                StoreId = store.Id,
                                LanguageId = customer.LanguageId ?? store.DefaultLanguageId,
                                CreatedOnUtc = DateTime.UtcNow
                            });
                        }
                    }
                }

                if (_forumSettings.ForumsEnabled && _forumSettings.SignaturesEnabled)
                    await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.SignatureAttribute, model.Signature);

                //GDPR
                if (_gdprSettings.GdprEnabled)
                    await LogGdprAsync(customer, oldCustomerModel, model, form);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Account.CustomerInfo.Updated"));

                return RedirectToRoute("CustomerInfo");
            }
        }
        catch (Exception exc)
        {
            ModelState.AddModelError("", exc.Message);
        }

        //If we got this far, something failed, redisplay form
        model = await _customerModelFactory.PrepareCustomerInfoModelAsync(model, customer, true, customerAttributesXml);

        return View(model);
    }

    [HttpPost] 
    public virtual async Task<IActionResult> EditProfile(IFormCollection form)
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Challenge();

        // Get rewards customer and update email/username
        var customer = await _workContext.GetCurrentCustomerAsync();

        // First track all changes so uncle bill's gets notification to update CounterPoint
        await TrackCustomerInfoChanges(form, customer);

        // Now make all actual changes
        customer.Email = form["email-address"];
        customer.Active = true;
        customer.Username = form["email-address"];
        await _customerService.UpdateCustomerAsync(customer);

        if (!String.IsNullOrWhiteSpace(form["current-password"]) && !String.IsNullOrWhiteSpace(form["new-password"]) && !String.IsNullOrWhiteSpace(form["confirm-password"]))
        {
            // Change password to user entered
            var changePasswordRequest = new ChangePasswordRequest(customer.Email, false, _customerSettings.DefaultPasswordFormat, form["new-password"]);
            var changePasswordResult = await _customerRegistrationService.ChangePasswordAsync(changePasswordRequest);
        }

        //form fields

        await _genericAttributeService.SaveAttributeAsync(customer, SystemCustomerAttributeNames.FirstName, form["first-name"]);
        await _genericAttributeService.SaveAttributeAsync(customer, SystemCustomerAttributeNames.LastName, form["last-name"]);


        if (_customerSettings.StreetAddressEnabled)
            await _genericAttributeService.SaveAttributeAsync(customer, SystemCustomerAttributeNames.StreetAddress, form["address-1"]);
        if (_customerSettings.StreetAddress2Enabled)
            await _genericAttributeService.SaveAttributeAsync(customer, SystemCustomerAttributeNames.StreetAddress2, form["address-2"]);

        if (_customerSettings.ZipPostalCodeEnabled)
            await _genericAttributeService.SaveAttributeAsync(customer, SystemCustomerAttributeNames.ZipPostalCode, form["address-zip"]);

        if (_customerSettings.CityEnabled)
            await _genericAttributeService.SaveAttributeAsync(customer, SystemCustomerAttributeNames.City, form["address-city"]);

        int stateProvinceId = int.Parse(form["StateProvinceId"]);
        if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
            await _genericAttributeService.SaveAttributeAsync(customer, SystemCustomerAttributeNames.StateProvinceId, stateProvinceId);
        
        if (_customerSettings.PhoneEnabled)
            await _genericAttributeService.SaveAttributeAsync(customer, SystemCustomerAttributeNames.Phone, form["address-phone"]);

        var addresses = await (await _customerService.GetAddressesByCustomerIdAsync(customer.Id))
           //enabled for the current store
           .WhereAwait(async a => a.CountryId == null || await _storeMappingService.AuthorizeAsync(await _countryService.GetCountryByAddressAsync(a)))
           .ToListAsync();



        // Update address records
        foreach (var addr in customer.Addresses)
        {
            customer.Addresses.Remove(addr);
        }
        var customerAddress = new Address
        {
            FirstName = await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.FirstName),
            


            LastName = await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.LastName),
            Email = customer.Email,
            Company = await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.Company),
            //CountryId = await _genericAttributeService.GetAttributeAsync<int>(customer, SystemCustomerAttributeNames.CountryId) > 0
            //    ? (int?)await _genericAttributeService.GetAttributeAsync<int>(customer, SystemCustomerAttributeNames.CountryId)
            //    : null,

            CountryId = 1,
            StateProvinceId = await _genericAttributeService.GetAttributeAsync<int>(customer, SystemCustomerAttributeNames.StateProvinceId) > 0
                ? (int?)await _genericAttributeService.GetAttributeAsync<int>(customer, SystemCustomerAttributeNames.StateProvinceId)
                : null,
            City = await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.City),
            Address1 = await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.StreetAddress),
            Address2 = await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.StreetAddress2),
            ZipPostalCode = await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.ZipPostalCode),
            PhoneNumber = await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.Phone),
            FaxNumber = await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.Fax),
            CreatedOnUtc = DateTime.UtcNow
        };

        await _genericAttributeService.SaveAttributeAsync<int>(customer, SystemCustomerAttributeNames.CountryId, 1);

        var countryId = await _genericAttributeService.GetAttributeAsync<int>(customer, SystemCustomerAttributeNames.CountryId);

        if (await _addressService.IsAddressValidAsync(customerAddress))
        {
            //set default address
            // customer.Addresses.Add(customerAddress);
            // customer.BillingAddress = customerAddress;
            // customer.ShippingAddress = customerAddress;
            //await _customerService.UpdateCustomerAsync(customer);

            foreach (var addr in customer.Addresses)
            {
                await _customerService.RemoveCustomerAddressAsync(customer, addr);
                //await _addressService.DeleteAddressAsync(addr);
            }            

            await _addressService.InsertAddressAsync(customerAddress);

            await _customerService.InsertCustomerAddressAsync(customer, customerAddress);
            customer.BillingAddress = customerAddress;
            customer.ShippingAddress = customerAddress;

            customer.ShippingAddressId = customerAddress.Id;
            customer.BillingAddressId = customerAddress.Id;

            customer.FirstName = customerAddress.FirstName;
            customer.ZipPostalCode = customerAddress.ZipPostalCode;
            customer.LastName = customerAddress.LastName;
            customer.Email = customerAddress.Email;
            customer.Phone = customerAddress.PhoneNumber;
            customer.StreetAddress = customerAddress.Address1;
            customer.StreetAddress2 = customerAddress.Address2;
            customer.City = customerAddress.City;
            customer.StateProvinceId = (int)customerAddress.StateProvinceId;

            await _customerService.UpdateCustomerAsync(customer);


            //customer.Addresses.Add(customerAddress);
            //customer.BillingAddress = customerAddress;
            //customer.ShippingAddress = customerAddress;
          

            //await _customerService.InsertCustomerAddressAsync(customer, customerAddress);

            //customer.BillingAddressId = customerAddress.Id;
            //customer.ShippingAddressId = customerAddress.Id;

            await _customerService.UpdateCustomerAsync(customer);


        }

        /***************************************************************************************/
        //var customAttrXml = await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.CustomCustomerAttributes);

        //CustomerAttribute receiveAttr = await _customerAttributeService.GetAllCustomerAttributesAsync().Where(x => x.Name == "Preferred Mode of Contact").FirstOrDefault();

        var customAttrXml = customer.CustomCustomerAttributesXML;
        var customerAttributes = await _customerAttributeService.GetAllAttributesAsync();
        
        var receiveAttr = customerAttributes.Where(x => x.Name == "Preferred Mode of Contact").FirstOrDefault();

        // Remove any existing selections
        try
        {
            string xPathQuery = "/Attributes/CustomerAttribute[@ID=" + receiveAttr.Id.ToString() + "]";

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(customAttrXml);
            XmlNode interestedInNode = xmlDoc.SelectSingleNode(xPathQuery);
            xmlDoc.ChildNodes[0].RemoveChild(interestedInNode);
            customAttrXml = xmlDoc.OuterXml;
        }
        catch { } // if there are no selections yet then just move on

        var receiveSelection = form["mail-group"];
        if (!String.IsNullOrEmpty(receiveSelection) && (receiveSelection == "Mail" || receiveSelection == "Email"))
        {

            var customerAttributeValues = (await _customerAttributeService
           .GetAttributeValuesAsync(receiveAttr.Id)).ToList();

            int itemId = customerAttributeValues.Where(x => x.Name == receiveSelection).FirstOrDefault().Id;

            customAttrXml = _customerAttributeParser.AddAttribute(customAttrXml, receiveAttr, itemId.ToString());
        }

        await _genericAttributeService.SaveAttributeAsync(customer, SystemCustomerAttributeNames.CustomCustomerAttributes, customAttrXml);

        customerAddress.CustomAttributes = customAttrXml;
        await _addressService.UpdateAddressAsync(customerAddress);

        customer.CustomCustomerAttributesXML = customAttrXml;
        await _customerService.UpdateCustomerAsync(customer);

        /***************************************************************************************/

        return RedirectToRoute("CustomerInfo");
    }

    [HttpPost]
    public virtual async Task<IActionResult> EditInterestedIn(IFormCollection form)
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Challenge();

        var customer = await _workContext.GetCurrentCustomerAsync();

        var customAttrXml = await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.CustomCustomerAttributes);


        var customerAttributes = await _customerAttributeService.GetAllAttributesAsync();
        var interestedInAttribute = customerAttributes.Where(x => x.Name == "Interested In").FirstOrDefault();

        // Remove any existing selections
        try
        {
            string xPathQuery = "/Attributes/CustomerAttribute[@ID=" + interestedInAttribute.Id.ToString() + "]";

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(customAttrXml);
            XmlNode interestedInNode = xmlDoc.SelectSingleNode(xPathQuery);
            xmlDoc.ChildNodes[0].RemoveChild(interestedInNode);
            customAttrXml = xmlDoc.OuterXml;
        }
        catch { } // if there are no selections yet then just move on

        var interestList = form["interested-group"];
        if (!String.IsNullOrEmpty(interestList))
        {
            foreach (var item in interestList.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            )
            {
                int selectedAttributeId = int.Parse(item);
                if (selectedAttributeId > 0)
                    customAttrXml = _customerAttributeParser.AddAttribute(customAttrXml, interestedInAttribute, selectedAttributeId.ToString());

            }
        }

        await _genericAttributeService.SaveAttributeAsync(customer, SystemCustomerAttributeNames.CustomCustomerAttributes, customAttrXml);

        customer.CustomCustomerAttributesXML = customAttrXml;
        await _customerService.UpdateCustomerAsync(customer);

        return RedirectToRoute("CustomerInfo");
    }

    [HttpPost]
    public virtual async Task<IActionResult> EditPreferredStore(IFormCollection form)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        var customAttrXml = await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.CustomCustomerAttributes);

        var customerAttributes = await _customerAttributeService.GetAllAttributesAsync();
        var preferredStoreAttr = customerAttributes.Where(x => x.Name == "cpPreferredStoreId").FirstOrDefault();

        // Remove any existing selections
        try
        {
            string xPathQuery = "/Attributes/CustomerAttribute[@ID=" + preferredStoreAttr.Id.ToString() + "]";

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(customAttrXml);
            XmlNode interestedInNode = xmlDoc.SelectSingleNode(xPathQuery);
            xmlDoc.ChildNodes[0].RemoveChild(interestedInNode);
            customAttrXml = xmlDoc.OuterXml;
        }
        catch { } // if there are no selections yet then just move on

        var prefStoreList = form["store-group"];
        if (!String.IsNullOrEmpty(prefStoreList))
        {
            foreach (var item in prefStoreList.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                int selectedAttributeId = int.Parse(item);
                if (selectedAttributeId > 0)
                    customAttrXml = _customerAttributeParser.AddAttribute(customAttrXml, preferredStoreAttr, selectedAttributeId.ToString());

            }
        }

        await _genericAttributeService.SaveAttributeAsync(customer, SystemCustomerAttributeNames.CustomCustomerAttributes, customAttrXml);

        customer.CustomCustomerAttributesXML = customAttrXml;
        await _customerService.UpdateCustomerAsync(customer);

        return RedirectToRoute("CustomerInfo");
    }

    [HttpPost]
    public virtual async Task<IActionResult> AddPetProfile(IFormCollection form)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        PetProfile newPet = new PetProfile()
        {
            Name = form["add-pet-name"],
            Species = form["add-pet-species"],
            Breed = form["add-pet-breed"],
            Gender = form["add-pet-gender"],
            Birthday = DateTime.Parse(form["add-pet-bday"]),
            CustomerId = form["add-rewards-number"]
        };

        await _petProfileService.InsertPetProfile(newPet);

        return RedirectToRoute("CustomerInfo");
    }

    [HttpPost]

    public virtual async Task<IActionResult> EditPetProfile(IFormCollection form)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        var petProfile = await _petProfileService.GetPetProfile(int.Parse(form["edit-pet-profile-id"]));

        if (petProfile != null)
        {
            petProfile.Name = form["edit-pet-name"];
            petProfile.Species = form["edit-pet-species"];
            petProfile.Breed = form["edit-pet-breed"];
            petProfile.Birthday = DateTime.Parse(form["edit-pet-bday"]);
            petProfile.Gender = form["edit-pet-gender"];

           await _petProfileService.UpdatePetProfile(petProfile);
        }

        return RedirectToRoute("CustomerInfo");
    }

    private async Task TrackCustomerInfoChanges(RegisterModel model, Customer customer)
    {
        CustomerInfoChange infoChange = new CustomerInfoChange();
        infoChange.CustomerId = customer.Id;
        infoChange.RewardsCardNumber = await HttpContext.Session.GetAsync<string>("rewardsCardNumber");


        //Session["rewardsCardNumber"].ToString();
        infoChange.ChangedOn = DateTime.Now;
        await _customerInfoChangeService.InsertCustomerInfoChange(infoChange);

        var stateProvinceId = await _genericAttributeService.GetAttributeAsync<int>(customer, SystemCustomerAttributeNames.StateProvinceId);
        // check each field for changes
        if (model.Email != customer.Email)
            await RecordCustomerInfoChangeData(infoChange.Id, "Email", customer.Email, model.Email);
        if (model.FirstName != await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.FirstName))
            await RecordCustomerInfoChangeData(infoChange.Id, "First Name", await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.FirstName), model.FirstName);
        if (model.LastName != await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.LastName))
            await RecordCustomerInfoChangeData(infoChange.Id, "Last Name", await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.LastName), model.LastName);
        if (model.StreetAddress != await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.StreetAddress))
            await RecordCustomerInfoChangeData(infoChange.Id, "Street Address", await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.StreetAddress), model.StreetAddress);
        if (model.StreetAddress2 != await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.StreetAddress2))
            await RecordCustomerInfoChangeData(infoChange.Id, "Street Address 2", await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.StreetAddress2), model.StreetAddress2);
        if (model.ZipPostalCode != await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.ZipPostalCode))
            await RecordCustomerInfoChangeData(infoChange.Id, "Zip Code", await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.ZipPostalCode), model.ZipPostalCode);
        if (model.City != await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.City))
            await RecordCustomerInfoChangeData(infoChange.Id, "City", await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.City), model.City);
        if (model.StateProvinceId != await _genericAttributeService.GetAttributeAsync<int>(customer, SystemCustomerAttributeNames.StateProvinceId))
            await RecordCustomerInfoChangeData(infoChange.Id, "State Province Id", stateProvinceId.ToString(), model.StateProvinceId.ToString());
        if (model.Phone != await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.Phone))
            await RecordCustomerInfoChangeData(infoChange.Id, "Phone", await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.Phone), model.Phone);
    }

    private async Task TrackCustomerInfoChanges(IFormCollection form, Customer customer)
    {
        CustomerInfoChange infoChange = new CustomerInfoChange();
        infoChange.CustomerId = customer.Id;
        infoChange.RewardsCardNumber = form["rewards-number"];
        infoChange.ChangedOn = DateTime.Now;
        await _customerInfoChangeService.InsertCustomerInfoChange(infoChange);

        var stateProvinceId = await _genericAttributeService.GetAttributeAsync<int>(customer, SystemCustomerAttributeNames.StateProvinceId);
        // check each field for changes
        if (form["email-address"] != customer.Email)
           await RecordCustomerInfoChangeData(infoChange.Id, "Email", customer.Email, form["email-address"]);
        if (form["first-name"] != await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.FirstName))
            await RecordCustomerInfoChangeData(infoChange.Id, "First Name", await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.FirstName), form["first-name"]);
        if (form["last-name"] != await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.LastName))
            await RecordCustomerInfoChangeData(infoChange.Id, "Last Name", await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.LastName), form["last-name"]);
        if (form["address-1"] != await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.StreetAddress))
            await RecordCustomerInfoChangeData(infoChange.Id, "Street Address", await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.StreetAddress), form["address-1"]);
        if (form["address-2"] != await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.StreetAddress2))
            await RecordCustomerInfoChangeData(infoChange.Id, "Street Address 2", await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.StreetAddress2), form["address-2"]);
        if (form["address-zip"] != await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.ZipPostalCode))
            await RecordCustomerInfoChangeData(infoChange.Id, "Zip Code", await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.ZipPostalCode), form["address-zip"]);
        if (form["address-city"] != await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.City))
            await RecordCustomerInfoChangeData(infoChange.Id, "City", await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.City), form["address-city"]);
        if (int.Parse(form["StateProvinceId"]) != await _genericAttributeService.GetAttributeAsync<int>(customer, SystemCustomerAttributeNames.StateProvinceId))
            await RecordCustomerInfoChangeData(infoChange.Id, "State Province Id", stateProvinceId.ToString(), int.Parse(form["StateProvinceId"]).ToString());
        if (form["address-phone"] != await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.Phone))
            await RecordCustomerInfoChangeData(infoChange.Id, "Phone", await _genericAttributeService.GetAttributeAsync<string>(customer, SystemCustomerAttributeNames.Phone), form["address-phone"]);
    }

    private async Task RecordCustomerInfoChangeData(int customerChangeInfoId, string fieldName, string oldValue, string newValue)
    {
        CustomerInfoChangeData infoChangeData = new CustomerInfoChangeData();
        infoChangeData.CustomerInfoChangeId = customerChangeInfoId;
        infoChangeData.FieldName = fieldName;
        infoChangeData.OldValue = oldValue;
        infoChangeData.NewValue = newValue;
        await _customerInfoChangeService.InsertCustomerInfoChangeData(infoChangeData);
    }

    [HttpPost]
    public virtual async Task<IActionResult> RemoveExternalAssociation(int id)
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Challenge();

        //ensure it's our record
        var ear = await _externalAuthenticationService.GetExternalAuthenticationRecordByIdAsync(id);

        if (ear == null)
        {
            return Json(new
            {
                redirect = Url.RouteUrl("CustomerInfo"),
            });
        }

        await _externalAuthenticationService.DeleteExternalAuthenticationRecordAsync(ear);

        return Json(new
        {
            redirect = Url.RouteUrl("CustomerInfo"),
        });
    }

    //available even when navigation is not allowed
    [CheckAccessPublicStore(ignore: true)]
    public virtual async Task<IActionResult> EmailRevalidation(string token, string email, Guid guid)
    {
        //For backward compatibility with previous versions where email was used as a parameter in the URL
        var customer = await _customerService.GetCustomerByEmailAsync(email)
                       ?? await _customerService.GetCustomerByGuidAsync(guid);

        if (customer == null)
            return RedirectToRoute("Homepage");

        var model = new EmailRevalidationModel { ReturnUrl = Url.RouteUrl("Homepage") };
        var cToken = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.EmailRevalidationTokenAttribute);
        if (string.IsNullOrEmpty(cToken))
        {
            model.Result = await _localizationService.GetResourceAsync("Account.EmailRevalidation.AlreadyChanged");
            return View(model);
        }

        if (!cToken.Equals(token, StringComparison.InvariantCultureIgnoreCase))
            return RedirectToRoute("Homepage");

        if (string.IsNullOrEmpty(customer.EmailToRevalidate))
            return RedirectToRoute("Homepage");

        if (_customerSettings.UserRegistrationType != UserRegistrationType.EmailValidation)
            return RedirectToRoute("Homepage");

        //change email
        try
        {
            await _customerRegistrationService.SetEmailAsync(customer, customer.EmailToRevalidate, false);
        }
        catch (Exception exc)
        {
            model.Result = await _localizationService.GetResourceAsync(exc.Message);
            return View(model);
        }

        customer.EmailToRevalidate = null;
        await _customerService.UpdateCustomerAsync(customer);
        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.EmailRevalidationTokenAttribute, "");

        //authenticate customer after changing email
        await _customerRegistrationService.SignInCustomerAsync(customer, null, true);

        model.Result = await _localizationService.GetResourceAsync("Account.EmailRevalidation.Changed");
        return View(model);
    }

    #endregion

    #region My account / Addresses

    public virtual async Task<IActionResult> Addresses()
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Challenge();

        var model = await _customerModelFactory.PrepareCustomerAddressListModelAsync();

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> AddressDelete(int addressId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        //find address (ensure that it belongs to the current customer)
        var address = await _customerService.GetCustomerAddressAsync(customer.Id, addressId);
        if (address != null)
        {
            await _customerService.RemoveCustomerAddressAsync(customer, address);
            await _customerService.UpdateCustomerAsync(customer);
            //now delete the address record
            await _addressService.DeleteAddressAsync(address);
        }

        //redirect to the address list page
        return Json(new
        {
            redirect = Url.RouteUrl("CustomerAddresses"),
        });
    }

    public virtual async Task<IActionResult> AddressAdd()
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Challenge();

        var model = new CustomerAddressEditModel();
        await _addressModelFactory.PrepareAddressModelAsync(model.Address,
            address: null,
            excludeProperties: false,
            addressSettings: _addressSettings,
            loadCountries: async () => await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id));

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> AddressAdd(CustomerAddressEditModel model, IFormCollection form)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        //custom address attributes
        var customAttributes = await _addressAttributeParser.ParseCustomAttributesAsync(form, NopCommonDefaults.AddressAttributeControlName);
        var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);
        foreach (var error in customAttributeWarnings)
        {
            ModelState.AddModelError("", error);
        }

        if (ModelState.IsValid)
        {
            var address = model.Address.ToEntity();
            address.CustomAttributes = customAttributes;
            address.CreatedOnUtc = DateTime.UtcNow;
            //some validation
            if (address.CountryId == 0)
                address.CountryId = null;
            if (address.StateProvinceId == 0)
                address.StateProvinceId = null;


            await _addressService.InsertAddressAsync(address);

            await _customerService.InsertCustomerAddressAsync(customer, address);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Account.CustomerAddresses.Added"));

            return RedirectToRoute("CustomerAddresses");
        }

        //If we got this far, something failed, redisplay form
        await _addressModelFactory.PrepareAddressModelAsync(model.Address,
            address: null,
            excludeProperties: true,
            addressSettings: _addressSettings,
            loadCountries: async () => await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id),
            overrideAttributesXml: customAttributes);

        return View(model);
    }

    public virtual async Task<IActionResult> AddressEdit(int addressId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        //find address (ensure that it belongs to the current customer)
        var address = await _customerService.GetCustomerAddressAsync(customer.Id, addressId);
        if (address == null)
            //address is not found
            return RedirectToRoute("CustomerAddresses");

        var model = new CustomerAddressEditModel();
        await _addressModelFactory.PrepareAddressModelAsync(model.Address,
            address: address,
            excludeProperties: false,
            addressSettings: _addressSettings,
            loadCountries: async () => await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id));

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> AddressEdit(CustomerAddressEditModel model, IFormCollection form)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        //find address (ensure that it belongs to the current customer)
        var address = await _customerService.GetCustomerAddressAsync(customer.Id, model.Address.Id);
        if (address == null)
            //address is not found
            return RedirectToRoute("CustomerAddresses");

        //custom address attributes
        var customAttributes = await _addressAttributeParser.ParseCustomAttributesAsync(form, NopCommonDefaults.AddressAttributeControlName);
        var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);
        foreach (var error in customAttributeWarnings)
        {
            ModelState.AddModelError("", error);
        }

        if (ModelState.IsValid)
        {
            address = model.Address.ToEntity(address);
            address.CustomAttributes = customAttributes;
            await _addressService.UpdateAddressAsync(address);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Account.CustomerAddresses.Updated"));

            return RedirectToRoute("CustomerAddresses");
        }

        //If we got this far, something failed, redisplay form
        await _addressModelFactory.PrepareAddressModelAsync(model.Address,
            address: address,
            excludeProperties: true,
            addressSettings: _addressSettings,
            loadCountries: async () => await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id),
            overrideAttributesXml: customAttributes);

        return View(model);
    }

    #endregion

    #region My account / Downloadable products

    public virtual async Task<IActionResult> DownloadableProducts()
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Challenge();

        if (_customerSettings.HideDownloadableProductsTab)
            return RedirectToRoute("CustomerInfo");

        var model = await _customerModelFactory.PrepareCustomerDownloadableProductsModelAsync();

        return View(model);
    }

    //ignore SEO friendly URLs checks
    [CheckLanguageSeoCode(ignore: true)]
    public virtual async Task<IActionResult> UserAgreement(Guid orderItemId)
    {
        var orderItem = await _orderService.GetOrderItemByGuidAsync(orderItemId);
        if (orderItem == null)
            return RedirectToRoute("Homepage");

        var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

        if (product == null || !product.HasUserAgreement)
            return RedirectToRoute("Homepage");

        var model = await _customerModelFactory.PrepareUserAgreementModelAsync(orderItem, product);

        return View(model);
    }

    #endregion

    #region My account / Change password

    public virtual async Task<IActionResult> ChangePassword()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        var model = await _customerModelFactory.PrepareChangePasswordModelAsync(customer);

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ChangePassword(ChangePasswordModel model, string returnUrl)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        if (ModelState.IsValid)
        {
            var changePasswordRequest = new ChangePasswordRequest(customer.Email,
                true, _customerSettings.DefaultPasswordFormat, model.NewPassword, model.OldPassword);
            var changePasswordResult = await _customerRegistrationService.ChangePasswordAsync(changePasswordRequest);
            if (changePasswordResult.Success)
            {
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Account.ChangePassword.Success"));

                //authenticate customer after changing password
                await _customerRegistrationService.SignInCustomerAsync(customer, null, true);

                if (string.IsNullOrEmpty(returnUrl))
                    return View(model);

                //prevent open redirection attack
                if (!Url.IsLocalUrl(returnUrl))
                    returnUrl = Url.RouteUrl("Homepage");

                return new RedirectResult(returnUrl);
            }

            //errors
            foreach (var error in changePasswordResult.Errors)
                ModelState.AddModelError("", error);
        }

        //If we got this far, something failed, redisplay form
        model = await _customerModelFactory.PrepareChangePasswordModelAsync(customer);

        return View(model);
    }

    #endregion

    #region My account / Avatar

    public virtual async Task<IActionResult> Avatar()
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Challenge();

        if (!_customerSettings.AllowCustomersToUploadAvatars)
            return RedirectToRoute("CustomerInfo");

        var model = new CustomerAvatarModel();
        model = await _customerModelFactory.PrepareCustomerAvatarModelAsync(model);

        return View(model);
    }

    [HttpPost, ActionName("Avatar")]
    [FormValueRequired("upload-avatar")]
    public virtual async Task<IActionResult> UploadAvatar(CustomerAvatarModel model, IFormFile uploadedFile)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        if (!_customerSettings.AllowCustomersToUploadAvatars)
            return RedirectToRoute("CustomerInfo");

        var contentType = uploadedFile?.ContentType.ToLowerInvariant();

        if (contentType != null && !contentType.Equals("image/jpeg") && !contentType.Equals("image/gif"))
            ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Avatar.UploadRules"));

        if (ModelState.IsValid)
        {
            try
            {
                var customerAvatar = await _pictureService.GetPictureByIdAsync(await _genericAttributeService.GetAttributeAsync<int>(customer, NopCustomerDefaults.AvatarPictureIdAttribute));
                if (uploadedFile != null && !string.IsNullOrEmpty(uploadedFile.FileName))
                {
                    var avatarMaxSize = _customerSettings.AvatarMaximumSizeBytes;
                    if (uploadedFile.Length > avatarMaxSize)
                        throw new NopException(string.Format(await _localizationService.GetResourceAsync("Account.Avatar.MaximumUploadedFileSize"), avatarMaxSize));

                    var customerPictureBinary = await _downloadService.GetDownloadBitsAsync(uploadedFile);
                    if (customerAvatar != null)
                        customerAvatar = await _pictureService.UpdatePictureAsync(customerAvatar.Id, customerPictureBinary, contentType, null);
                    else
                        customerAvatar = await _pictureService.InsertPictureAsync(customerPictureBinary, contentType, null);
                }

                var customerAvatarId = 0;
                if (customerAvatar != null)
                    customerAvatarId = customerAvatar.Id;

                await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.AvatarPictureIdAttribute, customerAvatarId);

                model.AvatarUrl = await _pictureService.GetPictureUrlAsync(
                    await _genericAttributeService.GetAttributeAsync<int>(customer, NopCustomerDefaults.AvatarPictureIdAttribute),
                    _mediaSettings.AvatarPictureSize,
                    false);

                return View(model);
            }
            catch (Exception exc)
            {
                ModelState.AddModelError("", exc.Message);
            }
        }

        //If we got this far, something failed, redisplay form
        model = await _customerModelFactory.PrepareCustomerAvatarModelAsync(model);
        return View(model);
    }

    [HttpPost, ActionName("Avatar")]
    [FormValueRequired("remove-avatar")]
    public virtual async Task<IActionResult> RemoveAvatar(CustomerAvatarModel model)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        if (!_customerSettings.AllowCustomersToUploadAvatars)
            return RedirectToRoute("CustomerInfo");

        var customerAvatar = await _pictureService.GetPictureByIdAsync(await _genericAttributeService.GetAttributeAsync<int>(customer, NopCustomerDefaults.AvatarPictureIdAttribute));
        if (customerAvatar != null)
            await _pictureService.DeletePictureAsync(customerAvatar);
        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.AvatarPictureIdAttribute, 0);

        return RedirectToRoute("CustomerAvatar");
    }

    #endregion

    #region GDPR tools

    public virtual async Task<IActionResult> GdprTools()
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Challenge();

        if (!_gdprSettings.GdprEnabled)
            return RedirectToRoute("CustomerInfo");

        var model = await _customerModelFactory.PrepareGdprToolsModelAsync();

        return View(model);
    }

    [HttpPost, ActionName("GdprTools")]
    [FormValueRequired("export-data")]
    public virtual async Task<IActionResult> GdprToolsExport()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        if (!_gdprSettings.GdprEnabled)
            return RedirectToRoute("CustomerInfo");

        //log
        await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ExportData, await _localizationService.GetResourceAsync("Gdpr.Exported"));

        var store = await _storeContext.GetCurrentStoreAsync();

        //export
        var bytes = await _exportManager.ExportCustomerGdprInfoToXlsxAsync(customer, store.Id);

        return File(bytes, MimeTypes.TextXlsx, "customerdata.xlsx");
    }

    [HttpPost, ActionName("GdprTools")]
    [FormValueRequired("delete-account")]
    public virtual async Task<IActionResult> GdprToolsDelete()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        if (!_gdprSettings.GdprEnabled)
            return RedirectToRoute("CustomerInfo");

        //log
        await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.DeleteCustomer, await _localizationService.GetResourceAsync("Gdpr.DeleteRequested"));

        await _workflowMessageService.SendDeleteCustomerRequestStoreOwnerNotificationAsync(customer, _localizationSettings.DefaultAdminLanguageId);

        var model = await _customerModelFactory.PrepareGdprToolsModelAsync();
        model.Result = await _localizationService.GetResourceAsync("Gdpr.DeleteRequested.Success");

        return View(model);
    }

    #endregion

    #region Check gift card balance

    //check gift card balance page
    //available even when a store is closed
    [CheckAccessClosedStore(ignore: true)]
    public virtual async Task<IActionResult> CheckGiftCardBalance()
    {
        if (!(_captchaSettings.Enabled && _customerSettings.AllowCustomersToCheckGiftCardBalance))
        {
            return RedirectToRoute("CustomerInfo");
        }

        var model = await _customerModelFactory.PrepareCheckGiftCardBalanceModelAsync();

        return View(model);
    }

    [HttpPost, ActionName("CheckGiftCardBalance")]
    [FormValueRequired("checkbalancegiftcard")]
    [ValidateCaptcha]
    public virtual async Task<IActionResult> CheckBalance(CheckGiftCardBalanceModel model, bool captchaValid)
    {
        //validate CAPTCHA
        if (_captchaSettings.Enabled && !captchaValid)
        {
            ModelState.AddModelError("", await _localizationService.GetResourceAsync("Common.WrongCaptchaMessage"));
        }

        if (ModelState.IsValid)
        {
            var giftCard = (await _giftCardService.GetAllGiftCardsAsync(giftCardCouponCode: model.GiftCardCode)).FirstOrDefault();
            if (giftCard != null && await _giftCardService.IsGiftCardValidAsync(giftCard))
            {
                var remainingAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(await _giftCardService.GetGiftCardRemainingAmountAsync(giftCard), await _workContext.GetWorkingCurrencyAsync());
                model.Result = await _priceFormatter.FormatPriceAsync(remainingAmount, true, false);
            }
            else
            {
                model.Message = await _localizationService.GetResourceAsync("CheckGiftCardBalance.GiftCardCouponCode.Invalid");
            }
        }

        return View(model);
    }

    #endregion

    #region Multi-factor Authentication

    //available even when a store is closed
    [CheckAccessClosedStore(ignore: true)]
    public virtual async Task<IActionResult> MultiFactorAuthentication()
    {
        if (!await _multiFactorAuthenticationPluginManager.HasActivePluginsAsync())
        {
            return RedirectToRoute("CustomerInfo");
        }

        if (!await _permissionService.AuthorizeAsync(StandardPermission.Security.ENABLE_MULTI_FACTOR_AUTHENTICATION))
            return RedirectToRoute("CustomerInfo");

        var model = new MultiFactorAuthenticationModel();
        model = await _customerModelFactory.PrepareMultiFactorAuthenticationModelAsync(model);
        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> MultiFactorAuthentication(MultiFactorAuthenticationModel model, IFormCollection form)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        if (!await _permissionService.AuthorizeAsync(StandardPermission.Security.ENABLE_MULTI_FACTOR_AUTHENTICATION))
            return RedirectToRoute("CustomerInfo");

        try
        {
            if (ModelState.IsValid)
            {
                //save MultiFactorIsEnabledAttribute
                if (!model.IsEnabled)
                {
                    if (!_multiFactorAuthenticationSettings.ForceMultifactorAuthentication)
                    {
                        await _genericAttributeService
                            .SaveAttributeAsync(customer, NopCustomerDefaults.SelectedMultiFactorAuthenticationProviderAttribute, string.Empty);

                        //raise change multi-factor authentication provider event       
                        await _eventPublisher.PublishAsync(new CustomerChangeMultiFactorAuthenticationProviderEvent(customer));
                    }
                    else
                    {
                        model = await _customerModelFactory.PrepareMultiFactorAuthenticationModelAsync(model);
                        model.Message = await _localizationService.GetResourceAsync("Account.MultiFactorAuthentication.Warning.ForceActivation");
                        return View(model);
                    }
                }
                else
                {
                    //save selected multi-factor authentication provider
                    var selectedProvider = await ParseSelectedProviderAsync(form);
                    var lastSavedProvider = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.SelectedMultiFactorAuthenticationProviderAttribute);
                    if (string.IsNullOrEmpty(selectedProvider) && !string.IsNullOrEmpty(lastSavedProvider))
                    {
                        selectedProvider = lastSavedProvider;
                    }

                    if (selectedProvider != lastSavedProvider)
                    {
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.SelectedMultiFactorAuthenticationProviderAttribute, selectedProvider);

                        //raise change multi-factor authentication provider event       
                        await _eventPublisher.PublishAsync(new CustomerChangeMultiFactorAuthenticationProviderEvent(customer));
                    }
                }

                return RedirectToRoute("MultiFactorAuthenticationSettings");
            }
        }
        catch (Exception exc)
        {
            ModelState.AddModelError("", exc.Message);
        }

        //If we got this far, something failed, redisplay form
        model = await _customerModelFactory.PrepareMultiFactorAuthenticationModelAsync(model);
        return View(model);
    }

    public virtual async Task<IActionResult> ConfigureMultiFactorAuthenticationProvider(string providerSysName)
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Challenge();

        if (!await _permissionService.AuthorizeAsync(StandardPermission.Security.ENABLE_MULTI_FACTOR_AUTHENTICATION))
            return RedirectToRoute("CustomerInfo");

        var model = new MultiFactorAuthenticationProviderModel();
        model = await _customerModelFactory.PrepareMultiFactorAuthenticationProviderModelAsync(model, providerSysName);

        return View(model);
    }

    #endregion

    public virtual async Task<IActionResult> ReOrder(string orderId)
    {
        await Task.Yield();
        return RedirectToAction("ReOrder", "ShoppingCart", new { orderId = orderId });
    }

    #endregion
}