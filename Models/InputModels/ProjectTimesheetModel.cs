using DocumentFormat.OpenXml.Office2010.ExcelAc;
using System;
using System.Collections.Generic;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Models.InputModels
{
    public class ProjectTimesheetModel
    {
        public Guid? Id { get; set; }
        public Guid ProjectTaskAsigneeId { get; set; }
        public Guid ProjectId { get; set; }
        public Guid? ProjectTaskId { get; set; }
        public Guid? ProjectSubTaskId { get; set; }
        public List<ProjectTimesheetRange> ProjectTimesheets { get; set; }
    }

    public class ProjectTimesheetRange
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double PercentageOfCompletion { get; set; }
        public bool Billable { get; set; }
    }
}
