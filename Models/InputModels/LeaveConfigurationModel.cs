using System;

namespace TimesheetBE.Models.InputModels
{
    public class LeaveConfigurationModel
    {
        public Guid? Id { get; set; }
        public Guid? SuperAdminId { get; set; }
        public int EligibleLeaveDays { get; set; }
        public bool StandardEligibleDays { get; set; }
    }
}
