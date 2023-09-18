using System;

namespace TimesheetBE.Models.InputModels
{
    public class LeaveModel
    {
        public Guid? Id { get; set; }
        public Guid EmployeeInformationId { get; set; }
        public Guid LeaveTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? ReasonForLeave { get; set; }
        public Guid? WorkAssigneeId { get; set; }
        public int NoOfLeaveDaysApplied { get; set; }
    }

    public enum LeaveStatuses
    {
        Approved = 1,
        Declined,
        Canceled
    }
}
