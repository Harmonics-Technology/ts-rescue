using System;
using TimesheetBE.Models.InputModels;

namespace TimesheetBE.Models.AppModels
{
    public class ProjectSubTask : BaseModel
    {
        public Guid TaskId { get; set; }
        public string Name { get; set; }
        public Guid AssigneeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Duration { get; set; }
        public bool TrackedByHours { get; set; }
        public double? DurationInHours { get; set; }
        public string TaskPriority { get; set; }
        public string Note { get; set; }
    }
}
