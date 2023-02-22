using System;

namespace TimesheetBE.Models.InputModels
{
    public class RejectPaymentPartnerInvoiceModel
    {
        public Guid InvoiceId { get; set; }
        public string RejectionReason { get; set; }
    }
}
