using BarcodeLib;
using CsvHelper;
using CsvHelper.Configuration;
using DocumentFormat.OpenXml.EMMA;
using HarfBuzzSharp;
using Humanizer;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Configuration;
using ZXing;
//using ZXing.SkiaSharp;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Custom;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.ScheduleTasks;
using Nop.Services.Seo;
using Nop.Services.Vendors;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using ZXing.Common;
using DocumentFormat.OpenXml.InkML;
using Nop.Core.Domain.Blogs;
using static System.Net.Mime.MediaTypeNames;
using ZXing.ImageSharp.Rendering;
using SixLabors.ImageSharp.Formats;
using System.Drawing;

using System.Drawing.Imaging;

using System.IO;
using System.Linq;

using System.Threading.Tasks;


namespace Nop.Services.ExportImport.CpImports
{
    public partial class CustomerImportTask : IScheduleTask
    {
        private readonly ICustomerService _customerService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ILocalizationService _localizationService;
        private readonly IGenericAttributeService _genericAttributeService;
     
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly CustomerSettings _customerSettings;
        protected readonly IAttributeParser<CustomerAttribute, CustomerAttributeValue> _customerAttributeParser;
        protected readonly IAttributeService<CustomerAttribute, CustomerAttributeValue> _customerAttributeService;

        public CustomerImportTask(
            ICustomerService customerService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            ILocalizationService localizationService,
            IGenericAttributeService genericAttributeService,
            ICustomerRegistrationService customerRegistrationService,
            ICustomerActivityService customerActivityService,
            CustomerSettings customerSettings,
            IAttributeParser<CustomerAttribute, CustomerAttributeValue> customerAttributeParser,
            IAttributeService<CustomerAttribute, CustomerAttributeValue> customerAttributeService)
        {
            this._customerService = customerService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._localizationService = localizationService;
            this._genericAttributeService = genericAttributeService;
            //this._customerAttributeService = customerAttributeService;
            //this._customerAttributeParser = customerAttributeParser;
            this._customerRegistrationService = customerRegistrationService;
            this._customerActivityService = customerActivityService;
            this._customerSettings = customerSettings;
            _customerAttributeParser = customerAttributeParser;
            _customerAttributeService = customerAttributeService;
        }

