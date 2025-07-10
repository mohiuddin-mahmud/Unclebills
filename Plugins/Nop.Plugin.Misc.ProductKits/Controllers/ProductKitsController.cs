using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Plugin.Misc.ProductKits.Models;
using Nop.Services.Messages;
using Org.BouncyCastle.Asn1.Cms;
using System.Xml.Linq;
using DocumentFormat.OpenXml.EMMA;
using Nop.Core.Domain.Directory;
using Nop.Services.Directory;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;

namespace Nop.Plugin.Misc.ProductKits.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class ProductKitsController : BaseAdminController
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly IPictureService _pictureService;
        private readonly IPermissionService _permissionService;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IProductAttributeService _productAttributeService;
        private readonly ISettingService _settingService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductAttributeParser _productAttributeParser;
        protected readonly INotificationService _notificationService;
        protected readonly ICurrencyService _currencyService;
        protected readonly CurrencySettings _currencySettings;
        #endregion

        #region Ctor

        public ProductKitsController(
            IProductService productService,
            ICategoryService categoryService,
            IWorkContext workContext,
            ILocalizationService localizationService,
            IPictureService pictureService,
            IPermissionService permissionService,
            IStaticCacheManager cacheManager,
            IProductAttributeService productAttributeService,
            ISettingService settingService,
            IProductModelFactory productModelFactory,
            IProductAttributeParser productAttributeParser,
            INotificationService notificationService,
            ICurrencyService currencyService,
            CurrencySettings currencySettings)
        {
            _productService = productService;
            _categoryService = categoryService;
            _workContext = workContext;
            _localizationService = localizationService;
            _pictureService = pictureService;
            _permissionService = permissionService;
            _cacheManager = cacheManager;
            _productAttributeService = productAttributeService;
            _settingService = settingService;
            _productModelFactory = productModelFactory;
            _productAttributeParser = productAttributeParser;
            _notificationService = notificationService;
            _currencyService = currencyService;
            _currencySettings = currencySettings;
        }

        #endregion

        #region Utilities

        protected virtual async Task<KitModel> GetKit(int kitId)
        {
            var kit = new KitModel();
            var product = await _productService.GetProductByIdAsync(kitId);
            kit.ProductId = product.Id;
            kit.ProductName = product.Name;
            kit.ProductSku = product.Sku;           

            var model = new KitModel
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ProductSku = product.Sku,
                PrimaryStoreCurrencyCode = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId)).CurrencyCode
            };

            // Get products in the kit
            var attributeMappings = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
            foreach (var mapping in attributeMappings)
            {
                var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(mapping.Id);
                foreach (var value in attributeValues.Where(v => v.AttributeValueType == AttributeValueType.AssociatedToProduct))
                {
                    var associatedProduct = await _productService.GetProductByIdAsync(value.AssociatedProductId);
                    if (associatedProduct == null)
                        continue;

                    var currentLanguage = await _workContext.GetWorkingLanguageAsync();
                    var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(mapping.ProductAttributeId);
                    var attributeName = await _localizationService.GetLocalizedAsync(productAttribute, a => a.Name, currentLanguage.Id);

                    model.KitProducts.Add(new KitProduct
                    {
                        ProductId = associatedProduct.Id,
                        ProductName = associatedProduct.Name,
                        ProductSku = associatedProduct.Sku,
                        ProductAttributeName = attributeName,
                        ProductAttributeId = mapping.ProductAttributeId
                    });
                }
            }

            // get the current kit price
            kit.Price = 0; // default value
            var combinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(product.Id);
            var combination = combinations.FirstOrDefault();
            model.Price = combination?.OverriddenPrice ?? 0;

            return model;
        }

        private async Task UpdateProductAttributeCombinationsAsync(Product kit, decimal price)
        {
            var results = new List<string>();

            // Remove existing combinations
            var prodAttrCombos = await _productAttributeService.GetAllProductAttributeCombinationsAsync(kit.Id);
            foreach (var pac in prodAttrCombos)
            {
                await _productAttributeService.DeleteProductAttributeCombinationAsync(pac);
            }

            // Get all attribute mappings with values
            var prodAttrMaps = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(kit.Id);
            var attrXml = new XElement("Attributes");

            foreach (var pam in prodAttrMaps)
            {
                // Load values for each mapping
                var values = await _productAttributeService.GetProductAttributeValuesAsync(pam.Id);

                // Check for associated product values
                if (values.Any(v => v.AttributeValueType == AttributeValueType.AssociatedToProduct))
                {
                    // Use first value (matching original behavior)
                    var firstValue = values.First();
                    attrXml.Add(new XElement("ProductAttribute",
                        new XAttribute("ID", pam.Id),
                        new XElement("ProductAttributeValue",
                            new XElement("Value", firstValue.Id)
                        )
                    ));
                }
            }

            var attrXmlString = attrXml.ToString();
            results.Add(attrXmlString);

            // Create new combination
            var prodAttrCombo = new ProductAttributeCombination
            {
                ProductId = kit.Id,
                StockQuantity = 10000,
                OverriddenPrice = price,
                AllowOutOfStockOrders = false,
                NotifyAdminForQuantityBelow = 1,
                AttributesXml = attrXmlString
            };
            await _productAttributeService.InsertProductAttributeCombinationAsync(prodAttrCombo);


            //// Remove existing combinations
            //var combinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(kit.Id);
            //foreach (var combination in combinations)
            //    await _productAttributeService.DeleteProductAttributeCombinationAsync(combination);

            //// Get attribute mappings for kit products
            //var attributeMappings = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(kit.Id);
            //var validMappings = new List<ProductAttributeMapping>();

            //foreach (var mapping in attributeMappings)
            //{
            //    var values = await _productAttributeService.GetProductAttributeValuesAsync(mapping.Id);
            //    if (values.Any(v => v.AttributeValueType == AttributeValueType.AssociatedToProduct))
            //    {
            //        validMappings.Add(mapping);
            //    }
            //}

            //if (!validMappings.Any())
            //    return;

            //// Generate attributes XML
            //var attributesXml = await _productAttributeParser.GenerateAllCombinationsAsync(kit);
            //if (string.IsNullOrEmpty(attributesXml.ToString()))
            //    return;

            //// Create new combination
            //var newCombination = new ProductAttributeCombination
            //{
            //    ProductId = kit.Id,
            //    StockQuantity = 10000,
            //    AllowOutOfStockOrders = false,
            //    NotifyAdminForQuantityBelow = 1,
            //    OverriddenPrice = price,
            //    AttributesXml = attributesXml.ToString()
            //};

            //await _productAttributeService.InsertProductAttributeCombinationAsync(newCombination);
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            var model = new ConfigurationModel();
            var settings = await _settingService.LoadSettingAsync<ProductKitsPluginSettings>();

            // Prepare categories
            model.AvailableCategories.Add(new SelectListItem { Text = "None Selected", Value = "0" });
            var categories = await _categoryService.GetAllCategoriesAsync(showHidden: true);
            foreach (var c in categories)
                model.AvailableCategories.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString() });

            model.CategoryId = settings?.CategoryId ?? 0;

            return View("~/Plugins/Misc.ProductKits/Views/ProductKits/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            var settings = await _settingService.LoadSettingAsync<ProductKitsPluginSettings>();
            settings.CategoryId = model.CategoryId;
            await _settingService.SaveSettingAsync(settings);
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));
            return await Configure();
        }

        [CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
        public async Task<IActionResult> Index()
        {
            await Task.Yield();
            return RedirectToAction("List");

            //a vendor should have access only to his products
            //var currentVendor = await _workContext.GetCurrentVendorAsync();
            ////vendor can edit "Reply text" only
            //var isLoggedInAsVendor = currentVendor != null;

            //var model = await _productModelFactory.PrepareProductListModelAsync(new ProductSearchModel());
            //model.IsLoggedInAsVendor = await _workContext.GetCurrentVendorAsync() != null;

            //return View("~/Plugins/Misc.ProductKits/Views/ProductKits/Index.cshtml", model);
        }

        [CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
        public virtual async Task<IActionResult> List()
        {
            //prepare model
            var model = await _productModelFactory.PrepareProductSearchModelAsync(new ProductSearchModel());

            return View("~/Plugins/Misc.ProductKits/Views/ProductKits/Index.cshtml", model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
        public virtual async Task<IActionResult> ProductList(ProductSearchModel searchModel)
        {
            // Filter by kit category
            var settings = await _settingService.LoadSettingAsync<ProductKitsPluginSettings>();
            searchModel.SearchCategoryId = settings.CategoryId;
            //prepare model
            var model = await _productModelFactory.PrepareProductListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        public async Task<IActionResult> GoToSku(ProductSearchModel searchModel)
        {
            var product = await _productService.GetProductBySkuAsync(searchModel.GoDirectlyToSku);

            // get category for kits
            var pluginSettings = _settingService.LoadSetting<ProductKitsPluginSettings>();

            if (product == null)
            {
                var combination = await _productAttributeService.GetProductAttributeCombinationBySkuAsync(searchModel.GoDirectlyToSku);
                if (combination != null)
                    product = await _productService.GetProductByIdAsync(combination.ProductId);
            }


            var categories = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id);

            if (product != null && categories.Any(x => x.CategoryId == pluginSettings.CategoryId))
            {
                var kitModel = await GetKit(product.Id);
                return View("~/Plugins/Misc.ProductKits/Views/ProductKits/Edit.cshtml", kitModel);


                //var settings = await _settingService.LoadSettingAsync<ProductKitsPluginSettings>();
                //var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id);
                //if (productCategories.Any(pc => pc.CategoryId == settings.CategoryId))
                //{
                //    return RedirectToAction("Edit", new { id = product.Id });
                //}
            }

            return RedirectToAction("Index");
        }



        [CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
        public async Task<IActionResult> Edit(int id)
        {
            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null || product.Deleted)
                return RedirectToAction("Index");

            var model = await GetKit(product.Id);
            return View("~/Plugins/Misc.ProductKits/Views/ProductKits/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        //public virtual async Task<IActionResult> Edit(KitModel model, bool continueEditing)
        //{
        //    //try to get a product with the specified id
        //    var product = await _productService.GetProductByIdAsync(model.Id);
        //    if (product == null || product.Deleted)
        //        return RedirectToAction("List");
                      

        //    product.Price = model.Price;
        //    await _productService.UpdateProductAsync(product);

        //    //prepare model
        //    model = await GetKit(model.ProductId);

        //    //if we got this far, something failed, redisplay form
        //    return View("~/Plugins/Misc.ProductKits/Views/ProductKits/Edit.cshtml", model);
        //}

        [HttpPost]
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
        public async Task<IActionResult> KitList(int productId)
        {         

            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return NotFound();

            var model = await GetKit(product.Id);
            return Json(new { Data = model.KitProducts, Total = model.KitProducts.Count });
        }



        [HttpPost]
        public async Task<ActionResult> AddProduct(KitProductModel model)
        {
            var results = new List<string>();
            if (!string.IsNullOrEmpty(model.ProductAttributeName) && !string.IsNullOrEmpty(model.ProductSku))
            {
                var product = await _productService.GetProductBySkuAsync(model.ProductSku);
                if (product != null)
                {
                    var kit = await _productService.GetProductByIdAsync(model.KitProductId);
                    if (kit != null)
                    {
                        // Product Attribute
                        var prodAttr = (await _productAttributeService.GetAllProductAttributesAsync())
                            .FirstOrDefault(a => a.Name == model.ProductAttributeName);

                        if (prodAttr == null)
                        {
                            prodAttr = new ProductAttribute { Name = model.ProductAttributeName };
                            await _productAttributeService.InsertProductAttributeAsync(prodAttr);
                        }

                        // Product Attribute Mapping
                        var prodAttrMap = new ProductAttributeMapping
                        {
                            ProductId = kit.Id,
                            ProductAttributeId = prodAttr.Id,
                            IsRequired = true,
                            AttributeControlTypeId = (int)AttributeControlType.ReadonlyCheckboxes,
                            TextPrompt = prodAttr.Name,
                            DisplayOrder = 0
                        };
                        await _productAttributeService.InsertProductAttributeMappingAsync(prodAttrMap);

                        // Product Attribute Value
                        var prodAttrVal = new ProductAttributeValue
                        {
                            ProductAttributeMappingId = prodAttrMap.Id,
                            Name = product.Name,
                            AttributeValueType = AttributeValueType.AssociatedToProduct,
                            AssociatedProductId = product.Id,
                            Quantity = 1,
                            IsPreSelected = true
                        };
                        await _productAttributeService.InsertProductAttributeValueAsync(prodAttrVal);

                        // Remove existing combinations
                        var prodAttrCombos = await _productAttributeService.GetAllProductAttributeCombinationsAsync(kit.Id);
                        foreach (var pac in prodAttrCombos)
                        {
                            await _productAttributeService.DeleteProductAttributeCombinationAsync(pac);
                        }

                        // Get all attribute mappings with values
                        var prodAttrMaps = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(kit.Id);
                        var attrXml = new XElement("Attributes");

                        foreach (var pam in prodAttrMaps)
                        {
                            // Load values for each mapping
                            var values = await _productAttributeService.GetProductAttributeValuesAsync(pam.Id);

                            // Check for associated product values
                            if (values.Any(v => v.AttributeValueType == AttributeValueType.AssociatedToProduct))
                            {
                                // Use first value (matching original behavior)
                                var firstValue = values.First();
                                attrXml.Add(new XElement("ProductAttribute",
                                    new XAttribute("ID", pam.Id),
                                    new XElement("ProductAttributeValue",
                                        new XElement("Value", firstValue.Id)
                                    )
                                ));
                            }
                        }

                        var attrXmlString = attrXml.ToString();
                        results.Add(attrXmlString);

                        // Create new combination
                        var prodAttrCombo = new ProductAttributeCombination
                        {
                            ProductId = kit.Id,
                            StockQuantity = 10000,
                            OverriddenPrice = model.Price,
                            AllowOutOfStockOrders = false,
                            NotifyAdminForQuantityBelow = 1,
                            AttributesXml = attrXmlString
                        };
                        await _productAttributeService.InsertProductAttributeCombinationAsync(prodAttrCombo);

                        results.Add("Success!");
                    }
                }
            }

            if (results.Count == 0)
                results.Add("Fail!");
            return Json(results);
        }
        //[HttpPost]
        //public async Task<IActionResult> AddProduct(KitProductModel model)
        //{
        //    var results = new List<string>();

        //    if (!string.IsNullOrEmpty(model.ProductAttributeName) &&
        //        !string.IsNullOrEmpty(model.ProductSku) &&
        //        model.KitProductId > 0)
        //    {
        //        var kit = await _productService.GetProductByIdAsync(model.KitProductId);
        //        var product = await _productService.GetProductBySkuAsync(model.ProductSku);

        //        if (kit != null && product != null)
        //        {
        //            // Get or create product attribute
        //            var productAttributes = await _productAttributeService.GetAllProductAttributesAsync();
        //            var productAttribute = productAttributes
        //                .FirstOrDefault(pa => pa.Name.Equals(model.ProductAttributeName, StringComparison.InvariantCultureIgnoreCase));

        //            if (productAttribute == null)
        //            {
        //                productAttribute = new ProductAttribute { Name = model.ProductAttributeName };
        //                await _productAttributeService.InsertProductAttributeAsync(productAttribute);
        //            }

        //            // Create attribute mapping
        //            var attributeMapping = new ProductAttributeMapping
        //            {
        //                ProductId = kit.Id,
        //                ProductAttributeId = productAttribute.Id,
        //                AttributeControlType = AttributeControlType.ReadonlyCheckboxes,
        //                IsRequired = true,
        //                TextPrompt = productAttribute.Name,
        //                DisplayOrder = 0
        //            };
        //            await _productAttributeService.InsertProductAttributeMappingAsync(attributeMapping);

        //            // Create attribute value
        //            var attributeValue = new ProductAttributeValue
        //            {
        //                ProductAttributeMappingId = attributeMapping.Id,
        //                Name = product.Name,
        //                AttributeValueType = AttributeValueType.AssociatedToProduct,
        //                AssociatedProductId = product.Id,
        //                Quantity = 1,
        //                IsPreSelected = true
        //            };
        //            await _productAttributeService.InsertProductAttributeValueAsync(attributeValue);

        //            // Update combinations
        //            await UpdateProductAttributeCombinationsAsync(kit, model.Price);
        //            results.Add("Success");
        //        }
        //    }

        //    if (results.Count == 0)
        //        results.Add("Fail");
        //    return Json(results);
        //}

        [HttpPost]
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        public async Task<IActionResult> DeleteSelected(KitDeleteModel model)
        {
            var results = new List<string>();

            if (model.SelectedIds != null && model.KitProductId > 0)
            {
                var kit = await _productService.GetProductByIdAsync(model.KitProductId);
                if (kit != null)
                {
                    foreach (var id in model.SelectedIds)
                    {
                        var mappings = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(kit.Id);
                        foreach (var mapping in mappings)
                        {
                            var values = await _productAttributeService.GetProductAttributeValuesAsync(mapping.Id);
                            if (values.Any(v =>
                                v.AttributeValueType == AttributeValueType.AssociatedToProduct &&
                                v.AssociatedProductId == id))
                            {
                                await _productAttributeService.DeleteProductAttributeMappingAsync(mapping);
                                break;
                            }
                        }
                    }

                    await UpdateProductAttributeCombinationsAsync(kit, model.Price);
                    results.Add("Success");
                }
            }

            if (results.Count == 0)
                results.Add("Fail");
            return Json(results);
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePrice(KitPriceModel model)
        {
            List<string> results = new List<string>();
            if (model.KitProductId > 0 && model.Price > decimal.Zero)
            {
                var prodAttrComboList = await _productAttributeService.GetAllProductAttributeCombinationsAsync(model.KitProductId);
                var prodAttrCombo = prodAttrComboList.FirstOrDefault();
                if (prodAttrCombo != null)
                {
                    prodAttrCombo.OverriddenPrice = model.Price;
                    await _productAttributeService.UpdateProductAttributeCombinationAsync(prodAttrCombo);
                    results.Add("Success!");
                }
            }

            // if not success, then it must have failed somewhere
            if (results.Count == 0)
                results.Add("Fail!");

            return Json(results);
           
        }

        public async Task<IActionResult> ProductAttributeAutoComplete(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 3)
                return Json(Array.Empty<object>());

            var attributes = await _productAttributeService.GetAllProductAttributesAsync();
            var result = attributes
                .Where(pa => pa.Name.Contains(term, StringComparison.InvariantCultureIgnoreCase))
                .Select(pa => new { label = pa.Name })
                .ToList();

            return Json(result);
        }

        #endregion
    }
}