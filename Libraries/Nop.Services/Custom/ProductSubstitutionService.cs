using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Orders;

namespace Nop.Services.Custom;

public class ProductSubstitutionService : IProductSubstitutionService
{
    private readonly IProductService _productService;
    private readonly ISpecificationAttributeService _specificationAttributeService;
    private readonly IShoppingCartService _shoppingCartService;

    public ProductSubstitutionService(IProductService productService,
        ISpecificationAttributeService specificationAttributeService,
        IShoppingCartService shoppingCartService)
    {
        this._productService = productService;
        this._specificationAttributeService = specificationAttributeService;
        this._shoppingCartService = shoppingCartService;
    }

    public bool ProductOutOfStock(ShoppingCartItem sci)
    {
        try
        {
            // if requested cart quantity is less than on hand stock at shipping warehouse then in stock
            // TODO - error here - max griffin
            if (sci.WarehouseId > 0 && ProductStockLevel(sci.Product, sci.WarehouseId) >= sci.Quantity)
                return false;
        }
        catch { }
        return true;
    }

    private int ProductStockLevel(Product p, int warehouseId)
    {
        try
        {
            var productWarehouseInventory = _productService.GetAllProductWarehouseInventoryRecordsAsync(p.Id).Result;
            return productWarehouseInventory.FirstOrDefault(pwi => pwi.WarehouseId == warehouseId).StockQuantity;
        }
        catch
        {
            return 0;
        }
    }

    public IList<Product> GetSubstituteProducts(ShoppingCartItem sci)
    {
        var subProducts = new List<Product>();

        if (sci.WarehouseId <= 0)
            return subProducts;

        // Get all specification attributes for the product
        var productSpecAttrs = _specificationAttributeService
            .GetProductSpecificationAttributesAsync(productId: sci.ProductId).Result;

        // Find Base SKU attribute using direct service calls
        string baseSkuValue = null;
        foreach (var psa in productSpecAttrs)
        {
            // Get the specification attribute option
            var option = _specificationAttributeService
                .GetSpecificationAttributeOptionByIdAsync(psa.SpecificationAttributeOptionId).Result;

            if (option == null)
                continue;

            // Get the specification attribute
            var attribute = _specificationAttributeService
                .GetSpecificationAttributeByIdAsync(option.SpecificationAttributeId).Result;

            if (attribute != null && attribute.Name == "Base SKU")
            {
                baseSkuValue = psa.CustomValue;
                break; // Found the Base SKU, exit loop
            }
        }

        if (string.IsNullOrWhiteSpace(baseSkuValue))
            return subProducts;

        // Get all product sizes asynchronously
        var allProductSizes =  _productService.GetAllProductSizes(baseSkuValue);

        foreach (var product in allProductSizes)
        {
            if (product.Id != sci.ProductId)
            {
                if (product.Id != sci.ProductId &&
                            ProductStockLevel(product, sci.WarehouseId) >= sci.Quantity)
                {
                    subProducts.Add(product);
                }
            }
        }

        return subProducts;
    }

    public void SubstituteCartItems(List<ShoppingCartItem> items, string substitutionAsString)
    {
        var firstItem = items.FirstOrDefault();
        var customer = firstItem.Customer;
        var cartType = firstItem.ShoppingCartType;
        var substitutions = substitutionAsString.Split(',');
        Dictionary<int, int> quantities = new Dictionary<int, int>();
        foreach (var substitution in substitutions)
        {
            var thisSubstitution = substitution.Split('|');
            var shoppingCartItem = items.Where(p => p.Id == int.Parse(thisSubstitution[1])).FirstOrDefault();
            if (shoppingCartItem != null)
            {
                try
                {
                    if (!quantities.ContainsKey(shoppingCartItem.Id))
                    {
                        quantities.Add(shoppingCartItem.Id, shoppingCartItem.Quantity);
                    }
                    _shoppingCartService.DeleteShoppingCartItemAsync(shoppingCartItem, ensureOnlyActiveCheckoutAttributes: true);
                }

                catch { }

                int qty = 1;
                if (quantities.ContainsKey(shoppingCartItem.Id))
                {
                    qty = quantities[shoppingCartItem.Id];
                }
                _shoppingCartService.AddToCartAsync(customer, _productService.GetProductByIdAsync(int.Parse(thisSubstitution[0])).Result,
                    cartType, items.FirstOrDefault().StoreId, quantity: qty);

                //should add warehouseid?
            }

        }
    }

   
}