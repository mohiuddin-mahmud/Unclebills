using Nop.Core.Domain.Shipping;

namespace Nop.Services.Shipping
{
    public interface IRestrictedWarehouses
    {
        IList<Warehouse> GetAllowedWarehouses(IList<Warehouse> allWarehouses);
    }
}
