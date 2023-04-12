using System;
using TimesheetBE.Models.IdentityModels;
using TimesheetBE.Models.ViewModels;

namespace TimesheetBE.Models.AppModels
{
    public class Leave : BaseModel
    {
        public Guid EmployeeInformationId { get; set; }
        public EmployeeInformation EmployeeInformation { get; set; }
        public Guid LeaveTypeId { get; set; }
        public LeaveType LeaveType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? ReasonForLeave { get; set; }
        public Guid? WorkAssigneeId { get; set; }
        public User WorkAssignee { get; set; }
        public Guid? AssignedSupervisorId { get; set; }
        public User AssignedSupervisor { get; set; }
        public int StatusId { get; set; }
        public Status Status { get; set; }
    }
}
