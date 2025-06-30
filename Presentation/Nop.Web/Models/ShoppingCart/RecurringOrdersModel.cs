using Nop.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Web.Models.ShoppingCart;
public partial record RecurringOrdersModel
{
    public RecurringOrdersModel()
    {
        RecurringOrders = new List<RecurringOrder>();
    }
    public List<RecurringOrder> RecurringOrders { get; set; }
}