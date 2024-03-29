﻿using System;
using System.Linq;
using TimesheetBE.Models.IdentityModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Repositories;

namespace TimesheetBE.Models.AppModels
{
    public class Leave : BaseModel
    {
        public Guid EmployeeInformationId { get; set; }
        public EmployeeInformation EmployeeInformation { get; set; }
        public Guid LeaveTypeId { get; set; }
        public LeaveType LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? ReasonForLeave { get; set; }
        public Guid? WorkAssigneeId { get; set; }
        public User WorkAssignee { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public int StatusId { get; set; }
        public Status Status { get; set; }
        public bool IsCanceled { get; set; }
    }
}
