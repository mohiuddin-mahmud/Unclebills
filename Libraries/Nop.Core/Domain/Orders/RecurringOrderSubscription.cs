using Nop.Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Orders
{
	public class RecurringOrderSubscription
	{
		public string Name { get; set; }
		public decimal Total { get; set; }
		public string ShippingStreetAddress { get; set; }
		public string ShippingCity { get; set; }
		public string ShippingState { get; set; }
		public string ShippingZip { get; set; }
	}
}
