using System;
using System.Collections.Generic;

namespace TimesheetBE.Models.InputModels
{
    public class ProjectTaskModel
    {
        public Guid? Id { get; set; }
        public Guid SuperAdminId { get; set; }
        public Guid? ProjectId { get; set; }
        public string Name { get; set; }
        //public OprationTaskCategory? Category { get; set; }
        public List<Guid> AssignedUsers { get; set; }
        public string? Department { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? Duration { get; set; }
        public bool TrackedByHours { get; set; }
        public double? DurationInHours { get; set; }
        public TaskPriority? TaskPriority { get; set; }
        public string? Note { get; set; }
        public bool IsAssignedToMe { get; set; }
        public bool IsOperationalTask { get; set; }
        public string? OperationalTaskStatus { get; set; }
    }

    public enum OprationTaskCategory
    {
        Planning_And_Scheduling = 1,
        Resource_Management,
        Budget_Management
    }

    public enum TaskPriority
    {
        High = 1,
        Normal,
        Low
    }
}