        public async Task ExecuteAsync()
        {
            //await _customerService.RefreshRewards();
            var filePath = $@"D:\ftp.unclebills.com\Customers_{DateTime.Now:yyyy-MM-dd}.csv";

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture))
            {
                var importCustomers = csv.GetRecords<ImportCustomer>();

                var country = await _countryService.GetCountryByThreeLetterIsoCodeAsync("USA");
                string[] blockedCustomers = (await _localizationService.GetResourceAsync("customer.import.donotimport"))?.Split(',') ?? new[] { "0" };

                foreach (var iCust in importCustomers)
                {
                    if (!string.IsNullOrWhiteSpace(iCust.Phone) &&
                        iCust.Phone.Length > 4 &&
                        !blockedCustomers.Contains(iCust.ExtraValueCardNumber.Trim()))
                    {
                        bool isNewCust = false;
                        var customer = await _customerService.GetCustomerByRewardsCardAsync(iCust.ExtraValueCardNumber);

                        if (customer == null)
                        {
                            isNewCust = true;
                            customer = new Customer
                            {
                                CustomerGuid = Guid.NewGuid(),
                                Username = iCust.ExtraValueCardNumber,
                                IsTaxExempt = false,
                                AffiliateId = 0,
                                VendorId = 0,
                                HasShoppingCartItems = false,
                                RequireReLogin = false,
                                FailedLoginAttempts = 0,
                                Active = true,
                                Deleted = false,
                                IsSystemAccount = false,
                                CreatedOnUtc = DateTime.UtcNow,
                                LastActivityDateUtc = DateTime.UtcNow,
                                RegisteredInStoreId = 1
                            };
                            await _customerService.InsertCustomerAsync(customer);
                        }

                        var stateProvince = await _stateProvinceService.GetStateProvinceByAbbreviationAsync(iCust.State) // all customers should be indiana residents
                                            ?? await _stateProvinceService.GetStateProvinceByIdAsync(21); // Indiana

                        // Save attributes
                        await _genericAttributeService.SaveAttributeAsync(customer, SystemCustomerAttributeNames.FirstName, iCust.FirstName.ToLower());
                        await _genericAttributeService.SaveAttributeAsync(customer, SystemCustomerAttributeNames.LastName, iCust.LastName.ToLower());


                        if (_customerSettings.StreetAddressEnabled)
                            await _genericAttributeService.SaveAttributeAsync(customer, SystemCustomerAttributeNames.StreetAddress, iCust.Address1.ToLower());
                        if (_customerSettings.StreetAddress2Enabled)
                            await _genericAttributeService.SaveAttributeAsync(customer, SystemCustomerAttributeNames.StreetAddress2, iCust.Address2.ToLower());

                        if (_customerSettings.ZipPostalCodeEnabled)
                            await _genericAttributeService.SaveAttributeAsync(customer, SystemCustomerAttributeNames.ZipPostalCode, iCust.Zip);

                        if (_customerSettings.CityEnabled)
                            await _genericAttributeService.SaveAttributeAsync(customer, SystemCustomerAttributeNames.City, iCust.City.ToLower());

                        if (_customerSettings.CountryEnabled)
                            await _genericAttributeService.SaveAttributeAsync(customer, SystemCustomerAttributeNames.CountryId, country.Id);

                        
                        if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                            await _genericAttributeService.SaveAttributeAsync(customer, SystemCustomerAttributeNames.StateProvinceId, stateProvince.Id);

                        if (_customerSettings.PhoneEnabled)
                            await _genericAttributeService.SaveAttributeAsync(customer, SystemCustomerAttributeNames.Phone, iCust.Phone);


                        // Process custom attributes
                        string attributesXml = "";
                        var customerAttributes = await _customerAttributeService.GetAllAttributesAsync();
                        foreach (var attribute in customerAttributes)
                        {
                            string enteredText = "";
                            switch (attribute.Name)
                            {
                                case "cpExtraValueCardNumber":
                                    enteredText = iCust.ExtraValueCardNumber;
                                    break;
                                case "cpCustomerId":
                                    enteredText = iCust.CustomerId;
                                    break;
                                case "cpPreferredStoreId":
                                    enteredText = iCust.PreferredStoreId;
                                    break;
                                case "cpExtraValueCardRewardsPoints":
                                    enteredText = iCust.ExtraValueCardRewardsPoints;
                                    break;
                                default:
                                    break;
                            }
                            attributesXml = _customerAttributeParser.AddAttribute(attributesXml, attribute, enteredText);
                        }
                        await _genericAttributeService.SaveAttributeAsync(customer, SystemCustomerAttributeNames.CustomCustomerAttributes, attributesXml);


                        if (isNewCust)
                        {
                            // generate password
                            string password = iCust.Phone.Substring(iCust.Phone.Length - 4);
                            if (!String.IsNullOrWhiteSpace(password))
                            {
                                var changePassRequest = new ChangePasswordRequest(customer.Username, false, _customerSettings.DefaultPasswordFormat, password);
                                var changePassResult = await _customerRegistrationService.ChangePasswordAsync(changePassRequest);
                            }

                            // Add roles
                            CustomerRole defaultCustomer = await _customerService.GetCustomerRoleBySystemNameAsync("Registered");
                            customer.CustomerRoles.Add(defaultCustomer);
                            await _customerService.UpdateCustomerAsync(customer);
                        }
                    }
                }
            }
        }
    }

    public partial class OrderImportTask : IScheduleTask
    {
        private readonly ICpOrderService _cpOrderService;

        public OrderImportTask(ICpOrderService cpOrderService)
        {
            this._cpOrderService = cpOrderService;
        }

        public async Task ExecuteAsync()
        {
            //var file = File.OpenRead(@"C:\Users\Kyle\Documents\Testing\UncleBills\Orders_2018-04-19.csv");
            var file = File.OpenRead(@"D:\ftp.unclebills.com\Orders_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv");

            using (TextReader txtRdr = new StreamReader(file))
            using (CsvReader csvRdr = new CsvReader(txtRdr, System.Globalization.CultureInfo.InvariantCulture))
            {
               
                IEnumerable<ImportCpOrder> importCpOrders = csvRdr.GetRecords<ImportCpOrder>();
                foreach (ImportCpOrder iCpOrder in importCpOrders)
                {
                    CpOrder cpOrder = await _cpOrderService.GetCpOrderByOrderId(iCpOrder.OrderId);
                    if (cpOrder == null)
                    {
                        cpOrder = new CpOrder()
                        {
                            OrderId = iCpOrder.OrderId,
                            CustomerId = iCpOrder.CustomerId,
                            StoreId = iCpOrder.StoreId,
                            OrderTotal = double.Parse(iCpOrder.OrderTotal),
                            PurchaseDate = DateTime.Parse(iCpOrder.PurchaseDate)
                        };
                        await _cpOrderService.InsertCpOrder(cpOrder);
                    }
                    else
                    {
                        cpOrder.CustomerId = iCpOrder.CustomerId;
                        cpOrder.StoreId = iCpOrder.StoreId;
                        cpOrder.OrderTotal = double.Parse(iCpOrder.OrderTotal);
                        cpOrder.PurchaseDate = DateTime.Parse(iCpOrder.PurchaseDate);
                        await _cpOrderService.UpdateCpOrder(cpOrder);
                    }
                }
            }
        }
    }


    public partial class OrderLineImportTask : IScheduleTask
    {
        private readonly ICpOrderService _cpOrderService;
        public OrderLineImportTask(ICpOrderService cpOrderService)
        {
            this._cpOrderService = cpOrderService;
        }

        public async Task ExecuteAsync()
        {
            //var file = File.OpenRead(@"C:\Users\Kyle\Documents\Testing\UncleBills\OrderLines_2018-04-19.csv");
            var file = File.OpenRead(@"D:\ftp.unclebills.com\OrderLines_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv");

            using (TextReader txtRdr = new StreamReader(file))
            using (CsvReader csvRdr = new CsvReader(txtRdr, System.Globalization.CultureInfo.InvariantCulture))
            {
                IEnumerable<ImportCpOrderLine> importCpOrderLines = csvRdr.GetRecords<ImportCpOrderLine>();
                foreach (ImportCpOrderLine iCpOrderLine in importCpOrderLines)
                {
                    CpOrderLine cpOrderLine = await _cpOrderService.GetCpOrderLine(iCpOrderLine.OrderId, iCpOrderLine.LineNumber);
                    if (cpOrderLine == null)
                    {
                        cpOrderLine = new CpOrderLine()
                        {
                            OrderId = iCpOrderLine.OrderId,
                            LineNumber = iCpOrderLine.LineNumber,
                            ProductId = iCpOrderLine.ProductId,
                            ProductDescription = iCpOrderLine.ProductDescription.ToLower(),
                            Quantity = iCpOrderLine.Quantity
                        };
                        await _cpOrderService.InsertCpOrderLine(cpOrderLine);
                    }
                    else
                    {
                        cpOrderLine.ProductId = iCpOrderLine.ProductId;
                        cpOrderLine.ProductDescription = iCpOrderLine.ProductDescription.ToLower();
                        cpOrderLine.Quantity = iCpOrderLine.Quantity;
                        await _cpOrderService.UpdateCpOrderLine(cpOrderLine);
                    }
                }
            }
        }
    }

    public partial class DiscountImportTask : IScheduleTask
    {
        private readonly ICpDiscountService _cpDiscountService;
        public DiscountImportTask(ICpDiscountService cpDiscountService)
        {
            this._cpDiscountService = cpDiscountService;
        }

        public async Task ExecuteAsync()
        {
            //var file = File.OpenRead(@"C:\Users\Kyle\Documents\Testing\UncleBills\FreqBuyerDiscs_2018-04-19.csv");
            var file = File.OpenRead(@"D:\ftp.unclebills.com\FreqBuyerDiscs_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv");

            using (TextReader txtRdr = new StreamReader(file))
            using (CsvReader csvRdr = new CsvReader(txtRdr, System.Globalization.CultureInfo.InvariantCulture))
            {

                List<ImportDiscount> importDiscounts = csvRdr.GetRecords<ImportDiscount>().ToList();
                foreach (ImportDiscount iDiscount in importDiscounts)
                {
                    CpDiscount cpDiscount = await _cpDiscountService.GetCpDiscount(iDiscount.CustomerId, iDiscount.LoyaltyCode);
                    if (cpDiscount == null)
                    {
                        cpDiscount = new CpDiscount()
                        {
                            CustomerId = iDiscount.CustomerId,
                            LoyaltyCode = iDiscount.LoyaltyCode,
                            LoyaltyName = iDiscount.LoyaltyName.ToLower(),
                            PurchaseGoal = iDiscount.PurchaseGoal,
                            PurchaseStatus = iDiscount.PurchaseStatus
                        };
                        await _cpDiscountService.InsertCpDiscount(cpDiscount);
                    }
                    else
                    {
                        if (cpDiscount.LoyaltyName != iDiscount.LoyaltyName.ToLower() ||
                            cpDiscount.PurchaseGoal != iDiscount.PurchaseGoal ||
                            cpDiscount.PurchaseStatus != iDiscount.PurchaseStatus)
                        {
                            cpDiscount.LoyaltyName = iDiscount.LoyaltyName.ToLower();
                            cpDiscount.PurchaseGoal = iDiscount.PurchaseGoal;
                            cpDiscount.PurchaseStatus = iDiscount.PurchaseStatus;
                            await _cpDiscountService.UpdateCpDiscount(cpDiscount);
                        }
                    }
                }
            }
        }
    }

    public partial class StoreImportTask : IScheduleTask
    {
        private readonly IVendorService _vendorService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IAddressService _addressService;

        public StoreImportTask(IVendorService vendorService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IUrlRecordService urlRecordService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IAddressService addressService)
        {
            this._vendorService = vendorService;
            this._customerActivityService = customerActivityService;
            this._localizationService = localizationService;
            this._urlRecordService = urlRecordService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._addressService = addressService;
        }

        public async Task ExecuteAsync()
        {
            //var file = File.OpenRead(@"C:\Users\Kyle\Documents\Testing\UncleBills\StoreLocations_2018-04-19.csv");
            var file = File.OpenRead(@"D:\ftp.unclebills.com\StoreLocations_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv");


            using (TextReader txtRdr = new StreamReader(file))
            using (CsvReader csvRdr = new CsvReader(txtRdr, System.Globalization.CultureInfo.InvariantCulture))
            {

                IEnumerable<ImportStoreLocation> importLocations = csvRdr.GetRecords<ImportStoreLocation>();
                foreach (ImportStoreLocation iStore in importLocations)
                {
                    var vendors = await _vendorService.GetAllVendorsAsync();
                    var vendor = vendors.Where(x => x.AdminComment == iStore.StoreId).FirstOrDefault();
                    if (vendor == null)
                    {
                        vendor = new Vendor()
                        {
                            Name = iStore.StoreName,
                            Active = true,
                            Deleted = false,
                            DisplayOrder = 1,
                            PageSize = 0,
                            AllowCustomersToSelectPageSize = false,
                            PictureId = 0,
                            AddressId = 0,
                            AdminComment = iStore.StoreId
                        };
                       await _vendorService.InsertVendorAsync(vendor);

                        //activity log
                        
                        await _customerActivityService.InsertActivityAsync("AddNewVendor",
                            string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewVendor"), vendor.Id), vendor);

                    }
                    else
                    {
                        vendor.Name = iStore.StoreName;
                        await _vendorService.UpdateVendorAsync(vendor);

                        //activity log
                        
                        await _customerActivityService.InsertActivityAsync("EditVendor",
              string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditVendor"), vendor.Id), vendor);

                    }

                    //search engine name
                    //string seName = vendor.ValidateSeName(string.Empty, vendor.Name, true);
                    //_urlRecordService.SaveSlug(vendor, seName, 0);



                    var seName = await _urlRecordService.ValidateSeNameAsync(vendor, string.Empty, vendor.Name, true);
                    await _urlRecordService.SaveSlugAsync(vendor, seName, 0);


                    //address                   
                    var country = await _countryService.GetCountryByThreeLetterIsoCodeAsync("USA");  // future will need this to be dynamic?
                    var stateProvince = await _stateProvinceService.GetStateProvinceByAbbreviationAsync("iStore.State", country?.Id); // all customers should be indiana residents


                    if (stateProvince == null)
                        stateProvince = await _stateProvinceService.GetStateProvinceByIdAsync(21); // Indiana
                    Address address = await _addressService.GetAddressByIdAsync(vendor.AddressId);
                    if (address == null)
                    {
                        address = new Address()
                        {
                            Address1 = iStore.Address1.ToLower(),
                            Address2 = iStore.Address2.ToLower(),
                            City = iStore.City.ToLower(),
                            StateProvinceId = stateProvince.Id,
                            CountryId = country.Id,
                            ZipPostalCode = iStore.Zip,
                            PhoneNumber = iStore.Phone,
                            CreatedOnUtc = DateTime.UtcNow
                        };

                        await _addressService.InsertAddressAsync(address);
                        vendor.AddressId = address.Id;
                        await _vendorService.UpdateVendorAsync(vendor);
                    }
                    else
                    {
                        address.Address1 = iStore.Address1.ToLower();
                        address.Address2 = iStore.Address2.ToLower();
                        address.City = iStore.City.ToLower();
                        address.StateProvinceId = stateProvince.Id;
                        address.CountryId = country.Id;
                        address.ZipPostalCode = iStore.Zip;
                        address.PhoneNumber = iStore.Phone;

                        await _addressService.UpdateAddressAsync(address);
                    }
                }
            }
        }
    }

    public partial class ProductImportTask : IScheduleTask
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ISpecificationAttributeService _specificationAttributeService;

        public ProductImportTask(IProductService productService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IUrlRecordService urlRecordService,
            ISpecificationAttributeService specificationAttributeService)
        {
            this._productService = productService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._urlRecordService = urlRecordService;
            this._specificationAttributeService = specificationAttributeService;
        }

        public async Task ExecuteAsync()
        {
            var file = File.OpenRead(@"D:\ftp.unclebills.com\Products_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv");
            //var file = File.OpenRead(@"E:\UBProductUpdates\combined-products.csv");
            using (TextReader txtRdr = new StreamReader(file))
            using (CsvReader csvRdr = new CsvReader(txtRdr, System.Globalization.CultureInfo.InvariantCulture))
            {
                List<ImportProduct> csvImportProducts = csvRdr.GetRecords<ImportProduct>().ToList();

                // retrieve all existing products that will be updated during the import
                var allIncomingProductSkus = csvImportProducts.Select(p => p.Sku).ToArray();
                var allProductsBySku = await _productService.GetProductsBySkuAsync(allIncomingProductSkus);

                // retrieve all categories IDs for all existing products
                var allProductsCategoryIds = await _categoryService.GetProductCategoryIdsAsync(allProductsBySku.Select(p => p.Id).ToArray());

                var allCategories = await _categoryService.GetAllCategoriesAsync();
                var allProductCategories = await _categoryService.GetAllProductCategoryAsync();

                // retrieve all manufacturers IDs for products
                var allProductsManufacturerIds = await _manufacturerService.GetProductManufacturerIdsAsync(allProductsBySku.Select(p => p.Id).ToArray());

                foreach (ImportProduct csvImportProduct in csvImportProducts)
                {
                    try
                    {
                        // determine if product is 'new' or 'update'
                        var product = allProductsBySku.FirstOrDefault(p => p.Sku == csvImportProduct.Sku);
                        bool isNew = product == null;
                        if (isNew)
                        {
                            product = new Product();
                            product.CreatedOnUtc = DateTime.UtcNow;
                        }

                        // update all simple properties
                        product.ProductTypeId = 5; // Simple Product = 5; Grouped Product = 10
                        product.ParentGroupedProductId = 0;
                        product.VisibleIndividually = true;
                        product.Name = csvImportProduct.Name;
                        product.ShortDescription = csvImportProduct.Description;
                        product.FullDescription = csvImportProduct.Description;
                        product.VendorId = 0;
                        product.ProductTemplateId = 1;
                        product.ShowOnHomepage = false;
                        product.MetaKeywords = string.Empty;
                        product.MetaDescription = string.Empty;
                        product.MetaTitle = csvImportProduct.Name;
                        product.AllowCustomerReviews = false;
                        product.Published = true;
                        product.Sku = csvImportProduct.Sku;
                        product.ManufacturerPartNumber = string.Empty;
                        product.Gtin = csvImportProduct.UPC;
                        product.IsGiftCard = false;
                        product.GiftCardTypeId = 0;
                        product.OverriddenGiftCardAmount = 0;
                        product.RequireOtherProducts = false;
                        product.RequiredProductIds = string.Empty;
                        product.AutomaticallyAddRequiredProducts = false;
                        product.IsDownload = false;
                        product.DownloadId = 0;
                        product.UnlimitedDownloads = false;
                        product.MaxNumberOfDownloads = 0;
                        product.DownloadActivationTypeId = 0;
                        product.HasSampleDownload = false;
                        product.SampleDownloadId = 0;
                        product.HasUserAgreement = false;
                        product.UserAgreementText = string.Empty;
                        product.IsRecurring = false;
                        product.RecurringCycleLength = 0;
                        product.RecurringCyclePeriodId = 0;
                        product.RecurringTotalCycles = 0;
                        product.IsRental = false;
                        product.RentalPriceLength = 0;
                        product.RentalPricePeriodId = 0;
                        product.IsShipEnabled = true;
                        product.IsPickupOnly = csvImportProduct.IsPickupOnly;
                        product.HasHiddenPrice = csvImportProduct.HasHiddenPrice;
                        product.IsFreeShipping = false;
                        product.ShipSeparately = false;
                        product.AdditionalShippingCharge = 0;
                        product.DeliveryDateId = 0;
                        product.IsTaxExempt = false;
                        product.TaxCategoryId = 6; // 6 = Taxable Item
                        product.IsTelecommunicationsOrBroadcastingOrElectronicServices = false;
                        product.ManageInventoryMethodId = 1; // Don't manage stock = 0, manage stock = 1
                        product.UseMultipleWarehouses = true;
                        product.WarehouseId = 0;
                        product.StockQuantity = 0;
                        product.DisplayStockAvailability = false;
                        product.DisplayStockQuantity = false;
                        product.MinStockQuantity = 0;
                        product.LowStockActivityId = 0; // Nothing
                        product.NotifyAdminForQuantityBelow = 0;
                        product.BackorderModeId = 0; // No Backorders
                        product.AllowBackInStockSubscriptions = false;
                        product.OrderMinimumQuantity = 1;
                        product.OrderMaximumQuantity = 10000;
                        product.AllowedQuantities = string.Empty;
                        product.AllowAddingOnlyExistingAttributeCombinations = false;
                        product.DisableBuyButton = false;
                        product.DisableWishlistButton = false;
                        product.AvailableForPreOrder = false;
                        product.PreOrderAvailabilityStartDateTimeUtc = DateTime.UtcNow;
                        product.CallForPrice = false;
                        product.Price = isNew ? 0 : product.Price;
                        product.OldPrice = 0;
                        product.ProductCost = 0;
                        product.CustomerEntersPrice = false;
                        product.MinimumCustomerEnteredPrice = 0;
                        product.MaximumCustomerEnteredPrice = 0;
                        product.BasepriceEnabled = false;
                        product.BasepriceAmount = 0;
                        product.BasepriceUnitId = 0;
                        product.BasepriceBaseAmount = 0;
                        product.BasepriceBaseUnitId = 0;
                        product.MarkAsNew = false;
                        product.MarkAsNewStartDateTimeUtc = DateTime.UtcNow;
                        product.MarkAsNewEndDateTimeUtc = DateTime.UtcNow;
                        product.Weight = csvImportProduct.Weight;
                        product.Length = 0;
                        product.Width = 0;
                        product.Height = 0;

                        // save changes to database
                        product.UpdatedOnUtc = DateTime.UtcNow;
                        if (isNew)
                            await _productService.InsertProductAsync(product);
                        else
                            await _productService.UpdateProductAsync(product);


                        var productSeName = await _urlRecordService.GetSeNameAsync(product);

                        // update slug
                        if (isNew || string.IsNullOrWhiteSpace(productSeName))
                        {
                      
                            //search engine name
                            var seName = await _urlRecordService.ValidateSeNameAsync(product, string.Empty, product.Name, true);
                            await _urlRecordService.SaveSlugAsync(product, seName, 0);

                        }

                        // Add to Category if it isn't already related
                        if (!String.IsNullOrWhiteSpace(csvImportProduct.Category))
                        {
                            var prodCategories = allCategories.Where(n => n.Name == csvImportProduct.Category);
                            foreach (var category in prodCategories)
                            {
                                var categoryCheck = allProductCategories.Where(n => n.ProductId == product.Id && n.CategoryId == category.Id);
                                if (!categoryCheck.Any())
                                {
                                    var productCategory = new ProductCategory
                                    {
                                        ProductId = product.Id,
                                        CategoryId = category.Id,
                                        IsFeaturedProduct = false,
                                        DisplayOrder = 1
                                    };
                                   await _categoryService.InsertProductCategoryAsync(productCategory);
                                }
                            }
                        }

                        // Add to Manufacturer (Brand)
                        if (!String.IsNullOrWhiteSpace(csvImportProduct.Brand))
                        {
                            var manBrands = await _manufacturerService.GetAllManufacturersAsync();

                            Manufacturer manBrand = manBrands.FirstOrDefault(x => x.Name.ToLower() == csvImportProduct.Brand.ToLower());
                            if (manBrand != null)
                            {
                                List<int> existingProductManufacturers = new List<int>();
                                if (allProductsManufacturerIds.Any())
                                {
                                    if (allProductsManufacturerIds.Keys.Contains(product.Id))
                                    {
                                        existingProductManufacturers = allProductsManufacturerIds[product.Id].ToList();
                                    }
                                }

                                if (!existingProductManufacturers.Contains(manBrand.Id))
                                {
                                    var productManufacturer = new ProductManufacturer
                                    {
                                        ProductId = product.Id,
                                        ManufacturerId = manBrand.Id,
                                        IsFeaturedProduct = false,
                                        DisplayOrder = 1
                                    };
                                    await _manufacturerService.InsertProductManufacturerAsync(productManufacturer);
                                }
                            }
                        }

                        // Add Specification Attributes
                        var specAttrs =  await _specificationAttributeService.GetSpecificationAttributesAsync();

                        // Base Sku
                        if (!string.IsNullOrWhiteSpace(csvImportProduct.BaseSku))
                        {
                            SpecificationAttribute baseSku = specAttrs.FirstOrDefault(x => x.Name == "Base SKU");
                            //SpecificationAttributeOption saOption = baseSku.SpecificationAttributeOptions.FirstOrDefault();

                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(baseSku.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault();

                            await InsertSpecAttrOption(product.Id, saOption.Id, csvImportProduct.BaseSku, (int)SpecificationAttributeType.CustomText, false);
                        }

                        // Main Ingredient
                        if (!string.IsNullOrWhiteSpace(csvImportProduct.MainIngredient))
                        {
                            SpecificationAttribute mainIngredient = specAttrs.FirstOrDefault(x => x.Name == "Primary Ingredient");
                            //SpecificationAttributeOption saOption = mainIngredient.SpecificationAttributeOptions.FirstOrDefault();


                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(mainIngredient.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault();

                            await InsertSpecAttrOption(product.Id, saOption.Id, csvImportProduct.MainIngredient, (int)SpecificationAttributeType.CustomText, false);
                        }

                        // Ingredients
                        if (!string.IsNullOrWhiteSpace(csvImportProduct.Ingredients))
                        {
                            SpecificationAttribute ingredients = specAttrs.FirstOrDefault(x => x.Name == "Ingredients");
                            
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(ingredients.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault();

                            await InsertSpecAttrOption(product.Id, saOption.Id, csvImportProduct.Ingredients, (int)SpecificationAttributeType.CustomHtmlText, false);
                        }

                        // Guaranteed Analysis
                        if (!string.IsNullOrWhiteSpace(csvImportProduct.Analysis))
                        {
                            SpecificationAttribute guaranteedAnalysis = specAttrs.FirstOrDefault(x => x.Name == "Guaranteed Analysis");
                            
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(guaranteedAnalysis.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault();

                            await InsertSpecAttrOption(product.Id, saOption.Id, csvImportProduct.Analysis, (int)SpecificationAttributeType.CustomHtmlText, false);
                        }

                        // Frequent Buyer Program
                        if (csvImportProduct.FBP)
                        {
                            SpecificationAttribute freqBuyerProg = specAttrs.FirstOrDefault(x => x.Name == "Frequent Buyer Program");
                            
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(freqBuyerProg.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault();

                            await InsertSpecAttrOption(product.Id, saOption.Id, string.Empty, (int)SpecificationAttributeType.Option, false);
                        }

                        // Expands To
                        if (!string.IsNullOrWhiteSpace(csvImportProduct.ExpandsTo))
                        {
                            SpecificationAttribute expandsTo = specAttrs.FirstOrDefault(x => x.Name == "Expands To");
                            
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(expandsTo.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault();
                            await InsertSpecAttrOption(product.Id, saOption.Id, csvImportProduct.ExpandsTo, (int)SpecificationAttributeType.CustomText, false);
                        }

                        // Includes
                        if (!string.IsNullOrWhiteSpace(csvImportProduct.Includes))
                        {
                            SpecificationAttribute includes = specAttrs.FirstOrDefault(x => x.Name == "Includes");
                            
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(includes.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault();
                            await InsertSpecAttrOption(product.Id, saOption.Id, csvImportProduct.Includes, (int)SpecificationAttributeType.CustomHtmlText, false);
                        }

                        // Dimensions
                        if (!string.IsNullOrWhiteSpace(csvImportProduct.Dimensions))
                        {
                            SpecificationAttribute dimensions = specAttrs.FirstOrDefault(x => x.Name == "Dimensions");

                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(dimensions.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault();
           
                            await InsertSpecAttrOption(product.Id, saOption.Id, csvImportProduct.Dimensions, (int)SpecificationAttributeType.CustomHtmlText, false);
                        }

                        /*************************************/
                        // Characteristics/Filter Data
                        SpecificationAttribute characteristics = specAttrs.FirstOrDefault(x => x.Name == "Characteristics");
                        if (csvImportProduct.Puppy)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Puppy");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Kitten)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Kitten");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Adult)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Adult");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Senior)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Senior");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.SmallBreed)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Small Breed");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.LargeBreed)
                        {                           
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Large Breed");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.GrainFree)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Grain Free");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Pellets)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Pellets");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Granule)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Granule");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Flakes)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Flakes");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Wafers)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Wafers");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Cubes)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Cubes");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Dehydrated)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Dehydrated");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Frozen)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Frozen");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Goldfish)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Goldfish");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Betta)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Betta");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Parakeet)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Parakeet");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Cockatiel)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Cockatiel");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.CanaryFinch)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Canary & Finch");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Conure)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Conure");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.ParrotHookbill)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Parrot & Hookbill");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Amphibian)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Amphibian");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Crustacean)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Crustacean");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.BeardedDragon)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Bearded Dragon");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Gecko)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Gecko");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Turtle)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Turtle");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Tortoise)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Tortoise");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.HermitCrab)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Hermit Crab");


                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Rabbit)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Rabbit");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Ferret)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Ferret");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.HamsterGerbil)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Hamster & Gerbil");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Chinchilla)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Chinchilla");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.GuineaPig)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Guinea Pig");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.RatMouse)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Rat & Mouse");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Hedgehog)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Hedgehog");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.SugarGlider)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Sugar Glider");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Clumping)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Clumping");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.NonClumping)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Non-Clumping");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.MultiCat)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Multi-Cat");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Snake)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Snake");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Lightweight)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Lightweight");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Gallons10)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Up to 10 Gallons");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Gallons19)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "11 Gallons - 19 Gallons");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Gallons39)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "20 Gallons - 39 Gallons");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Gallons55)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "40 Gallons - 55 Gallons");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Gallons75)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "56 Gallons - 75 Gallons");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Gallons75Plus)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "75 Gallons or Larger");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.ReplacementFiltersMediaAccessories)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Replacement Filters and Media Accessories");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Balls)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Balls");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.RopeToys)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Rope Toys");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.PlushToys)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Plush Toys");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.FlyingToys)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Flying Toys");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.TreatDispensingToys)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Treat Dispensing");


                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.BallsChasers)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Balls & Chasers");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Catnip)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Catnip");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.HuntingStalkingToys)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Hunting/Stalking Toys");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Teasers)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Teasers");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Electronic)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Electronic");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Mirrors)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Mirrors");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Ladders)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Ladders");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.ChewForage)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Chew & Forage");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.PerchesSwings)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Perches & Swings");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Fluorescent)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Fluorescent");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.LED)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "LED");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.StickBarTreats)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Stick/Bar Treats");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Millet)
                        {                            
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Millet");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.ContainsFruit)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Contains Fruit");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Cuttlebone)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Cuttlebone");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Watts015)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Under 15 Watts");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Watts1525)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "15-25 Watts");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Watts2655)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "26-55 Watts");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Watts56100)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "56-100 Watts");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Watts101250)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "101-250 Watts");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Shampoos)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Shampoos");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.ColognesSprays)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Colognes & Sprays");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.BrushesCombsDeshedders)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Brushes, Combs, Deshedders");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.ClippersShearsAccessories)
                        {                            
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Clippers, Shears, Accessories");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.NailCare)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Nail Care");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        //if (csvImportProduct.ShippingEnabled)
                        //{
                        //    SpecificationAttributeOption saOption = characteristics.SpecificationAttributeOptions.FirstOrDefault(x => x.Name == "Shipping Enabled");
                        //    InsertSpecAttrOption(product.Id, saOption.Id);
                        //}
                        if (csvImportProduct.DogBowl)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Dog Bowl");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.ElevatedFeeder)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Elevated Feeder");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.DogWaterBottles)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Dog Water Bottles");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.DualBowls)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Dual Bowls");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.DogFoodStorage)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Dog Food Storage");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.DentalCare)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Dental Care");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.EarCare)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Ear Care");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.MedicatedSprayTopicals)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Medicated Spray / Topicals");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.MedicatedDropsChews)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Medicated Drops & Chews");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.HomeCleanersDeodorizers)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Home Cleaners & Deodorizers");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Calming)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Calming");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Digestive)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Digestive");

                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.CBD)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "CBD");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.HipJoint)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "DHip & Joint");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Freshwater)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Freshwater");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Saltwater)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Saltwater");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Pond)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Pond");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.PillsAndChews)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Pills and Chews");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.TopicalsSpraysShampoos)
                        {
                           var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Topicals, Sprays, & Shampoos");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.HomeAndOutdoorSprays)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Home and Outdoor Sprays");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Collars)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Collars");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Decorations)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Decorations");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Hides)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Hides");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Thermometers)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Thermometers");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.ElectronicAccessory)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Electronic Accessory");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.CageAccessories)
                        {

                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Cage Accessories");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.VacuumsSiphons)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Vacuums & Siphons");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.ScrapersBladesScrubbers)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Scrapers, Blades, & Scrubbers");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Magnets)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Magnets");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.LiquidSupplement)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Liquid Supplement");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.PowderedSupplement)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Powdered Supplement");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Perch)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Perch");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.NestingSacks)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Nesting / Sacks");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.SeedDishCatcher)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Seed Dish / Catcher");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.ShampoosConditionersSprays)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Shampoos, Conditioners, & Sprays");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.NailTrimmers)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Nail Trimmers");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.Brushes)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Brushes");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.EarCleaner)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Ear Cleaner");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.DeodorizersStainRemovers)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Deodorizers & Stain Removers");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.BowlsWaterBottles)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Bowls & Water Bottles");
                            InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.BallsWheels)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Balls & Wheels");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.TubesTunnels)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Tubes & Tunnels");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.ChewsBonesBullySticks)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Chews, Bones, & Bully Sticks");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }
                        if (csvImportProduct.EdibleChews)
                        {
                            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(characteristics.Id));
                            SpecificationAttributeOption saOption = options.FirstOrDefault(x => x.Name == "Edible Chews");
                            await InsertSpecAttrOption(product.Id, saOption.Id);
                        }

                        // END Characteristics/Filter Data
                        /*****************************************/
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
        }

        private async Task<ProductSpecificationAttribute> GetSpecAttrOption(int productId, int specAttrOptionId)
        {
            var productSpecificationAttributes = await _specificationAttributeService.GetProductSpecificationAttributesAsync(productId, specAttrOptionId);

            return productSpecificationAttributes.FirstOrDefault();
        }

        private async Task InsertSpecAttrOption(int productId, int specAttrOptionId)
        {
            await InsertSpecAttrOption(productId, specAttrOptionId, string.Empty, (int)SpecificationAttributeType.Option, true);
        }

        private async Task InsertSpecAttrOption(int productId, int specAttrOptionId, string customValue, int attrTypeId, bool allowFilter)
        {
            ProductSpecificationAttribute updatePsa = await GetSpecAttrOption(productId, specAttrOptionId);
            if (updatePsa != null)
            {
                updatePsa.CustomValue = customValue;
                updatePsa.AttributeTypeId = attrTypeId;
                updatePsa.AllowFiltering = allowFilter;
               await _specificationAttributeService.UpdateProductSpecificationAttributeAsync(updatePsa);
            }
            else
            {
                ProductSpecificationAttribute newPsa = new ProductSpecificationAttribute
                {
                    ProductId = productId,
                    SpecificationAttributeOptionId = specAttrOptionId,
                    AllowFiltering = allowFilter,
                    ShowOnProductPage = true,
                    DisplayOrder = 0,
                    AttributeTypeId = attrTypeId,
                    CustomValue = customValue
                };
                await _specificationAttributeService.InsertProductSpecificationAttributeAsync(newPsa);
            }
        }
    }


    public partial class InventoryImportFullTask : IScheduleTask
    {
        // this task can be run on demand to update inventory on all skus
        // the nightly and partial updates are only doing daily deltas
        // this file import will completely reset all levels to counter point levels
        // if for some reason the have gotten off course over time - missed nightly updates, or bad data in nightly updates
        private readonly IWarehouseInventoryImportService _warehouseInventoryImportService;

        public InventoryImportFullTask(IWarehouseInventoryImportService warehouseInventoryImportService)
        {
            this._warehouseInventoryImportService = warehouseInventoryImportService;
        }

        public async Task ExecuteAsync()
        {
            string fileFullPath = @"D:\ftp.unclebills.com\Inventory_Full_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv";
            string result = await _warehouseInventoryImportService.Import(fileFullPath);

            await Task.Yield();
        }
    }

    public partial class InventoryImportNightlyTask : IScheduleTask
    {
        // this task runs nightly to keep inventory accurate from the full days changes
        // the expected import file contains all inventory deltas from the previous day
        // this overrides the partial daily updates that run every 15 minutes
        // this is not meant to be a full inventory update - if you try this you will likely crash/timeout
        // to run full inventory updates, you will need to break the file up into batches - less per batch = better performance
        private readonly IWarehouseInventoryImportService _warehouseInventoryImportService;

        public InventoryImportNightlyTask(IWarehouseInventoryImportService warehouseInventoryImportService)
        {
            this._warehouseInventoryImportService = warehouseInventoryImportService;
        }

        public async Task ExecuteAsync()
        {
            string fileFullPath = @"D:\ftp.unclebills.com\Inventory_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv";
            string result = await _warehouseInventoryImportService.Import(fileFullPath);
            await Task.Yield();
        }
    }

    public partial class InventoryImportPartialDailyUpdatesTask : IScheduleTask
    {
        // this task runs from start of day every 15 minutes until end of day
        // this attempts to keep stock quantities "live" on the web during daily changes
        // the expected import file contains only deltas since the last import was run today
        private readonly IWarehouseInventoryImportService _warehouseInventoryImportService;

        public InventoryImportPartialDailyUpdatesTask(IWarehouseInventoryImportService warehouseInventoryImportService)
        {
            this._warehouseInventoryImportService = warehouseInventoryImportService;
        }

        public async Task ExecuteAsync()
        {
            string fileFullPath = @"D:\ftp.unclebills.com\Inventory_LiveDeltas_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv";
            string result = await _warehouseInventoryImportService.Import(fileFullPath);
            // to prevent i/o write issues with the next update today, delete this file
            File.Delete(fileFullPath);
            await Task.Yield();
        }
    }

    public partial class RewardsCertificateImportTask : IScheduleTask
    {
        private readonly IGiftCardService _giftCardService;

        public RewardsCertificateImportTask(IGiftCardService giftCardService)
        {
            this._giftCardService = giftCardService;
        }

        public async Task ExecuteAsync()
        {
            var file = File.OpenRead(@"D:\ftp.unclebills.com\RewardsCertificates_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv");

            using (var txtRdr = new StreamReader(file))
            using (var csvRdr = new CsvReader(txtRdr, System.Globalization.CultureInfo.InvariantCulture))
            {

                List<ImportRewardsCertificate> csvCertificates = csvRdr.GetRecords<ImportRewardsCertificate>().ToList();
                foreach (ImportRewardsCertificate csvCert in csvCertificates)
                {
                    try
                    {
                        bool update = true;
                        GiftCard giftCard = await _giftCardService.GetGiftCardByCouponCode(csvCert.RewardCode);

                        // if gift card isn't in the system and has a balance remaining then import into system
                        if (giftCard == null && csvCert.AmountRemaining > 0)
                        {
                            update = false;
                            giftCard = new GiftCard();
                        }

                        // if gift card is in the system (update or create) then set properties
                        if (giftCard != null)
                        {
                            giftCard.GiftCardTypeId = 0; // 0 = Virtual, 1 = Physical
                            giftCard.Amount = (decimal)csvCert.AmountRemaining;
                            giftCard.GiftCardCouponCode = csvCert.RewardCode;
                            giftCard.IsGiftCardActivated = csvCert.ExpiresOn.Date > DateTime.Now.Date && csvCert.AmountRemaining > 0;
                            giftCard.IsRewardsCertificate = true;
                            giftCard.RewardsAccountId = csvCert.ExtraValueCardNumber;
                            giftCard.CreatedOnUtc = DateTime.UtcNow; // added to allow updates to be tracked for cleanup
                            if (update)
                                await _giftCardService.UpdateGiftCardAsync(giftCard);
                            else
                                await _giftCardService.InsertGiftCardAsync(giftCard);
                        }
                    }
                    catch { } // skip any fails
                }
            }

            // Remove any unnecessary or expired rewards certificates
            List<GiftCard> giftCards = await _giftCardService.GetGiftCardsToRemoveAsync(DateTime.Now, true);
            foreach (GiftCard gc in giftCards)
            {
                try
                {
                    // only delete those without usage history
                    if (gc.GiftCardUsageHistory == null || gc.GiftCardUsageHistory.Count() == 0)
                       await _giftCardService.DeleteGiftCardAsync(gc);
                    else
                    {
                        gc.IsGiftCardActivated = false;
                        await _giftCardService.UpdateGiftCardAsync(gc);
                    }
                }
                catch { } // skip any fails
            }
        }
    }

    //public partial class RewardsCertificateEmailTask : IScheduleTask
    //{
    //    private readonly IGiftCardService _giftCardService;
    //    private readonly IWorkflowMessageService _workflowMessageService;
    //    private readonly LocalizationSettings _localizationSettings;
    //    private readonly IWebHelper _webHelper;

    //    public RewardsCertificateEmailTask(
    //        IGiftCardService giftCardService,
    //        IWorkflowMessageService workflowMessageService,
    //        LocalizationSettings localizationSettings,
    //        IWebHelper webHelper)
    //    {
    //        _giftCardService = giftCardService;
    //        _workflowMessageService = workflowMessageService;
    //        _localizationSettings = localizationSettings;
    //        _webHelper = webHelper;
    //    }

    //    public async Task ExecuteAsync()
    //    {
    //        var failedCerts = new List<ImportRewardsCertificate>();
    //        int count = 0;

    //        string filePath = $@"D:\ftp.unclebills.com\RewardsCertificates_{DateTime.Now:yyyy-MM-dd}.csv";

    //        using (var reader = new StreamReader(filePath))
    //        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
    //        {
    //            var csvCertificates = csv.GetRecordsAsync<ImportRewardsCertificate>();

    //            await foreach (var csvCert in csvCertificates)
    //            {
    //                try
    //                {
    //                    var giftCard = await _giftCardService.GetGiftCardByCouponCode(csvCert.RewardCode);

    //                    if (giftCard == null || string.IsNullOrEmpty(csvCert.CustomerEmail))
    //                    {
    //                        failedCerts.Add(csvCert);
    //                        continue;
    //                    }

    //                    bool closeToExpiration = csvCert.ExpiresOn <= DateTime.Now.AddDays(14);
    //                    string barcodeName = $"{DateTime.Now:yyyyMMdd}_{count}_{csvCert.RewardCode}.png";
    //                    string barcodePath = $@"D:\barcodes\{barcodeName}";

    //                    // Generate barcode with ZXing.Net
    //                    await GenerateBarcodeAsync(csvCert.RewardCode, barcodePath);

    //                    // Send notification asynchronously
    //                    await _workflowMessageService.SendRewardCertificateNotification(
    //                        barcodeName,
    //                        barcodePath,
    //                        csvCert,
    //                        closeToExpiration,
    //                        _localizationSettings.DefaultAdminLanguageId
    //                    );
    //                    count++;
    //                }
    //                catch
    //                {
    //                    failedCerts.Add(csvCert);
    //                }
    //            }
    //        }

    //        // Process failed certificates
    //        if (failedCerts.Any())
    //        {
    //            string fileName = $"FailedCertificates_{DateTime.Now:yyyy-MM-dd}.csv";
    //            string failedFilePath = $@"D:\failedcerts\{fileName}";

    //            await using (var writer = new StreamWriter(failedFilePath, false, Encoding.UTF8))
    //            await using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
    //            {
    //                await csv.WriteRecordsAsync(failedCerts);
    //            }

    //            await _workflowMessageService.SendFailedCertsNotification(
    //                fileName,
    //                failedFilePath,
    //                _localizationSettings.DefaultAdminLanguageId
    //            );
    //        }
    //    }

    //    private async Task GenerateBarcodeAsync(string code, string outputPath)
    //    {
    //        var barcodeWriter = new BarcodeWriterPixelData
    //        {
    //            Format = BarcodeFormat.CODE_39,
    //            Options = new EncodingOptions
    //            {
    //                Height = 120,
    //                Width = 290,
    //                Margin = 10,
    //                PureBarcode = false
    //            }
    //        };

    //        var pixelData = barcodeWriter.Write(code);

    //        await using (var bitmap = new System.Drawing.Bitmap(
    //            pixelData.Width,
    //            pixelData.Height,
    //            System.Drawing.Imaging.PixelFormat.Format32bppRgb))
    //        {
    //            var bitmapData = bitmap.LockBits(
    //                new Rectangle(0, 0, pixelData.Width, pixelData.Height),
    //                ImageLockMode.WriteOnly,
    //                System.Drawing.Imaging.PixelFormat.Format32bppRgb);

    //            try
    //            {
    //                Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
    //            }
    //            finally
    //            {
    //                bitmap.UnlockBits(bitmapData);
    //            }

    //            await using (var stream = new FileStream(outputPath, FileMode.Create))
    //            {
    //                bitmap.Save(stream, ImageFormat.Png);
    //            }
    //        }
    //    }
    //}

    //public partial class RewardsCertificateEmailTask : IScheduleTask
    //{
    //    private readonly IGiftCardService _giftCardService;
    //    private readonly IWorkflowMessageService _workflowMessageService;
    //    private readonly LocalizationSettings _localizationSettings;

    //    public RewardsCertificateEmailTask(IGiftCardService giftCardService, IWorkflowMessageService workflowMessageService, LocalizationSettings localizationSettings)
    //    {
    //        this._giftCardService = giftCardService;
    //        this._workflowMessageService = workflowMessageService;
    //        this._localizationSettings = localizationSettings;
    //    }

    //    public async Task ExecuteAsync()
    //    {
    //        var failedCerts = new List<ImportRewardsCertificate>();
    //        int count = 0;

    //        var file = File.OpenRead(@"D:\ftp.unclebills.com\RewardsCertificates_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv");


    //        using (var txtRdr = new StreamReader(file))
    //        using (var csvRdr = new CsvReader(txtRdr, System.Globalization.CultureInfo.InvariantCulture))
    //        {


    //            List<ImportRewardsCertificate> csvCertificates = csvRdr.GetRecords<ImportRewardsCertificate>().ToList();
    //            foreach (ImportRewardsCertificate csvCert in csvCertificates)
    //            {
    //                try
    //                {
    //                    GiftCard giftCard = await _giftCardService.GetGiftCardByCouponCode(csvCert.RewardCode);

    //                    if (giftCard == null || String.IsNullOrEmpty(csvCert.CustomerEmail))
    //                    {
    //                        //add log entry for null gift card/nonexistent email
    //                        failedCerts.Add(csvCert);
    //                        continue;
    //                    }

    //                    bool closeToExpiration = csvCert.ExpiresOn <= DateTime.Now.AddDays(14);

    //                    //generate the barcode image
    //                    string barcodeName = DateTime.Now.ToString("yyyyMMdd") + "_" + count + "_" + csvCert.RewardCode + ".png";
    //                    string barcodePath = @"D:\barcodes\" + barcodeName;
    //                    var b = new Barcode();
    //                    b.IncludeLabel = true;
    //                    var img = b.Encode(TYPE.CODE39, csvCert.RewardCode, 290, 120);
    //                    img.Save(barcodePath);

    //                    await _workflowMessageService.SendRewardCertificateNotification(barcodeName, barcodePath, csvCert, closeToExpiration, _localizationSettings.DefaultAdminLanguageId);
    //                    count++;


    //                }
    //                catch
    //                {
    //                    // add log entry with error message?
    //                }
    //            }
    //        }

    //        if (failedCerts.Any())
    //        {
    //            //create the csv file
    //            string fileName = "FailedCertificates_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv";
    //            string filePath = @"D:\failedcerts\" + fileName;

    //            using (TextWriter writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
    //            {
    //                var csv = new CsvWriter(writer);
    //                csv.WriteRecords(failedCerts);
    //            }

    //            //send the failed certificates email
    //            await _workflowMessageService.SendFailedCertsNotification(fileName, filePath, _localizationSettings.DefaultAdminLanguageId);
    //        }
    //    }
    //}


    public partial class RewardsCertificateEmailTask : IScheduleTask
    {
        private readonly IGiftCardService _giftCardService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;

        public RewardsCertificateEmailTask(
            IGiftCardService giftCardService,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings)
        {
            _giftCardService = giftCardService;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
        }

        public async Task ExecuteAsync()
        {
            var failedCerts = new List<ImportRewardsCertificate>();
            int count = 0;
            string today = DateTime.Now.ToString("yyyy-MM-dd");

            string inputFilePath = $@"D:\ftp.unclebills.com\RewardsCertificates_{today}.csv";

            using (var reader = new StreamReader(inputFilePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var csvCertificates = csv.GetRecords<ImportRewardsCertificate>();

                foreach (var csvCert in csvCertificates)
                {
                    try
                    {
                        var giftCard = await _giftCardService.GetGiftCardByCouponCode(csvCert.RewardCode);
                        if (giftCard == null || string.IsNullOrEmpty(csvCert.CustomerEmail))
                        {
                            failedCerts.Add(csvCert);
                            continue;
                        }

                        bool closeToExpiration = csvCert.ExpiresOn <= DateTime.Now.AddDays(14);
                        string barcodeName = $"{DateTime.Now:yyyyMMdd}_{count}_{csvCert.RewardCode}.png";
                        string barcodePath = $@"D:\barcodes\{barcodeName}";

                        // Generate barcode using ZXing.Net with ImageSharp
                        GenerateBarcodeAsync(csvCert.RewardCode, barcodePath);

                        await _workflowMessageService.SendRewardCertificateNotification(
                            barcodeName,
                            barcodePath,
                            csvCert,
                            closeToExpiration,
                            _localizationSettings.DefaultAdminLanguageId
                        );
                        count++;
                    }
                    catch (Exception ex)
                    {
                        // Log error and add to failed certs
                        failedCerts.Add(csvCert);
                    }
                }
            }

            if (failedCerts.Count > 0)
            {
                string fileName = $"FailedCertificates_{today}.csv";
                string filePath = $@"D:\failedcerts\{fileName}";

                await using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
                await using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    await csv.WriteRecordsAsync(failedCerts);
                }

                await _workflowMessageService.SendFailedCertsNotification(
                    fileName,
                    filePath,
                    _localizationSettings.DefaultAdminLanguageId
                );
            }
        }

        private void GenerateBarcodeAsync(string code, string outputPath)
        {
            var barcode = new BarcodeLib.Barcode();
            barcode.IncludeLabel = true;
            barcode.LabelPosition = LabelPositions.BOTTOMCENTER;

            // Customize barcode appearance
            barcode.Alignment = AlignmentPositions.CENTER;
            barcode.Width = 290;
            barcode.Height = 120;
            barcode.BackColor = Color.White;
            barcode.ForeColor = Color.Black;

            // Generate CODE39 barcode
            var image = barcode.Encode(
                TYPE.CODE39,
                code,
                barcode.Width,
                barcode.Height
            );

            // Save image synchronously
            image.Save(outputPath, ImageFormat.Png);
        }
    }
    public partial class GiftCardImportTask : IScheduleTask
    {
        private readonly IGiftCardService _giftCardService;

        public GiftCardImportTask(IGiftCardService giftCardService)
        {
            this._giftCardService = giftCardService;
        }

        public async Task ExecuteAsync()
        {
            var file = File.OpenRead(@"D:\ftp.unclebills.com\GiftCards_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv");


            using (var txtRdr = new StreamReader(file))
            using (var csvRdr = new CsvReader(txtRdr, System.Globalization.CultureInfo.InvariantCulture))
            {
                List<ImportGiftCard> csvCards = csvRdr.GetRecords<ImportGiftCard>().ToList();
                foreach (ImportGiftCard csvCard in csvCards)
                {
                    try
                    {
                        bool update = true;
                        GiftCard giftCard = await _giftCardService.GetGiftCardByCouponCode(csvCard.GiftCardCode);
                        if (giftCard == null)
                        {
                            update = false;
                            giftCard = new GiftCard();
                        }

                        giftCard.GiftCardTypeId = 0; // 0 = Virtual, 1 = Physical
                        giftCard.Amount = (decimal)csvCard.AmountRemaining;
                        giftCard.GiftCardCouponCode = csvCard.GiftCardCode;
                        giftCard.IsGiftCardActivated = csvCard.AmountRemaining > 0;
                        giftCard.IsRewardsCertificate = false;
                        giftCard.CreatedOnUtc = DateTime.UtcNow; // added to allow updates to be tracked for cleanup
                        if (update)
                            await _giftCardService.UpdateGiftCardAsync(giftCard);
                        else
                            await _giftCardService.InsertGiftCardAsync(giftCard);
                    }
                    catch { } // skip any fails
                }
            }

            // Remove any unnecessary or expired gift cards
            List<GiftCard> giftCards = await _giftCardService.GetGiftCardsToRemoveAsync(DateTime.Now, false);
            foreach (GiftCard gc in giftCards)
            {
                try
                {
                    // only delete those without usage history
                    if (gc.GiftCardUsageHistory == null || gc.GiftCardUsageHistory.Count() == 0)
                        await _giftCardService.DeleteGiftCardAsync(gc);
                    else
                    {
                        gc.IsGiftCardActivated = false;
                        await _giftCardService.UpdateGiftCardAsync(gc);
                    }
                }
                catch { } // skip any fails
            }
        }
    }

    public partial class PromoDiscountsImportTask : IScheduleTask
    {
        private readonly IDiscountService _discountService;
        private readonly IProductService _productService;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerService _customerService;

        public PromoDiscountsImportTask(IDiscountService discountService,
            IProductService productService,
            ISettingService settingService,
            ILocalizationService localizationService,
            ICustomerService customerService)
        {
            this._discountService = discountService;
            this._productService = productService;
            this._settingService = settingService;
            this._localizationService = localizationService;
            this._customerService = customerService;
        }

        public async Task ExecuteAsync()
        {
            var file = File.OpenRead(@"D:\ftp.unclebills.com\PromoDiscounts_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt");
            using (TextReader txtRdr = new StreamReader(file))
            {
                string jsonString = txtRdr.ReadToEnd();
                List<ImportPromoDiscount> jsonPromoDiscounts = JsonConvert.DeserializeObject<List<ImportPromoDiscount>>(jsonString);
                foreach (ImportPromoDiscount jsonDiscount in jsonPromoDiscounts)
                {
                    try
                    {
                        // determine if Skus even exist - no reason to create discount if the products are not in the online store
                        var trimmedSkus = jsonDiscount.DiscountedSkus.Select(s => s.Trim());
                        var discProducts = await _productService.GetProductsBySkuAsync(trimmedSkus.ToArray());
                        if (discProducts == null || discProducts.Count() < 1)
                        {
                            // delete if already had been created before this rule to check if discounted skus are on the website
                            Discount d = await _discountService.GetDiscountByCode(jsonDiscount.Title);
                            if (d != null)
                            {
                                await _discountService.DeleteDiscountAsync(d);
                            }
                        }
                        else
                        {

                            // Step 1 - The Discount Definition
                            bool update = true;
                            Discount discount = await _discountService.GetDiscountByCode(jsonDiscount.Title);
                            if (discount == null)
                            {
                                update = false;
                                discount = new Discount();
                            }

                            discount.Name = jsonDiscount.Title;
                            discount.CouponCode = jsonDiscount.Title;
                            discount.DiscountTypeId = 2;
                            discount.DiscountType = DiscountType.AssignedToSkus;
                            discount.UsePercentage = true;
                            discount.DiscountPercentage = Convert.ToDecimal(jsonDiscount.DiscValue);
                            discount.MaximumDiscountedQuantity = jsonDiscount.DiscountedQty;
                            discount.StartDateUtc = jsonDiscount.StartDate;
                            discount.EndDateUtc = DateTime.UtcNow.AddDays(-2); // jsonDiscount.EndDate; // On 8-21-18, this change made every discount expired 
                                                                               // this was done as a temp fix for the discount bug reported
                                                                               // see active collab task #292
                            discount.RequiresCouponCode = false;
                            discount.DiscountLimitation = DiscountLimitationType.Unlimited;
                            discount.DiscountLimitationId = 0;

                            if (update)
                                await _discountService.UpdateDiscountAsync(discount);
                            else
                                await _discountService.InsertDiscountAsync(discount);

                            // Step 2 - Required Products

                           
                            var defaultGroup = (await _discountService.GetAllDiscountRequirementsAsync(discount.Id, true)).FirstOrDefault(requirement => !requirement.ParentId.HasValue && requirement.IsGroup);
                           
                            //var defaultGroup = discount.DiscountRequirements.FirstOrDefault(requirement => !requirement.ParentId.HasValue && requirement.IsGroup);
                            if (defaultGroup == null)
                            {
                                //add default requirement group
                                defaultGroup = new DiscountRequirement()
                                {
                                    DiscountId = discount.Id,
                                    IsGroup = true,
                                    InteractionType = RequirementGroupInteractionType.And,
                                    DiscountRequirementRuleSystemName = await _localizationService.GetResourceAsync("Admin.Promotions.Discounts.Requirements.DefaultRequirementGroup")
                                };

                                //add default requirement group
                                await _discountService.InsertDiscountRequirementAsync(defaultGroup);
                                 
                            }

                            // new requirement - has one of these products in cart


                            var discountRequirement = (await _discountService.GetAllDiscountRequirementsAsync(discount.Id, true)).FirstOrDefault(requirement => requirement.DiscountRequirementRuleSystemName == "DiscountRequirement.HasOneProduct");


                            //var discountRequirement = discount.DiscountRequirements.FirstOrDefault(requirement => requirement.DiscountRequirementRuleSystemName == "DiscountRequirement.HasOneProduct");
                            if (discountRequirement == null)
                            {
                                
                                discountRequirement = new DiscountRequirement()
                                {
                                    DiscountId = discount.Id,
                                    IsGroup = false,
                                    DiscountRequirementRuleSystemName = "DiscountRequirement.HasOneProduct",
                                    ParentId = defaultGroup.Id,
                                    InteractionType = RequirementGroupInteractionType.And
                                };
                                await _discountService.InsertDiscountRequirementAsync(discountRequirement);

                            }

                            // add required skus to setting service
                            string reqSkus = "";
                            foreach (string sku in jsonDiscount.RequiredSkus)
                            {
                                var product = await _productService.GetProductBySkuAsync(sku);
                                if (product != null)
                                {
                                    if (reqSkus.Length > 0)
                                        reqSkus += ",";

                                    reqSkus += product.Id.ToString();

                                    int reqQty = jsonDiscount.RequiredQty;
                                    if (jsonDiscount.DiscountedSkus.Contains(sku))
                                        reqQty += 1;

                                    if (reqQty > 1)
                                        reqSkus += ":" + reqQty;
                                }
                            }
                            _settingService.SetSetting(string.Format("DiscountRequirement.RestrictedProductIds-{0}", discountRequirement.Id), reqSkus);

                            // must be in customer role registered

                            var discountRequirement2 = (await _discountService.GetAllDiscountRequirementsAsync(discount.Id, true)).FirstOrDefault(requirement => requirement.DiscountRequirementRuleSystemName == "DiscountRequirement.MustBeAssignedToCustomerRole");

                            if (discountRequirement2 == null)
                            {
                                discountRequirement2 = new DiscountRequirement()
                                {
                                    IsGroup = false,
                                    DiscountRequirementRuleSystemName = "DiscountRequirement.MustBeAssignedToCustomerRole",
                                    ParentId = defaultGroup.Id,
                                    DiscountId = discount.Id,                         
                                    InteractionType = RequirementGroupInteractionType.And
                                };
                                await _discountService.InsertDiscountRequirementAsync(discountRequirement2);
                            }

                            // add required role to setting service
                            var roles = await _customerService.GetAllCustomerRolesAsync();
                            var role = roles.FirstOrDefault(x => x.SystemName == "Registered");
                            if (role != null)
                            {
                                _settingService.SetSetting(string.Format("DiscountRequirement.MustBeAssignedToCustomerRole-{0}", discountRequirement2.Id), role.Id);
                            }

                            // Step 3 - Discounted Products
                           

                            foreach (var sku in jsonDiscount.DiscountedSkus)
                            {
                                var product = await _productService.GetProductBySkuAsync(sku);
                                if (product == null)
                                    continue; // skip non-existent SKUs

                                // Check if mapping already exists to avoid duplicates.
                                var existingMapping = await _productService.GetDiscountAppliedToProductAsync(product.Id, discount.Id);
                                if (existingMapping == null)
                                {
                                    var mapping = new DiscountProductMapping
                                    {
                                        DiscountId = discount.Id,
                                        EntityId = product.Id
                                    };
                                    await _productService.InsertDiscountProductMappingAsync(mapping);
                                }
                            }


                            
                        }
                    }
                    catch
                    {
                        // ignored
                    } // skip any fails
                }
            }
        }
    }

    public partial class FrequentBuyerImportTask : IScheduleTask
    {
        private readonly ICpDiscountService _cpDiscountService;

        public FrequentBuyerImportTask(ICpDiscountService cpDiscountService)
        {
            this._cpDiscountService = cpDiscountService;
        }

        public async Task ExecuteAsync()
        {
            var file = File.OpenRead(@"D:\ftp.unclebills.com\FrequentBuyerPrograms_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt");
            using (TextReader txtRdr = new StreamReader(file))
            {
                string jsonString = txtRdr.ReadToEnd();
                List<ImportFrequentBuyerProgram> jsonPrograms = JsonConvert.DeserializeObject<List<ImportFrequentBuyerProgram>>(jsonString);
                foreach (ImportFrequentBuyerProgram jsonFBP in jsonPrograms)
                {
                    try
                    {
                        // Get any existing database records for this program
                        List<CpDiscountProduct> existingFBPproducts = _cpDiscountService.GetCpDiscountProducts(jsonFBP.LoyaltyCode).Result.ToList();
                        // Remove any skus in database that are not in the import
                        existingFBPproducts.ForEach(discProd =>
                        {
                            if (!jsonFBP.ProductSkus.Contains(discProd.ProductSku))
                            { _cpDiscountService.DeleteCpDiscountProduct(discProd); }
                        });
                        // Add each sku that doesn't already exist
                        jsonFBP.ProductSkus.ForEach(prodSku =>
                        {
                            CpDiscountProduct progProduct = _cpDiscountService.GetCpDiscountProduct(jsonFBP.LoyaltyCode, prodSku).Result;
                            if (progProduct == null)
                            {
                                progProduct = new CpDiscountProduct() { LoyaltyCode = jsonFBP.LoyaltyCode, ProductSku = prodSku };
                                _cpDiscountService.InsertCpDiscountProduct(progProduct);
                            }
                        });
                    }
                    catch { } // skip any fails
                }

                await Task.Yield();
            }
        }
    }

    //public partial class MultilineDiscountImportTask : IScheduleTask
    //{
    //    private readonly IProductService _productService;
    //    private readonly ISettingService _settingService;
    //    private readonly IUrlRecordService _urlRecordService;
    //    private readonly ICategoryService _categoryService;
    //    private readonly IProductAttributeService _productAttributeService;

    //    public MultilineDiscountImportTask(IProductService productService,
    //        ISettingService settingService,
    //        IUrlRecordService urlRecordService,
    //        ICategoryService categoryService,
    //        IProductAttributeService productAttributeService)
    //    {
    //        this._productService = productService;
    //        this._settingService = settingService;
    //        this._urlRecordService = urlRecordService;
    //        this._categoryService = categoryService;
    //    }

    //    public async Task ExecuteAsync()
    //    {
    //        // Get kit category id
    //        int kitCategoryId = 0; // assume missing
    //        var pluginSettings = _settingService.LoadSetting<ProductKitsPluginSettings>();
    //        if (pluginSettings != null)
    //            kitCategoryId = pluginSettings.CategoryId;

    //        // Need categories to check if relationship already exists
    //        var allProductCategories = await _categoryService.GetAllProductCategoryAsync();
    //        allProductCategories = allProductCategories.Where(x => x.CategoryId == kitCategoryId);

    //        var file = File.OpenRead(@"D:\ftp.unclebills.com\MultilineKits_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt");
    //        using (TextReader txtRdr = new StreamReader(file))
    //        {
    //            string jsonString = txtRdr.ReadToEnd();
    //            List<ImportMultilineKit> jsonKitProducts = JsonConvert.DeserializeObject<List<ImportMultilineKit>>(jsonString);
    //            // flat file where multiple records should be tied together - therefore select distinct groups and loop through those
    //            List<string> kitCodes = jsonKitProducts.Select(x => x.KitCode).Distinct().ToList();
    //            foreach (string kitCode in kitCodes)
    //            {
    //                try
    //                {
    //                    bool deletedAny = false;
    //                    bool addedAny = false;

    //                    // get all import products related to this group
    //                    List<ImportMultilineKit> kitProducts = jsonKitProducts.Where(x => x.KitCode == kitCode).ToList();

    //                    bool update = true;
    //                    Product kitProduct = await _productService.GetProductBySkuAsync(kitCode);
    //                    if (kitProduct == null)
    //                    {
    //                        update = false;
    //                        kitProduct = new Product();
    //                        kitProduct.CreatedOnUtc = DateTime.UtcNow;
    //                    }
    //                    // update all simple properties
    //                    kitProduct.ProductTypeId = 5; // Simple Product = 5; Grouped Product = 10
    //                    kitProduct.ParentGroupedProductId = 0;
    //                    kitProduct.VisibleIndividually = false;
    //                    kitProduct.Name = kitCode;
    //                    kitProduct.ShortDescription = kitProducts.First().KitDescription;
    //                    kitProduct.VendorId = 0;
    //                    kitProduct.ProductTemplateId = 1;
    //                    kitProduct.ShowOnHomepage = false;
    //                    kitProduct.MetaKeywords = string.Empty;
    //                    kitProduct.MetaDescription = string.Empty;
    //                    kitProduct.MetaTitle = string.Empty;
    //                    kitProduct.AllowCustomerReviews = false;
    //                    kitProduct.Published = true;
    //                    kitProduct.Sku = kitCode;
    //                    kitProduct.ManufacturerPartNumber = string.Empty;
    //                    kitProduct.Gtin = string.Empty;
    //                    kitProduct.IsGiftCard = false;
    //                    kitProduct.GiftCardTypeId = 0;
    //                    kitProduct.OverriddenGiftCardAmount = 0;
    //                    kitProduct.RequireOtherProducts = false;
    //                    kitProduct.RequiredProductIds = string.Empty;
    //                    kitProduct.AutomaticallyAddRequiredProducts = false;
    //                    kitProduct.IsDownload = false;
    //                    kitProduct.DownloadId = 0;
    //                    kitProduct.UnlimitedDownloads = false;
    //                    kitProduct.MaxNumberOfDownloads = 0;
    //                    kitProduct.DownloadActivationTypeId = 0;
    //                    kitProduct.HasSampleDownload = false;
    //                    kitProduct.SampleDownloadId = 0;
    //                    kitProduct.HasUserAgreement = false;
    //                    kitProduct.UserAgreementText = string.Empty;
    //                    kitProduct.IsRecurring = false;
    //                    kitProduct.RecurringCycleLength = 0;
    //                    kitProduct.RecurringCyclePeriodId = 0;
    //                    kitProduct.RecurringTotalCycles = 0;
    //                    kitProduct.IsRental = false;
    //                    kitProduct.RentalPriceLength = 0;
    //                    kitProduct.RentalPricePeriodId = 0;
    //                    kitProduct.IsShipEnabled = true;
    //                    kitProduct.IsPickupOnly = false;
    //                    kitProduct.HasHiddenPrice = false;
    //                    kitProduct.IsFreeShipping = false;
    //                    kitProduct.ShipSeparately = false;
    //                    kitProduct.AdditionalShippingCharge = 0;
    //                    kitProduct.DeliveryDateId = 0;
    //                    kitProduct.IsTaxExempt = false;
    //                    kitProduct.TaxCategoryId = 6; // 6 = Taxable Item
    //                    kitProduct.IsTelecommunicationsOrBroadcastingOrElectronicServices = false;
    //                    kitProduct.ManageInventoryMethodId = 0; // Don't manage stock = 0
    //                    kitProduct.UseMultipleWarehouses = false;
    //                    kitProduct.WarehouseId = 0;
    //                    kitProduct.StockQuantity = 0;
    //                    kitProduct.DisplayStockAvailability = false;
    //                    kitProduct.DisplayStockQuantity = false;
    //                    kitProduct.MinStockQuantity = 0;
    //                    kitProduct.LowStockActivityId = 0; // Nothing
    //                    kitProduct.NotifyAdminForQuantityBelow = 0;
    //                    kitProduct.BackorderModeId = 0; // No Backorders
    //                    kitProduct.AllowBackInStockSubscriptions = false;
    //                    kitProduct.OrderMinimumQuantity = 1;
    //                    kitProduct.OrderMaximumQuantity = 10000;
    //                    kitProduct.AllowedQuantities = string.Empty;
    //                    kitProduct.AllowAddingOnlyExistingAttributeCombinations = false;
    //                    kitProduct.DisableBuyButton = false;
    //                    kitProduct.DisableWishlistButton = false;
    //                    kitProduct.AvailableForPreOrder = false;
    //                    kitProduct.PreOrderAvailabilityStartDateTimeUtc = DateTime.UtcNow;
    //                    kitProduct.CallForPrice = false;
    //                    kitProduct.Price = Convert.ToDecimal(kitProducts.First().KitPrice);
    //                    kitProduct.OldPrice = 0;
    //                    kitProduct.ProductCost = 0;
    //                    kitProduct.CustomerEntersPrice = false;
    //                    kitProduct.MinimumCustomerEnteredPrice = 0;
    //                    kitProduct.MaximumCustomerEnteredPrice = 0;
    //                    kitProduct.BasepriceEnabled = false;
    //                    kitProduct.BasepriceAmount = 0;
    //                    kitProduct.BasepriceUnitId = 0;
    //                    kitProduct.BasepriceBaseAmount = 0;
    //                    kitProduct.BasepriceBaseUnitId = 0;
    //                    kitProduct.MarkAsNew = false;
    //                    kitProduct.MarkAsNewStartDateTimeUtc = DateTime.UtcNow;
    //                    kitProduct.MarkAsNewEndDateTimeUtc = DateTime.UtcNow;
    //                    kitProduct.Weight = 0;
    //                    kitProduct.Length = 0;
    //                    kitProduct.Width = 0;
    //                    kitProduct.Height = 0;

    //                    // save changes to database
    //                    kitProduct.UpdatedOnUtc = DateTime.UtcNow;
    //                    if (!update)
    //                        await _productService.InsertProductAsync(kitProduct);
    //                    else
    //                        await _productService.UpdateProductAsync(kitProduct);
                       
    //                    var productSeName = await _urlRecordService.GetSeNameAsync(kitProduct);

    //                    // update slug
    //                    if (!update || string.IsNullOrWhiteSpace(productSeName))
    //                    {
    //                        //search engine name
    //                        var seName = await _urlRecordService.ValidateSeNameAsync(kitProduct, string.Empty, kitProduct.Name, true);
    //                        await _urlRecordService.SaveSlugAsync(kitProduct, seName, 0);
    //                    }


    //                    // ensure placement in kit category
    //                    if (!update || !allProductCategories.Any(x => x.ProductId == kitProduct.Id))
    //                    {
    //                        var productCategory = new ProductCategory
    //                        {
    //                            ProductId = kitProduct.Id,
    //                            CategoryId = kitCategoryId,
    //                            IsFeaturedProduct = false,
    //                            DisplayOrder = 1
    //                        };
    //                        await _categoryService.InsertProductCategoryAsync(productCategory);
    //                    }

    //                    // manage kit products
    //                    // if update, then might need to remove products from the kit?
    //                    if (update)
    //                    {
    //                        var attributeMappings = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(kitProduct.Id);
    //                        // get the products that are already in the kit - if not in import list then delete
    //                        foreach (ProductAttributeMapping pam in attributeMappings)
    //                        {
    //                            var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(pam.Id);
    //                            foreach (ProductAttributeValue pav in attributeValues)
    //                            {
    //                                if (pav.AttributeValueTypeId == (int)AttributeValueType.AssociatedToProduct)
    //                                {
    //                                    // get this product
    //                                    var p = await _productService.GetProductByIdAsync(pav.AssociatedProductId);
    //                                    // if this product isn't in the import list, we must remove it from the kit
    //                                    if (p != null && !kitProducts.Exists(x => x.ProductSku == p.Sku))
    //                                    {
    //                                        // remove selected product from the kit
    //                                        var prodAttrMap = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(kitProduct.Id);


    //                                        // var prodAttrMaps = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(kitProduct.Id);

    //                                        ProductAttributeMapping pam3 = null;
    //                                        foreach (var pam2 in prodAttrMap)
    //                                        {
    //                                            // Fetch attribute values for each mapping
    //                                            var pavs = await _productAttributeService.GetProductAttributeValuesAsync(pam2.Id);

    //                                            // Filter for AssociatedToProduct type
    //                                            var associatedValue = pavs.Where(v => v.AttributeValueType == AttributeValueType.AssociatedToProduct && v.AssociatedProductId == p.Id).FirstOrDefault();
    //                                            if (associatedValue != null)
    //                                            {
    //                                                pam3 = pam2;
    //                                                await _productAttributeService.DeleteProductAttributeValueAsync(associatedValue);
    //                                            }
    //                                        }

    //                                        if (pam3 != null)
    //                                        {
    //                                            await _productAttributeService.DeleteProductAttributeMappingAsync(pam3);
    //                                            deletedAny = true;
    //                                        }

    //                                    }
    //                                }
    //                            }
    //                        }
    //                    }

    //                    // for all imports, we perform the add/exists check
    //                    foreach (ImportMultilineKit kProd in kitProducts)
    //                    {
    //                        Product p = await _productService.GetProductBySkuAsync(kProd.ProductSku);
    //                        if (p != null)
    //                        {

    //                            var attributeMappings = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(kitProduct.Id);

    //                            foreach (ProductAttributeMapping pam in attributeMappings)
    //                            {
    //                                var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(pam.Id);

    //                                if (attributeValues.FirstOrDefault(x => x.AttributeValueTypeId == (int)AttributeValueType.AssociatedToProduct && x.AssociatedProductId == p.Id) == null)
    //                                {
    //                                    // add product to kit
    //                                    // Product Attribute
    //                                    var prodAttrList = _productAttributeService.GetAllProductAttributesAsync().Result.ToList();
    //                                    prodAttrList = prodAttrList.Where(a => a.Name == "Multiline").ToList();
    //                                    var prodAttr = prodAttrList.FirstOrDefault();

    //                                    if (prodAttr == null)
    //                                    {
    //                                        // create new attribute
    //                                        prodAttr = new ProductAttribute();
    //                                        prodAttr.Name = "Multiline";
    //                                        await _productAttributeService.InsertProductAttributeAsync(prodAttr);
    //                                    }

    //                                    // Product Attribute Mapping
    //                                    var prodAttrMap = new ProductAttributeMapping();
    //                                    prodAttrMap.ProductId = kitProduct.Id;
    //                                    prodAttrMap.ProductAttributeId = prodAttr.Id;
    //                                    prodAttrMap.IsRequired = true;
    //                                    prodAttrMap.AttributeControlTypeId = (int)AttributeControlType.ReadonlyCheckboxes;
    //                                    prodAttrMap.TextPrompt = prodAttr.Name;
    //                                    prodAttrMap.DisplayOrder = 0;
    //                                    await _productAttributeService.InsertProductAttributeMappingAsync(prodAttrMap);

    //                                    // Product Attribute Value
    //                                    var prodAttrVal = new ProductAttributeValue();
    //                                    prodAttrVal.ProductAttributeMappingId = prodAttrMap.Id;
    //                                    prodAttrVal.Name = p.Name;
    //                                    prodAttrVal.AttributeValueTypeId = (int)AttributeValueType.AssociatedToProduct;
    //                                    prodAttrVal.AssociatedProductId = p.Id;
    //                                    prodAttrVal.Quantity = kProd.ProductQty;
    //                                    prodAttrVal.IsPreSelected = true;
    //                                   await _productAttributeService.InsertProductAttributeValueAsync(prodAttrVal);

    //                                    addedAny = true;
    //                                }
    //                            }
    //                        }
    //                    }
    //                    // Clean up data if anything has been changed
    //                    if (deletedAny || addedAny)
    //                    {
    //                        // Remove Existing Product Attribute Combinations because they don't include the newly added PAV
    //                        var prodAttrCombos = await _productAttributeService.GetAllProductAttributeCombinationsAsync(kitProduct.Id);
    //                        foreach (ProductAttributeCombination pac in prodAttrCombos)
    //                        {
    //                            await _productAttributeService.DeleteProductAttributeCombinationAsync(pac);
    //                        }
    //                        // Create the Product Attribute Combination
    //                        var prodAttrCombo = new ProductAttributeCombination();
    //                        prodAttrCombo.ProductId = kitProduct.Id;
    //                        prodAttrCombo.StockQuantity = 10000;
    //                        prodAttrCombo.OverriddenPrice = kitProduct.Price;
    //                        prodAttrCombo.AllowOutOfStockOrders = false;
    //                        prodAttrCombo.NotifyAdminForQuantityBelow = 1;
                          


    //                        // 3. Fetch attribute mappings
    //                        var prodAttrMaps = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(kitProduct.Id);

    //                        string attrXml = "<Attributes>";
    //                        foreach (var pam in prodAttrMaps)
    //                        {
    //                            // Fetch attribute values for each mapping
    //                            var pavs = await _productAttributeService.GetProductAttributeValuesAsync(pam.Id);

    //                            // Filter for AssociatedToProduct type
    //                            var associatedValue = pavs.FirstOrDefault(v => v.AttributeValueType == AttributeValueType.AssociatedToProduct);
    //                            if (associatedValue != null)
    //                            {
    //                                attrXml += $"<ProductAttribute ID=\"{pam.Id}\">" +
    //                                           $"<ProductAttributeValue><Value>{associatedValue.Id}</Value></ProductAttributeValue>" +
    //                                           $"</ProductAttribute>";
    //                            }
    //                        }
    //                        attrXml += "</Attributes>";

    //                        prodAttrCombo.AttributesXml = attrXml;

    //                        // 4. Insert new combination
    //                        await _productAttributeService.InsertProductAttributeCombinationAsync(prodAttrCombo);
    //                    }
    //                }
    //                catch { } // skip any fails
    //            }
    //        }
    //    }
    //}

    public partial class ProductOutOfStockTask : IScheduleTask
    {
        private readonly IProductService _productService;

        public ProductOutOfStockTask(IProductService productService)
        {
            this._productService = productService;
        }

        public async Task ExecuteAsync()
        {
            await _productService.HideOutOfStockProducts();
        }
    }

    public partial class MultilineDiscountImportTask : IScheduleTask
    {
        private readonly IProductService _productService;
        private readonly ISettingService _settingService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ICategoryService _categoryService;
        private readonly IProductAttributeService _productAttributeService;

        public MultilineDiscountImportTask(
            IProductService productService,
            ISettingService settingService,
            IUrlRecordService urlRecordService,
            ICategoryService categoryService,
            IProductAttributeService productAttributeService)
        {
            _productService = productService;
            _settingService = settingService;
            _urlRecordService = urlRecordService;
            _categoryService = categoryService;
            _productAttributeService = productAttributeService;
        }

        public async Task ExecuteAsync()
        {
            // Get kit category id
            int kitCategoryId = 0;
            var pluginSettings = await _settingService.LoadSettingAsync<ProductKitsPluginSettings>();
            if (pluginSettings != null)
                kitCategoryId = pluginSettings.CategoryId;

            // Get all product categories for the kit category
            //var allProductCategories = (await _categoryService.GetAllProductCategoriesAsync(categoryId: kitCategoryId)).ToList();

            var allProductCategories = await _categoryService.GetAllProductCategoryAsync();
            allProductCategories = allProductCategories.Where(x => x.CategoryId == kitCategoryId);


            var filePath = @"D:\ftp.unclebills.com\MultilineKits_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
            string jsonString = await File.ReadAllTextAsync(filePath);
            List<ImportMultilineKit> jsonKitProducts = JsonConvert.DeserializeObject<List<ImportMultilineKit>>(jsonString);

            List<string> kitCodes = jsonKitProducts.Select(x => x.KitCode).Distinct().ToList();
            foreach (string kitCode in kitCodes)
            {
                try
                {
                    bool deletedAny = false;
                    bool addedAny = false;

                    List<ImportMultilineKit> kitProducts = jsonKitProducts.Where(x => x.KitCode == kitCode).ToList();

                    bool update = true;
                    Product kitProduct = await _productService.GetProductBySkuAsync(kitCode);
                    if (kitProduct == null)
                    {
                        update = false;
                        kitProduct = new Product { CreatedOnUtc = DateTime.UtcNow };
                    }

                    // Update product properties
                    kitProduct.ProductTypeId = (int)ProductType.GroupedProduct;
                    kitProduct.VisibleIndividually = false;
                    kitProduct.Name = kitCode;
                    kitProduct.ShortDescription = kitProducts.First().KitDescription;
                    kitProduct.Published = true;
                    kitProduct.Sku = kitCode;
                    kitProduct.Price = Convert.ToDecimal(kitProducts.First().KitPrice);
                    kitProduct.UpdatedOnUtc = DateTime.UtcNow;

                    // Set other properties as needed (omitted for brevity)
                    // ...

                    if (!update)
                        await _productService.InsertProductAsync(kitProduct);
                    else
                        await _productService.UpdateProductAsync(kitProduct);

                    // Update slug
                    if (!update || string.IsNullOrWhiteSpace(await _urlRecordService.GetSeNameAsync(kitProduct)))
                    {
                        var seName = await _urlRecordService.ValidateSeNameAsync(kitProduct, kitProduct.Name, kitProduct.Name, true);
                        await _urlRecordService.SaveSlugAsync(kitProduct, seName, 0);
                    }

                    // Ensure category placement
                    if (!update || !allProductCategories.Any(x => x.ProductId == kitProduct.Id))
                    {
                        await _categoryService.InsertProductCategoryAsync(new ProductCategory
                        {
                            ProductId = kitProduct.Id,
                            CategoryId = kitCategoryId,
                            IsFeaturedProduct = false,
                            DisplayOrder = 1
                        });
                    }

                    // Process kit products
                    if (update)
                    {
                        // Get all attribute mappings for this product
                        var attributeMappings = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(kitProduct.Id);
                        foreach (var pam in attributeMappings)
                        {
                            // Get values for each mapping separately
                            var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(pam.Id);
                            foreach (var pav in attributeValues)
                            {
                                if (pav.AttributeValueTypeId == (int)AttributeValueType.AssociatedToProduct)
                                {
                                    var p = await _productService.GetProductByIdAsync(pav.AssociatedProductId);
                                    if (p != null && !kitProducts.Exists(x => x.ProductSku == p.Sku))
                                    {
                                        await _productAttributeService.DeleteProductAttributeValueAsync(pav);
                                        deletedAny = true;

                                        // Delete mapping if no values remain
                                        var remainingValues = await _productAttributeService.GetProductAttributeValuesAsync(pam.Id);
                                        if (!remainingValues.Any())
                                        {
                                            await _productAttributeService.DeleteProductAttributeMappingAsync(pam);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Get or create the "Multiline" product attribute
                    var prodAttr = (await _productAttributeService.GetAllProductAttributesAsync())
                        .FirstOrDefault(a => a.Name == "Multiline")
                        ?? new ProductAttribute { Name = "Multiline" };

                    if (prodAttr.Id == 0)
                        await _productAttributeService.InsertProductAttributeAsync(prodAttr);

                    // Get or create attribute mapping
                    var prodAttrMap = (await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(kitProduct.Id))
                        .FirstOrDefault(m => m.ProductAttributeId == prodAttr.Id);

                    if (prodAttrMap == null)
                    {
                        prodAttrMap = new ProductAttributeMapping
                        {
                            ProductId = kitProduct.Id,
                            ProductAttributeId = prodAttr.Id,
                            IsRequired = true,
                            AttributeControlTypeId = (int)AttributeControlType.ReadonlyCheckboxes,
                            TextPrompt = prodAttr.Name,
                            DisplayOrder = 0
                        };
                        await _productAttributeService.InsertProductAttributeMappingAsync(prodAttrMap);
                    }

                    // Get existing values for this mapping
                    var existingValues = (await _productAttributeService.GetProductAttributeValuesAsync(prodAttrMap.Id))
                        .Where(v => v.AttributeValueTypeId == (int)AttributeValueType.AssociatedToProduct)
                        .ToList();

                    // Add new products to kit
                    foreach (ImportMultilineKit kProd in kitProducts)
                    {
                        Product p = await _productService.GetProductBySkuAsync(kProd.ProductSku);
                        if (p == null)
                            continue;

                        // Check if product already exists in kit
                        bool productExists = existingValues.Any(v => v.AssociatedProductId == p.Id);

                        if (!productExists)
                        {
                            var prodAttrVal = new ProductAttributeValue
                            {
                                ProductAttributeMappingId = prodAttrMap.Id,
                                Name = p.Name,
                                AttributeValueTypeId = (int)AttributeValueType.AssociatedToProduct,
                                AssociatedProductId = p.Id,
                                Quantity = kProd.ProductQty,
                                IsPreSelected = true
                            };
                            await _productAttributeService.InsertProductAttributeValueAsync(prodAttrVal);
                            addedAny = true;
                        }
                    }

                    // Update combinations if changes occurred
                    if (deletedAny || addedAny)
                    {
                        var combinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(kitProduct.Id);
                        foreach (var pac in combinations)
                        {
                            await _productAttributeService.DeleteProductAttributeCombinationAsync(pac);
                        }

                        var newCombination = new ProductAttributeCombination
                        {
                            ProductId = kitProduct.Id,
                            StockQuantity = 10000,
                            OverriddenPrice = kitProduct.Price,
                            AllowOutOfStockOrders = false,
                            NotifyAdminForQuantityBelow = 1
                        };

                        // Build attributes XML
                        var attributeXml = new StringBuilder("<Attributes>");

                        // Get all values for our specific attribute mapping
                        var allValues = await _productAttributeService.GetProductAttributeValuesAsync(prodAttrMap.Id);
                        foreach (var value in allValues)
                        {
                            attributeXml.Append($@"<ProductAttribute ID=""{prodAttrMap.Id}"">");
                            attributeXml.Append($@"<ProductAttributeValue><Value>{value.Id}</Value></ProductAttributeValue>");
                            attributeXml.Append("</ProductAttribute>");
                        }
                        attributeXml.Append("</Attributes>");

                        newCombination.AttributesXml = attributeXml.ToString();
                        await _productAttributeService.InsertProductAttributeCombinationAsync(newCombination);
                    }
                }
                catch
                {
                    // Consider adding logging here
                }
            }
        }
    }
    public partial class RelatedProductImportTask : IScheduleTask
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly INopDataProvider _dataProvider;
        protected readonly IRepository<RelatedProduct> _relatedProductRepository;
        public RelatedProductImportTask(IProductService productService, 
            ICategoryService categoryService,
            INopDataProvider dataProvider,
            IRepository<RelatedProduct> relatedProductRepository)
        {
            this._productService = productService;
            this._categoryService = categoryService;
            _dataProvider = dataProvider;
            _relatedProductRepository = relatedProductRepository;
        }
        public async Task ExecuteAsync()
        {
            var file = File.OpenRead(@"D:\ftp.unclebills.com\RelatedProducts_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv");
            //var file = File.OpenRead(@"E:\UBProductUpdates\related-products.csv"); 

            List<RelatedProduct> newRelatedProducts = new List<RelatedProduct>();

            using (TextReader txtRdr = new StreamReader(file))
            using (CsvReader csvRdr = new CsvReader(txtRdr, System.Globalization.CultureInfo.InvariantCulture))
            {

                List<ImportRelatedProduct> csvRelatedProducts = csvRdr.GetRecords<ImportRelatedProduct>().ToList();

                var allCategories = await _categoryService.GetAllCategoriesAsync();
                var allProductCategories = await _categoryService.GetAllProductCategoryAsync();

                foreach (ImportRelatedProduct rp in csvRelatedProducts)
                {
                    var category = allCategories.Where(a => a.Name == rp.Category);

                    IList<Product> products = new List<Product>();
                    products = await _productService.GetProductsBySkuAsync(rp.Skus.Split(','));

                    //var productsInCategory = allProductCategories.Where(async c =>  (await _categoryService.GetCategoryByIdAsync(c.CategoryId)).Name == rp.Category);

                    var productsInCategory = new List<ProductCategory>();
                    foreach (var c in allProductCategories)
                    {
                        var category2 = await _categoryService.GetCategoryByIdAsync(c.CategoryId);
                        if (category2.Name == rp.Category)
                        {
                            productsInCategory.Add(c);
                        }
                    }

                    foreach (var prod in productsInCategory)
                    {
                        foreach (var product in products)
                        {
                            RelatedProduct newRP = new RelatedProduct();
                            newRP.ProductId1 = prod.ProductId;
                            newRP.ProductId2 = product.Id;
                            newRP.DisplayOrder = 0;

                            newRelatedProducts.Add(newRP);
                        }
                    }
                }
            }

            // Delete all existing related products
            var existing = await _relatedProductRepository.Table.ToListAsync();
            foreach (var rel in existing)
                await _relatedProductRepository.DeleteAsync(rel);

            // Insert new related products
            foreach (var newRP in newRelatedProducts)
                await _relatedProductRepository.InsertAsync(newRP);

          
        }

        /// <summary>
        /// Deletes all records from dbo.RelatedProduct by calling the stored procedure.
        /// </summary>
        public async Task DeleteAllRelatedProductsAsync()
        {
            // Call the stored procedure (no parameters)
            await _dataProvider.ExecuteNonQueryAsync("EXEC dbo.DeleteAllRelatedProducts");
        }
    }

    #region CSV Data Objects

    public class ImportCustomer
    {
        public string CustomerId { get; set; }
        public string ExtraValueCardNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string PreferredStoreId { get; set; }
        public string ExtraValueCardRewardsPoints { get; set; }
    }

    public sealed class ImportCustomerMap : ClassMap<ImportCustomer>
    {
        public ImportCustomerMap()
        {
            Map(m => m.CustomerId).Name("CustomerId");
            Map(m => m.ExtraValueCardNumber).Name("ExtraValueCardNumber");
            Map(m => m.FirstName).Name("FirstName");
            Map(m => m.LastName).Name("LastName");
            Map(m => m.Address1).Name("Address1");
            Map(m => m.Address2).Name("Address2");
            Map(m => m.City).Name("City");
            Map(m => m.State).Name("State");
            Map(m => m.Zip).Name("Zip");
            Map(m => m.Phone).Name("Phone");
            Map(m => m.Email).Name("Email");
            Map(m => m.PreferredStoreId).Name("PreferredStoreId");
            Map(m => m.ExtraValueCardRewardsPoints).Name("ExtraValueCardRewardsPoints");
        }
    }

    public class ImportCpOrder
    {
        public string OrderId { get; set; }
        public string StoreId { get; set; }
        public string CustomerId { get; set; }
        public string PurchaseDate { get; set; }
        public string OrderTotal { get; set; }
    }

    public sealed class ImportCpOrderMap : ClassMap<ImportCpOrder>
    {
        public ImportCpOrderMap()
        {
            Map(m => m.OrderId).Name("OrderId");
            Map(m => m.StoreId).Name("StoreId");
            Map(m => m.CustomerId).Name("CustomerId");
            Map(m => m.PurchaseDate).Name("PurchaseDate");
            Map(m => m.OrderTotal).Name("OrderTotal");
        }
    }

    public class ImportCpOrderLine
    {
        public string OrderId { get; set; }
        public string LineNumber { get; set; }
        public string ProductId { get; set; }
        public string ProductDescription { get; set; }
        public string Quantity { get; set; }
    }

    public sealed class ImportCpOrderLineMap : ClassMap<ImportCpOrderLine>
    {
        public ImportCpOrderLineMap()
        {
            Map(m => m.OrderId).Name("OrderId");
            Map(m => m.LineNumber).Name("LineNumber");
            Map(m => m.ProductId).Name("ProductId");
            Map(m => m.ProductDescription).Name("ProductDescription");
            Map(m => m.Quantity).Name("Quantity");
        }
    }

    public class ImportDiscount
    {
        public string CustomerId { get; set; }
        public string LoyaltyCode { get; set; }
        public string LoyaltyName { get; set; }
        public string PurchaseGoal { get; set; }
        public string PurchaseStatus { get; set; }
    }

    public sealed class ImportDiscountMap : ClassMap<ImportDiscount>
    {
        public ImportDiscountMap()
        {
            Map(m => m.CustomerId).Name("CustomerId");
            Map(m => m.LoyaltyCode).Name("LoyaltyCode");
            Map(m => m.LoyaltyName).Name("LoyaltyName");
            Map(m => m.PurchaseGoal).Name("PurchaseGoal");
            Map(m => m.PurchaseStatus).Name("PurchaseStatus");
        }
    }

    public class ImportStoreLocation
    {
        public string StoreId { get; set; }
        public string StoreName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }
    }

    public sealed class ImportStoreLocationMap : ClassMap<ImportStoreLocation>
    {
        public ImportStoreLocationMap()
        {
            Map(m => m.StoreId).Name("StoreId");
            Map(m => m.StoreName).Name("StoreName");
            Map(m => m.Address1).Name("Address1");
            Map(m => m.Address2).Name("Address2");
            Map(m => m.City).Name("City");
            Map(m => m.State).Name("State");
            Map(m => m.Zip).Name("Zip");
            Map(m => m.Phone).Name("Phone");
        }
    }

    public class ImportProduct
    {
        public string Sku { get; set; }
        public string BaseSku { get; set; }
        public string Name { get; set; }
        public string UPC { get; set; }
        public string Description { get; set; }
        public string Ingredients { get; set; }
        public string Analysis { get; set; }
        public decimal Weight { get; set; }
        public string MainIngredient { get; set; }
        public string Brand { get; set; }
        public string Category { get; set; }
        public bool Puppy { get; set; }
        public bool Kitten { get; set; }
        public bool Adult { get; set; }
        public bool Senior { get; set; }
        public bool SmallBreed { get; set; }
        public bool LargeBreed { get; set; }
        public bool GrainFree { get; set; }
        public bool FBP { get; set; }
        public bool Pellets { get; set; }
        public bool Granule { get; set; }
        public bool Flakes { get; set; }
        public bool Wafers { get; set; }
        public bool Cubes { get; set; }
        public bool Dehydrated { get; set; }
        public bool Frozen { get; set; }
        public bool Goldfish { get; set; }
        public bool Betta { get; set; }
        public bool Parakeet { get; set; }
        public bool Cockatiel { get; set; }
        public bool CanaryFinch { get; set; }
        public bool Conure { get; set; }
        public bool ParrotHookbill { get; set; }
        public bool Amphibian { get; set; }
        public bool Crustacean { get; set; }
        public bool BeardedDragon { get; set; }
        public bool Gecko { get; set; }
        public bool Turtle { get; set; }
        public bool Tortoise { get; set; }
        public bool HermitCrab { get; set; }
        public bool Rabbit { get; set; }
        public bool Ferret { get; set; }
        public bool HamsterGerbil { get; set; }
        public bool Chinchilla { get; set; }
        public bool GuineaPig { get; set; }
        public bool RatMouse { get; set; }
        public bool Hedgehog { get; set; }
        public bool SugarGlider { get; set; }
        public bool Clumping { get; set; }
        public bool NonClumping { get; set; }
        public bool MultiCat { get; set; }
        public bool Lightweight { get; set; }
        public bool Snake { get; set; }
        public string ExpandsTo { get; set; }
        public string Includes { get; set; }
        public string Dimensions { get; set; }
        public bool Gallons10 { get; set; }
        public bool Gallons19 { get; set; }
        public bool Gallons39 { get; set; }
        public bool Gallons55 { get; set; }
        public bool Gallons75 { get; set; }
        public bool Gallons75Plus { get; set; }
        public bool ReplacementFiltersMediaAccessories { get; set; }
        public bool Balls { get; set; }
        public bool RopeToys { get; set; }
        public bool PlushToys { get; set; }
        public bool FlyingToys { get; set; }
        public bool TreatDispensingToys { get; set; }
        public bool BallsChasers { get; set; }
        public bool Catnip { get; set; }
        public bool HuntingStalkingToys { get; set; }
        public bool Teasers { get; set; }
        public bool Electronic { get; set; }
        public bool Ladders { get; set; }
        public bool Mirrors { get; set; }
        public bool PerchesSwings { get; set; }
        public bool ChewForage { get; set; }
        public bool Fluorescent { get; set; }
        public bool LED { get; set; }
        public bool StickBarTreats { get; set; }
        public bool Millet { get; set; }

        public bool ContainsFruit { get; set; }

        public bool Cuttlebone { get; set; }
        public bool Watts015 { get; set; }
        public bool Watts1525 { get; set; }
        public bool Watts2655 { get; set; }
        public bool Watts56100 { get; set; }
        public bool Watts101250 { get; set; }

        public bool Shampoos { get; set; }
        public bool ColognesSprays { get; set; }
        public bool BrushesCombsDeshedders { get; set; }
        public bool ClippersShearsAccessories { get; set; }
        public bool NailCare { get; set; }

        //public bool ShippingEnabled { get; set; }

        public bool DogBowl { get; set; }
        public bool ElevatedFeeder { get; set; }
        public bool DogWaterBottles { get; set; }
        public bool DualBowls { get; set; }
        public bool DogFoodStorage { get; set; }

        public bool DentalCare { get; set; }
        public bool EarCare { get; set; }
        public bool MedicatedSprayTopicals { get; set; }
        public bool MedicatedDropsChews { get; set; }
        public bool HomeCleanersDeodorizers { get; set; }
        public bool Calming { get; set; }
        public bool Digestive { get; set; }
        public bool CBD { get; set; }
        public bool HipJoint { get; set; }
        public bool Freshwater { get; set; }
        public bool Saltwater { get; set; }
        public bool Pond { get; set; }

        public bool HasHiddenPrice { get; set; }
        public bool IsPickupOnly { get; set; }

        public bool PillsAndChews { get; set; }
        public bool TopicalsSpraysShampoos { get; set; }
        public bool HomeAndOutdoorSprays { get; set; }
        public bool Collars { get; set; }

        public bool Decorations { get; set; }
        public bool Hides { get; set; }
        public bool Thermometers { get; set; }
        public bool ElectronicAccessory { get; set; }
        public bool CageAccessories { get; set; }

        public bool VacuumsSiphons { get; set; }
        public bool ScrapersBladesScrubbers { get; set; }
        public bool Magnets { get; set; }

        public bool LiquidSupplement { get; set; }
        public bool PowderedSupplement { get; set; }
        public bool Perch { get; set; }
        public bool NestingSacks { get; set; }
        public bool SeedDishCatcher { get; set; }

        public bool ShampoosConditionersSprays { get; set; }
        public bool NailTrimmers { get; set; }
        public bool Brushes { get; set; }
        public bool EarCleaner { get; set; }
        public bool DeodorizersStainRemovers { get; set; }

        public bool BowlsWaterBottles { get; set; }
        public bool BallsWheels { get; set; }
        public bool TubesTunnels { get; set; }

        public bool ChewsBonesBullySticks { get; set; }
        public bool EdibleChews { get; set; }
    }

    //   public sealed class ImportProductMap : ClassMap<ImportProduct>
    //   {
    //       public ImportProductMap()
    //       {
    //           Map(m => m.Sku).Name("Sku");
    //           Map(m => m.BaseSku).Name("BaseSku");
    //           Map(m => m.Name).Name("Name");
    //           Map(m => m.UPC).Name("UPC");
    //           Map(m => m.Description).Name("Description");
    //           Map(m => m.Ingredients).Name("Ingredients");
    //           Map(m => m.Analysis).Name("Analysis");
    //           Map(m => m.Weight).Name("Weight");
    //           Map(m => m.MainIngredient).Name("MainIngredient");
    //           Map(m => m.Brand).Name("Brand");
    //           Map(m => m.Category).Name("Category");
    //           Map(m => m.Puppy).Name("Puppy");
    //           Map(m => m.Kitten).Name("Kitten");
    //           Map(m => m.Adult).Name("Adult");
    //           Map(m => m.Senior).Name("Senior");
    //           Map(m => m.SmallBreed).Name("SmallBreed");
    //           Map(m => m.LargeBreed).Name("LargeBreed");
    //           Map(m => m.GrainFree).Name("GrainFree");
    //           Map(m => m.FBP).Name("FBP");
    //           Map(m => m.Pellets).Name("Pellets");
    //           Map(m => m.Granule).Name("Granule");
    //           Map(m => m.Flakes).Name("Flakes");
    //           Map(m => m.Wafers).Name("Wafers");
    //           Map(m => m.Cubes).Name("Cubes");
    //           Map(m => m.Dehydrated).Name("Dehydrated");
    //           Map(m => m.Frozen).Name("Frozen");
    //           Map(m => m.Goldfish).Name("Goldfish");
    //           Map(m => m.Betta).Name("Betta");
    //           Map(m => m.Parakeet).Name("Parakeet");
    //           Map(m => m.Cockatiel).Name("Cockatiel");
    //           Map(m => m.CanaryFinch).Name("CanaryFinch");
    //           Map(m => m.Conure).Name("Conure");
    //           Map(m => m.ParrotHookbill).Name("ParrotHookbill");
    //           Map(m => m.Amphibian).Name("Amphibian");
    //           Map(m => m.Crustacean).Name("Crustacean");
    //           Map(m => m.BeardedDragon).Name("BeardedDragon");
    //           Map(m => m.Gecko).Name("Gecko");
    //           Map(m => m.Turtle).Name("Turtle");
    //           Map(m => m.Tortoise).Name("Tortoise");
    //           Map(m => m.HermitCrab).Name("HermitCrab");
    //           Map(m => m.Rabbit).Name("Rabbit");
    //           Map(m => m.Ferret).Name("Ferret");
    //           Map(m => m.HamsterGerbil).Name("HamsterGerbil");
    //           Map(m => m.Chinchilla).Name("Chinchilla");
    //           Map(m => m.GuineaPig).Name("GuineaPig");
    //           Map(m => m.RatMouse).Name("RatMouse");
    //           Map(m => m.Hedgehog).Name("Hedgehog");
    //           Map(m => m.SugarGlider).Name("SugarGlider");
    //           Map(m => m.Clumping).Name("Clumping");
    //           Map(m => m.NonClumping).Name("NonClumping");
    //           Map(m => m.MultiCat).Name("MultiCat");
    //           Map(m => m.Lightweight).Name("Lightweight");
    //           Map(m => m.Snake).Name("Snake");
    //           Map(m => m.ExpandsTo).Name("ExpandsTo");
    //           Map(m => m.Includes).Name("Includes");
    //           Map(m => m.Dimensions).Name("Dimensions");
    //           Map(m => m.Gallons10).Name("Gallons10");
    //           Map(m => m.Gallons19).Name("Gallons19");
    //           Map(m => m.Gallons39).Name("Gallons39");
    //           Map(m => m.Gallons55).Name("Gallons55");
    //           Map(m => m.Gallons75).Name("Gallons75");
    //           Map(m => m.Gallons75Plus).Name("Gallons75Plus");
    //           Map(m => m.ReplacementFiltersMediaAccessories).Name("ReplacementFiltersMediaAccessories");
    //		Map(m => m.Balls).Name("Balls");
    //           Map(m => m.RopeToys).Name("Rope Toys");
    //           Map(m => m.PlushToys).Name("Plush Toys");
    //           Map(m => m.FlyingToys).Name("Flying Toys");
    //           Map(m => m.TreatDispensingToys).Name("Treat Dispensing Toys");
    //           Map(m => m.BallsChasers).Name("BallsChasers");
    //           Map(m => m.Catnip).Name("Catnip");
    //           Map(m => m.HuntingStalkingToys).Name("HuntingStalkingToys");
    //           Map(m => m.Teasers).Name("Teasers");
    //           Map(m => m.Electronic).Name("Electronic");
    //           Map(m => m.Ladders).Name("Ladders");
    //           Map(m => m.Mirrors).Name("Mirrors");
    //           Map(m => m.PerchesSwings).Name("Perches & Swings");
    //           Map(m => m.ChewForage).Name("Chew & Forage");
    //           Map(m => m.Fluorescent).Name("Fluorescent");
    //           Map(m => m.LED).Name("LED");
    //           Map(m => m.StickBarTreats).Name("Stick/Bar Treats");
    //           Map(m => m.Millet).Name("Millet");
    //           Map(m => m.ContainsFruit).Name("Contains Fruit");
    //           Map(m => m.Cuttlebone).Name("Cuttlebone");
    //           Map(m => m.Watts015).Name("Under 15 Watts");
    //           Map(m => m.Watts1525).Name("Watts 15-25");
    //           Map(m => m.Watts2655).Name("Watts 26-55");
    //           Map(m => m.Watts56100).Name("Watts 56-100");
    //           Map(m => m.Watts101250).Name("Watts 101-250");
    //           Map(m => m.Shampoos).Name("Shampoos");
    //           Map(m => m.ColognesSprays).Name("Colognes & Sprays");
    //           Map(m => m.BrushesCombsDeshedders).Name("Brushes, Combs, Deshedders");
    //           Map(m => m.ClippersShearsAccessories).Name("Clippsers, Shears, Accessories");
    //           Map(m => m.NailCare).Name("Nail Care");
    //           //Map(m => m.ShippingEnabled).Name("Shipping Enabled");
    //           Map(m => m.DogBowl).Name("Dog Bowl");
    //           Map(m => m.ElevatedFeeder).Name("Elevated Feeder");
    //           Map(m => m.DogWaterBottles).Name("Dog Water Bottles");
    //           Map(m => m.DualBowls).Name("Dual Bowls");
    //           Map(m => m.DogFoodStorage).Name("Dog Food Storage");
    //           Map(m => m.DentalCare).Name("Dental Care");
    //           Map(m => m.EarCare).Name("Ear Care");
    //           Map(m => m.MedicatedSprayTopicals).Name("Medicated Spray / Topicals");
    //           Map(m => m.MedicatedDropsChews).Name("Medicated Drops & Chews");
    //           Map(m => m.HomeCleanersDeodorizers).Name("Home Cleaners & Deodorizers");
    //           Map(m => m.Calming).Name("Calming");
    //           Map(m => m.Digestive).Name("Digestive");
    //           Map(m => m.CBD).Name("CBD");
    //           Map(m => m.HipJoint).Name("Hip & Joint");
    //           Map(m => m.Freshwater).Name("Freshwater");
    //           Map(m => m.Saltwater).Name("Saltwater");
    //           Map(m => m.Pond).Name("Pond");
    //           Map(m => m.HasHiddenPrice).Name("Has Hidden Price");
    //           Map(m => m.IsPickupOnly).Name("Is Pickup Only");
    //		Map(m => m.PillsAndChews).Name("Pills and Chews");
    //		Map(m => m.TopicalsSpraysShampoos).Name("Topicals, Sprays, & Shampoos");
    //		Map(m => m.HomeAndOutdoorSprays).Name("Home and Outdoor Sprays");
    //		Map(m => m.Collars).Name("Collars");
    //		Map(m => m.Decorations).Name("Decorations");
    //		Map(m => m.Hides).Name("Hides");
    //		Map(m => m.Thermometers).Name("Thermometers");
    //		Map(m => m.ElectronicAccessory).Name("Electronic Accessory");
    //		Map(m => m.CageAccessories).Name("Cage Accessories");
    //           Map(m => m.VacuumsSiphons).Name("Vacuums & Siphons");
    //           Map(m => m.ScrapersBladesScrubbers).Name("Scrapers, Blades, & Scrubbers");
    //           Map(m => m.Magnets).Name("Magnets");
    //           Map(m => m.LiquidSupplement).Name("Liquid Supplement");
    //           Map(m => m.PowderedSupplement).Name("Powdered Supplement");
    //           Map(m => m.Perch).Name("Perch");
    //           Map(m => m.NestingSacks).Name("Nesting / Sacks");
    //           Map(m => m.SeedDishCatcher).Name("Seed Dish / Catcher");
    //           Map(m => m.ShampoosConditionersSprays).Name("Shampoos, Conditioners, & Sprays");
    //           Map(m => m.NailTrimmers).Name("Nail Trimmers");
    //           Map(m => m.Brushes).Name("Brushes");
    //           Map(m => m.EarCleaner).Name("Ear Cleaner");
    //           Map(m => m.DeodorizersStainRemovers).Name("Deodorizers & Stain Removers");
    //           Map(m => m.BowlsWaterBottles).Name("Bowls & Water Bottles");
    //           Map(m => m.BallsWheels).Name("Balls & Wheels");
    //           Map(m => m.TubesTunnels).Name("Tubes & Tunnels");
    //       }
    //}

    public class ImportItemPrice
    {
        public string Sku { get; set; }
        public string RegPrice { get; set; }
        public string DiscPrice { get; set; }
    }

    public sealed class ImportItemPriceMap : ClassMap<ImportItemPrice>
    {
        public ImportItemPriceMap()
        {
            Map(m => m.Sku).Name("Sku");
            Map(m => m.RegPrice).Name("RegPrice");
            Map(m => m.DiscPrice).Name("DiscPrice");
        }
    }

    public class ImportInventory
    {
        public string Sku { get; set; }
        public string StoreId { get; set; }
        public decimal MinQty { get; set; }
        public decimal MaxQty { get; set; }
        public decimal OnHand { get; set; }
    }

    public sealed class ImportInventoryMap : ClassMap<ImportInventory>
    {
        public ImportInventoryMap()
        {
            Map(m => m.Sku).Name("Sku");
            Map(m => m.StoreId).Name("StoreId");
            Map(m => m.MinQty).Name("MinQty");
            Map(m => m.MaxQty).Name("MaxQty");
            Map(m => m.OnHand).Name("OnHand");
        }
    }

    public class ImportRewardsCertificate
    {
        public string ExtraValueCardNumber { get; set; }
        public string RewardCode { get; set; }
        public double AmountRemaining { get; set; }
        public DateTime ExpiresOn { get; set; }
        public string CustomerEmail { get; set; }
    }

    public sealed class ImportRewardsCertificateMap : ClassMap<ImportRewardsCertificate>
    {
        public ImportRewardsCertificateMap()
        {
            Map(m => m.RewardCode).Name("RewardCode");
            Map(m => m.ExtraValueCardNumber).Name("ExtraValueCardNumber");
            Map(m => m.AmountRemaining).Name("AmountRemaining");
            Map(m => m.ExpiresOn).Name("ExpiresOn");
        }
    }

    public class ImportGiftCard
    {
        public string GiftCardCode { get; set; }
        public double AmountRemaining { get; set; }
    }

    public sealed class ImportGiftCardMap : ClassMap<ImportGiftCard>
    {
        public ImportGiftCardMap()
        {
            Map(m => m.GiftCardCode).Name("GiftCardCode");
            Map(m => m.AmountRemaining).Name("AmountRemaining");
        }
    }

    public class ImportRelatedProduct
    {
        public string Category { get; set; }
        public string Skus { get; set; }
    }

    #endregion

    #region Json Data Objects

    public class ImportPromoDiscount
    {
        public string Title { get; set; }
        public string DiscType { get; set; }
        public double DiscValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<string> DiscountedSkus { get; set; }
        public int DiscountedQty { get; set; }
        public List<string> RequiredSkus { get; set; }
        public int RequiredQty { get; set; }
    }

    public class ImportFrequentBuyerProgram
    {
        public string LoyaltyCode { get; set; }
        public List<string> ProductSkus { get; set; }
    }

    public class ImportMultilineKit
    {
        public string KitCode { get; set; }
        public string KitDescription { get; set; }
        public double KitPrice { get; set; }
        public string ProductSku { get; set; }
        public int ProductQty { get; set; }
    }

    public class ProductKitsPluginSettings : ISettings
    {
        public int CategoryId { get; set; }
    }

    #endregion
}