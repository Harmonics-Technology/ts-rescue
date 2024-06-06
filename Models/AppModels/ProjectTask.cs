using System;
using System.Collections.Generic;
using TimesheetBE.Models.InputModels;

namespace TimesheetBE.Models.AppModels
{
    public class ProjectTask : BaseModel
    {
        public Guid SuperAdminId { get; set; }
        public Guid? ProjectId { get; set; }
        public string Name { get; set; }
        //public string? Category { get; set; }
        public string? Department { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? Duration { get; set; }
        public bool? TrackedByHours { get; set; }
        public double? DurationInHours { get; set; }
        public string? TaskPriority { get; set; }
        public string? Note { get; set; }
        public bool IsCompleted { get; set; }
        public double PercentageOfCompletion { get; set; }
        public bool? IsAssignedToMe { get; set; }
        public bool IsOperationalTask { get; set; }
        public string? OperationalTaskStatus { get; set; }
        public ICollection<ProjectSubTask> SubTasks { get; set; }
        public ICollection<ProjectTaskAsignee> Assignees { get; set; }

        public string GetStatus()
        {
            if (IsCompleted == true) return "Completed";
            if (DateTime.Now.Date >= StartDate.Date && IsCompleted == false) return "Ongoing";
            return "Not Started";
        }
    }
}
