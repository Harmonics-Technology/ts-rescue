using System;

namespace TimesheetBE.Models.ViewModels
{
    public class RecentTimeSheetDashboardView
    {
        public Guid EmployeeInformationId { get; set; }
        public DateTime Date { get; set; }
        public string FullName { get; set; }
        public string  Status { get; set; }
        public bool IsApproved { get; set; }
        public int Hours { get; set; }
    }
}
