using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Custom
{
    public interface IProductSubstitutionService
    {
        bool ProductOutOfStock(ShoppingCartItem sci);
        IList<Product> GetSubstituteProducts(ShoppingCartItem sci);
        void SubstituteCartItems(List<ShoppingCartItem> items, string substitutionAsString);
    }
}
