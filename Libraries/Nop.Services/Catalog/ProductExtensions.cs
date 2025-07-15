using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Core.Infrastructure;
using Nop.Data;

namespace Nop.Services.Catalog;

public static class ProductExtensions
{
    /// <summary>
    /// Sorts the elements of a sequence in order according to a product sorting rule
    /// </summary>
    /// <param name="productsQuery">A sequence of products to order</param>
    /// <param name="currentLanguage">Current language</param>
    /// <param name="orderBy">Product sorting rule</param>
    /// <param name="localizedPropertyRepository">Localized property repository</param>
    /// <returns>An System.Linq.IOrderedQueryable`1 whose elements are sorted according to a rule.</returns>
    /// <remarks>
    /// If <paramref name="orderBy"/> is set to <c>Position</c> and passed <paramref name="productsQuery"/> is
    /// ordered sorting rule will be skipped
    /// </remarks>
    public static IQueryable<Product> OrderBy(this IQueryable<Product> productsQuery, IRepository<LocalizedProperty> localizedPropertyRepository, Language currentLanguage, ProductSortingEnum orderBy)
    {
        if (orderBy == ProductSortingEnum.NameAsc || orderBy == ProductSortingEnum.NameDesc)
        {
            var currentLanguageId = currentLanguage.Id;

            var query =
                from product in productsQuery
                join localizedProperty in localizedPropertyRepository.Table on new
                    {
                        product.Id,
                        languageId = currentLanguageId,
                        keyGroup = nameof(Product),
                        key = nameof(Product.Name)
                    }
                    equals new
                    {
                        Id = localizedProperty.EntityId,
                        languageId = localizedProperty.LanguageId,
                        keyGroup = localizedProperty.LocaleKeyGroup,
                        key = localizedProperty.LocaleKey
                    } into localizedProperties
                from localizedProperty in localizedProperties.DefaultIfEmpty(new LocalizedProperty { LocaleValue = product.Name })
                select new 
                {
                    sortName = localizedProperty == null ? product.Name : localizedProperty.LocaleValue,
                    product
                };

            if (orderBy == ProductSortingEnum.NameAsc)
                productsQuery = from item in query
                    orderby item.sortName
                    select item.product;
            else
                productsQuery = from item in query
                    orderby item.sortName descending
                    select item.product;

            return productsQuery;
        }

        return orderBy switch
        {
            ProductSortingEnum.PriceAsc => productsQuery.OrderBy(p => p.Price),
            ProductSortingEnum.PriceDesc => productsQuery.OrderByDescending(p => p.Price),
            ProductSortingEnum.CreatedOn => productsQuery.OrderByDescending(p => p.CreatedOnUtc),
            ProductSortingEnum.Position when productsQuery is IOrderedQueryable => productsQuery,
            _ => productsQuery.OrderBy(p => p.DisplayOrder).ThenBy(p => p.Id)
        };
    }

    public static int GetTotalStockQuantity(this Product product,
           bool useReservedQuantity = true, int warehouseId = 0)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        // Check inventory management method
        if (product.ManageInventoryMethod != ManageInventoryMethod.ManageStock)
            return 0;

        // Handle multiple warehouses
        if (product.UseMultipleWarehouses)
        {
            var repo = EngineContext.Current.Resolve<IRepository<ProductWarehouseInventory>>();
            var query = repo.Table.Where(pwi => pwi.ProductId == product.Id);

            // Apply warehouse filter if specified
            if (warehouseId > 0)
            {
                query = query.Where(pwi => pwi.WarehouseId == warehouseId);
            }

            var warehouseInventory = query.ToList();

            if (!warehouseInventory.Any())
                return 0;

            var totalStock = warehouseInventory.Sum(x => x.StockQuantity);

            if (useReservedQuantity)
                totalStock -= warehouseInventory.Sum(x => x.ReservedQuantity);

            return totalStock;
        }

        // Single warehouse - use the main stock quantity
        return product.StockQuantity;
    }
    //public static int GetTotalStockQuantity(this Product product,
    //       bool useReservedQuantity = true, int warehouseId = 0)
    //{
    //    if (product == null)
    //        throw new ArgumentNullException("product");

    //    if (product.ManageInventoryMethod != ManageInventoryMethod.ManageStock)
    //    {
    //        //We can calculate total stock quantity when 'Manage inventory' property is set to 'Track inventory'
    //        return 0;
    //    }

    //    if (product.UseMultipleWarehouses)
    //    {
    //        var pwi = product.ProductWarehouseInventory;
    //        if (warehouseId > 0)
    //        {
    //            pwi = pwi.Where(x => x.WarehouseId == warehouseId).ToList();
    //        }
    //        var result = pwi.Sum(x => x.StockQuantity);
    //        if (useReservedQuantity)
    //        {
    //            result = result - pwi.Sum(x => x.ReservedQuantity);
    //        }
    //        return result;
    //    }

    //    return product.StockQuantity;
    //}
}