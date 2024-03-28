using System;
using System.Collections.Generic;

namespace TimesheetBE.Models.InputModels
{
    public class PaymentPartnerInvoiceModel
    {
        public List<ApprovedPayrollInvoices> Invoices { get; set; }
        public double TotalAmount { get; set; }
        public string Rate { get; set; }
        public Guid ClientId { get; set; }
    }

    public class ApprovedPayrollInvoices
    {
        public Guid InvoiceId { get; set; }
        public double ExchangeRate { get; set; }
    }
}