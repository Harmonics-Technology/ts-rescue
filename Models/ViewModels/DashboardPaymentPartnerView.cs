using System.Collections.Generic;

namespace TimesheetBE.Models.ViewModels
{
    public class DashboardPaymentPartnerView
    {
        public List<RecentPayrollView> RecentPayroll { get; set; }
        public List<InvoiceView> RecentApprovedInvoice { get; set; }
        public List<InvoiceView> RecentInvoicedInvoice { get; set; }
        //public List<RecentInvoiceView> RecentInvoice { get; set; }
    }
}
