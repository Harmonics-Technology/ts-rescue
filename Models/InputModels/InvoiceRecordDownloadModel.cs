using System;
using System.Collections.Generic;

namespace TimesheetBE.Models.InputModels
{
    public class InvoiceRecordDownloadModel
    {
        public InvoiceRecord Record { get; set; }
        public Guid? PayrollGroupId { get; set; }
        public List<string> rowHeaders { get; set; }
    }

    public enum InvoiceRecord
    {
        PendingPayrolls = 1,
        ProcessedPayrolls,
        PendingInvoices,
        ProcessedInvoices,
        PaymentPartnerInvoices,
        ClientInvoices

    }
}
