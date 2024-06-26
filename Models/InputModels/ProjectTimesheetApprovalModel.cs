﻿using DocumentFormat.OpenXml.Office2010.ExcelAc;
using System;
using System.Collections.Generic;

namespace TimesheetBE.Models.InputModels
{
    public class ProjectTimesheetApprovalModel
    {
        public Guid EmployeeInformationId { get; set; }
        public Guid? TimesheetId { get; set; }
        public bool Approve { get; set; }
        public string? Reason { get; set; }
        //public DateTime? StartDate { get; set; }
        //public DateTime? EndDate { get; set; }
        public List<DateTime> Dates { get; set; }
    }
}
