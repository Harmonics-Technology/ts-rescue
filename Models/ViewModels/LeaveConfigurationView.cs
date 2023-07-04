using System;

namespace TimesheetBE.Models.ViewModels
{
    public class LeaveConfigurationView
    {
        public Guid Id { get; set; }
        public Guid? SuperAdminId { get; set; }
        public int EligibleLeaveDays { get; set; }
        public bool StandardEligibleDays { get; set; }
    }
}
