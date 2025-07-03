using CsvHelper;
using CsvHelper.Configuration;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Shipping;
using Nop.Services.Catalog;
using Nop.Services.Shipping;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Custom
{
    public class WarehouseInventoryImportService : IWarehouseInventoryImportService
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IShippingService _shippingService;

        #endregion

        #region Ctor

        public WarehouseInventoryImportService(IProductService productService,
            IShippingService shippingService)
        {
            this._productService = productService;
            this._shippingService = shippingService;
        }

        #endregion

        #region Methods

        public string Import(string csvFileFullLocalPath)
        {
            try
            {
                // get list of all warehouses
                IList<Warehouse> allWarehouses = this._shippingService.GetAllWarehouses();

                // process inventory import file
                var file = File.OpenRead(csvFileFullLocalPath);
                using (TextReader txtRdr = new StreamReader(file))
                {
                    // read csv into object collection
                    CsvReader csvRdr = new CsvReader(txtRdr);
                    List<ImportInventory> importInventories = csvRdr.GetRecords<ImportInventory>().ToList();

                    // get list of affected product SKUs from the import collection
                    List<string> importSkus = importInventories.Select(x => x.Sku).Distinct().ToList();

                    // for each product SKU, update each warehouse inventory records
                    foreach (string importSku in importSkus)
                    {
                        Product nopProduct = this._productService.GetProductBySku(importSku);
                        if (nopProduct != null)
                        {
                            foreach (Warehouse warehouse in allWarehouses)
                            {
                                ImportInventory importProductWarehouse = importInventories.FirstOrDefault(x => x.Sku == importSku && Convert.ToInt32(x.StoreId) == warehouse.UbpcStoreId);
                                if (importProductWarehouse != null)
                                {
                                    int importQuantity = Convert.ToInt32(importProductWarehouse.OnHand);
                                    if (importQuantity < 0) importQuantity = 0; // prevent negative stock counts from entering the system
                                    ProductWarehouseInventory pwi = nopProduct.ProductWarehouseInventory.FirstOrDefault(x => x.WarehouseId == warehouse.Id);
                                    if (pwi != null)
                                    {
                                        // update
                                        pwi.StockQuantity = importQuantity;
                                    }
                                    else
                                    {
                                        // insert
                                        pwi = new ProductWarehouseInventory
                                        {
                                            WarehouseId = warehouse.Id,
                                            ProductId = nopProduct.Id,
                                            StockQuantity = importQuantity,
                                            ReservedQuantity = 0
                                        };
                                        nopProduct.ProductWarehouseInventory.Add(pwi);
                                    }
                                }
                                // else we don't need to do any update/insert for this warehouse-product combination
                            }
                            this._productService.UpdateProduct(nopProduct); // commit updates to the database
                        }
                        // else this product is not on the website, therefore we don't need to record inventory                        
                    }
                }
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
            return "SUCCESS";
        }

        #endregion
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
}
