using System;

namespace TimesheetBE.Models.InputModels
{
    public class InvoiceModel
    {
    }

    public class TreatInvoiceModel
    {
        public Guid InvoiceId { get; set; }
        public double Rate { get; set; }
    }
}
