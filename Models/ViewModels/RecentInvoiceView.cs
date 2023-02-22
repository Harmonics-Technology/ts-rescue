using System;

namespace TimesheetBE.Models.ViewModels
{
    public class RecentInvoiceView
    {
        public string Client { get; set; }
        public string TeamMemberName { get; set; }
        public string InvoiceReference { get; set; }
        public double Amount { get; set; }
        public DateTime GeneratedOn { get; set; }
        public string Status { get; set; }
    }
}