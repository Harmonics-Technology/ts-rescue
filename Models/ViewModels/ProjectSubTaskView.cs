using System;
using System.Collections.Generic;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Models.ViewModels
{
    public class ProjectSubTaskView
    {
        public Guid Id { get; set; }
        public Guid ProjectTaskId { get; set; }
        public ProjectTaskView ProjectTask { get; set; }
        public string Name { get; set; }
        public Guid ProjectTaskAsigneeId { get; set; }
        public ProjectTaskAsigneeView ProjectTaskAsignee { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Duration { get; set; }
        public double? HoursSpent { get; set; }
        public string TaskPriority { get; set; }
        public string Note { get; set; }
        public string Status { get; set; }
        public ICollection<ProjectTimesheetView> ProjectTimesheets { get; set; }
    }
}
