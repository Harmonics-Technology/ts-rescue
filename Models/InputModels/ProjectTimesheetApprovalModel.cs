﻿using System;

namespace TimesheetBE.Models.InputModels
{
    public class ProjectTimesheetApprovalModel
    {
        public Guid TimesheetId { get; set; }
        public bool Approve { get; set; }
        public string? Reason { get; set; }
    }
}