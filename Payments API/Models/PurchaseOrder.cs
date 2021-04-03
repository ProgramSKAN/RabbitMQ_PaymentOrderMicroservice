using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payments_API.Models
{
    public class PurchaseOrder
    {
        public decimal AmountToPay { get; set; }
        public string PurchaseOrderNumber { get; set; }
        public string CompanyName { get; set; }
        public int PaymentDayTerms { get; set; }
    }
}

