using System;
using System.Collections.Generic;

namespace TimesheetBE.Models.InputModels
{
    public class ProjectModel
    {
        public Guid? Id { get; set; }
        public Guid SuperAdminId { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Duration { get; set; }
        public decimal Budget { get; set; }
        public List<Guid> AssignedUsers { get; set; }
        public string Note { get; set; }
        public string? DocumentURL { get; set; }
        public int StatusId { get; set; }
        public decimal? BudgetThreshold { get; set; }
    }

    public enum ProjectStatus
    {
        NotStarted = 1,
        InProgress,
        Completed
    }
}
