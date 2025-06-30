using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Orders;

public partial class RecurringOrder : BaseEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int CustomerId { get; set; }
    public string SubscriptionId { get; set; }
    public int InitialOrderId { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime UpdatedOnUtc { get; set; }
    public DateTime NextOrderUtc { get; set; }
    public int RecurringOrderPeriodId { get; set; }
    public bool Deleted { get; set; }
    public bool LastPaymentFailed { get; set; }
    public RecurringOrderPeriod RecurringOrderPeriod
    {
        get
        {
            return (RecurringOrderPeriod)RecurringOrderPeriodId;
        }
        set
        {
            RecurringOrderPeriodId = (int)value;
        }
    }
}