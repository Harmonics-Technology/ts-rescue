using System.Collections.Generic;

namespace TimesheetBE.Models.ViewModels
{
    public class DashboardView
    {
        public int TotalClients { get; set; }
        public int TotalTeamMembers { get; set; }
        public int TotalDownLines { get; set; }
        public List<UserView> RecentCLients { get; set; }
        public List<InvoiceView> RecentPayrolls { get; set; }
        public List<InvoiceView> RecentInvoiced { get; set; }
        public List<PaySlipView> RecentPayslips { get; set; }
        public List<TimeSheetHistoryView> RecentTimeSheet { get; set; }

    }
}