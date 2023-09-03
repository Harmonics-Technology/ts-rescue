using System;
using System.Collections.Generic;
using TimesheetBE.Models.IdentityModels;
using TimesheetBE.Models.InputModels;

namespace TimesheetBE.Models.AppModels
{
    public class ProjectSubTask : BaseModel
    {
        public Guid ProjectTaskId { get; set; }
        public ProjectTask ProjectTask { get; set; }
        public string Name { get; set; }
        public Guid ProjectTaskAsigneeId { get; set; }
        public ProjectTaskAsignee ProjectTaskAsignee { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Duration { get; set; }
        public bool TrackedByHours { get; set; }
        public double? DurationInHours { get; set; }
        public string TaskPriority { get; set; }
        public string Note { get; set; }
        public bool IsCompleted { get; set; }
        public ICollection<ProjectTimesheet> ProjectTimesheets { get; set; }

        public string GetStatus()
        {
            if (IsCompleted == true) return "Completed";
            if (DateTime.Now.Date > StartDate.Date) return "Ongoing";
            return "Not Started";
        }
    }
}
