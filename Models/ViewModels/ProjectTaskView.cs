using System;
using System.Collections.Generic;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Models.ViewModels
{
    public class ProjectTaskView
    {
        public Guid Id { get; set; }
        public Guid SuperAdminId { get; set; }
        public Guid? ProjectId { get; set; }
        public string Name { get; set; }
        public string? Category { get; set; }
        public string? Department { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double DurationInHours { get; set; }
        public string TaskPriority { get; set; }
        public string? Note { get; set; }
        public double? HoursSpent { get; set; }
        public int? SubTaskCount { get; set;}
        public string Status { get; set; }
        public double? Progress { get; set; }
        public double PercentageOfCompletion { get; set; }
        public bool IsCompleted { get; set; }
        public ICollection<ProjectTaskAsigneeView> Assignees { get; set; }
        public ICollection<ProjectSubTaskView> SubTasks { get; set; }   
    }
}
