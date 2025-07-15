using Nop.Core.Domain.Common;
using Nop.Core.Domain.Shipping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Nop.Services.Custom.LocationService;

namespace Nop.Services.Custom;

public interface ILocationService
{
    Warehouse GetNearestWarehouse(Address address, IList<Warehouse> warehouses);
    double GetDistanceInMiles(Address address, Warehouse wareHouse);
    Coordinates GetLatLon(Address address);
}