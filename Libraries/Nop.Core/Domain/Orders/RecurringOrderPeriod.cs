using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Orders
{
	public enum RecurringOrderPeriod
	{
        [Display(Name = "Every 1 Week")]
        Every1Week = 1,
        [Display(Name = "Every 2 Weeks")]
        Every2Weeks = 2,
        [Display(Name = "Every 3 Weeks")]
        Every3Weeks = 3,
        [Display(Name = "Every 4 Weeks")]
        Every4Weeks = 4,
        [Display(Name = "Every 5 Weeks")]
        Every5Weeks = 5,
        [Display(Name = "Every 6 Weeks")]
        Every6Weeks = 6,
        [Display(Name = "Every 7 Weeks")]
        Every7Weeks = 7,
        [Display(Name = "Every 8 Weeks")]
        Every8Weeks = 8,
        [Display(Name = "Every 3 Months")]
        Every3Months = 9,
        [Display(Name = "Every 4 Months")]
        Every4Months = 10,
        [Display(Name = "Every 5 Months")]
        Every5Months = 11,
        [Display(Name = "Every 6 Months")]
        Every6Months = 12,
    }
}
