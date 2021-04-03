using System;
using System.Collections.Generic;
using System.Text;

namespace PurchaseOrderConsumer.RabbitMQ
{
    public class PurchaseOrder
    {
        public decimal AmountToPay;
        public string PurchaseOrderNumber;
        public string CompanyName;
        public int PaymentDayTerms;
    }
}
