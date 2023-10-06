using System;

namespace TimesheetBE.Models.InputModels
{
    public class ProjectTimesheetApprovalModel
    {
        public Guid ProjectTaskAsigneeId { get; set; }
        public Guid? TimesheetId { get; set; }
        public bool Approve { get; set; }
        public string? Reason { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
