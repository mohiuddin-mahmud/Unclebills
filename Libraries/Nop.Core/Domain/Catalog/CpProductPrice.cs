using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Catalog
{
    public partial class CpProductPrice : BaseEntity
    {
        public string Sku { get; set; }
        public decimal RegPrice { get; set; }
        public decimal EvCardPrice { get; set; }
    }
}
