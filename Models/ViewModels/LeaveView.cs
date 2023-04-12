﻿using System;

namespace TimesheetBE.Models.ViewModels
{
    public class LeaveView
    {
        public Guid Id { get; set; }
        public Guid EmployeeInformationId { get; set; }
        public EmployeeInformationView EmployeeInformation { get; set; }
        public string LeaveType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ReasonForLeave { get; set; }
        public Guid? WorkAssigneeId { get; set; }
        public UserView WorkAssignee { get; set; }
        public string Status { get; set; }
    }
}