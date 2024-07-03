using System;

namespace TimesheetBE.Models.AppModels
{
    public class LeaveConfiguration : BaseModel
    {
        public Guid? SuperAdminId { get; set; }
        public int EligibleLeaveDays { get; set; }
        public bool IsStandardEligibleDays { get; set; }
    }
}
