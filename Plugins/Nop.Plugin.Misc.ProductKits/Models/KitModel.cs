using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.ProductKits.Models
{
    public record KitModel : BaseNopEntityModel
    {
        public KitModel() {
            KitProducts = new List<KitProduct>();
            AddProductToKit = new KitProduct();
        }

        public int ProductId { get; set; } // product id for the kit
        public string ProductName { get; set; } // product name for the kit
        public string ProductSku { get; set; } // product sku for the kit
        public List<KitProduct> KitProducts { get; set; } // collection of products in the kit
        public KitProduct AddProductToKit { get; set; } // for adding new product to kit
        [NopResourceDisplayName("Admin.Catalog.Products.Fields.Price")]
        public decimal Price { get; set; } // Price of the Kit


        public string PrimaryStoreCurrencyCode { get; set; }
    }

    public class KitProduct
    {
        public KitProduct() { }
        public int ProductId { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Products.Fields.Name")]
        public string ProductName { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Products.Fields.Sku")]
        public string ProductSku { get; set; }
        public int ProductAttributeId { get; set; }
        [NopResourceDisplayName("Plugins.Misc.ProductKits.Edit.Attribute")]
        public string ProductAttributeName { get; set; }

    }

    public record KitProductModel : BaseNopEntityModel
    {
        public KitProductModel() { }

        [Required]
        [NopResourceDisplayName("Admin.Catalog.Products.Fields.Id")]
        public int KitProductId { get; set; }

        [Required]
        [NopResourceDisplayName("Admin.Catalog.Products.Fields.Sku")]
        public string ProductSku { get; set; }

        [Required]
        [NopResourceDisplayName("Plugins.Misc.ProductKits.Edit.Attribute")]
        public string ProductAttributeName { get; set; }

        [Required]
        [NopResourceDisplayName("Admin.Catalog.Products.Fields.Price")]
        public decimal Price { get; set; }
    }

    public record KitPriceModel : BaseNopEntityModel
    {
        public KitPriceModel() { }

        [Required]
        [NopResourceDisplayName("Admin.Catalog.Products.Fields.Id")]
        public int KitProductId { get; set; }

        [Required]
        [NopResourceDisplayName("Admin.Catalog.Products.Fields.Price")]
        public decimal Price { get; set; }
    }

    public record KitDeleteModel : BaseNopEntityModel
    {
        public KitDeleteModel() { }

        [Required]
        public int KitProductId { get; set; }

        [Required]
        public ICollection<int> SelectedIds { get; set; }

        [Required]
        [NopResourceDisplayName("Admin.Catalog.Products.Fields.Price")]
        public decimal Price { get; set; }
    }
}
