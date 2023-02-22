using System;

namespace TimesheetBE.Models.ViewModels
{
    public class RecentPayrollView
    {
        public string Client { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double Rate { get; set; }
        public double TotalAmount { get; set; }
        public string Status { get; set; }
    }
}
