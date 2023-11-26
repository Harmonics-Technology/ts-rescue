using System;
using System.Collections.Generic;

namespace TimesheetBE.Models.InputModels
{
    public class PaymentPartnerInvoiceModel
    {
        public List<Guid> InvoiceIds { get; set; }
        public double TotalAmount { get; set; }
        public string Rate { get; set; }
        public Guid ClientId { get; set; }
    }
}