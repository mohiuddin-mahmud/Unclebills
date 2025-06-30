using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Orders
{
	public partial class RecurringOrderContext : BaseEntity
	{
		public bool IsRecurring { get; set; }
		public RecurringOrderPeriod RecurringOrderPeriod { get; set; }
		public string Name { get; set; }

		public RecurringOrderContext()
		{
			IsRecurring = false;
			RecurringOrderPeriod = RecurringOrderPeriod.Every1Week;
		}
	}
}
