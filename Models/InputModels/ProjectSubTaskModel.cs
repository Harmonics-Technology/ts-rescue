using System;

namespace TimesheetBE.Models.InputModels
{
    public class ProjectSubTaskModel
    {
        public Guid? Id { get; set; }
        public Guid TaskId { get; set; }
        public string Name { get; set; }
        public Guid AssigneeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Duration { get; set; }
        public bool TrackedByHours { get; set; }
        public double? DurationInHours { get; set; }
        public TaskPriority TaskPriority { get; set; }
        public string Note { get; set; }
    }
}
