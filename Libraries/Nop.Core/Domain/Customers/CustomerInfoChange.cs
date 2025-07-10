using System;
using System.Collections.Generic;

namespace Nop.Core.Domain.Customers
{
    public partial class CustomerInfoChange : BaseEntity
    {
        private ICollection<CustomerInfoChangeData> _changedDataItems;

        public int CustomerId { get; set; }
        public string RewardsCardNumber { get; set; }
        public DateTime ChangedOn { get; set; }
    }

    public partial class CustomerInfoChangeData : BaseEntity
    {
        public int CustomerInfoChangeId { get; set; }
        public string FieldName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
}
