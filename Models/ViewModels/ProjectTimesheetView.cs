using System;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Models.ViewModels
{
    public class ProjectTimesheetView
    {
        public Guid Id { get; set; }
        public Guid ProjectTaskAsigneeId { get; set; }
        public Guid ProjectId { get; set; }
        public ProjectView Project { get; set; }
        public Guid? ProjectTaskId { get; set; }
        public Guid? ProjectSubTaskId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double PercentageOfCompletion { get; set; }
        public bool Billable { get; set; }
        public double TotalHours { get; set; }
        public decimal AmountEarned { get; set; }
    }
}
