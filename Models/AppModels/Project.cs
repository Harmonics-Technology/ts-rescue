using System;
using System.Collections.Generic;

namespace TimesheetBE.Models.AppModels
{
    public class Project : BaseModel
    {
        public Guid SuperAdminId { get; set; }     
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Duration { get; set; }
        public decimal Budget { get; set; } 
        public string Note { get; set; }
        public string? DocumentURL { get; set; }
        public bool IsCompleted { get; set; }
        public decimal BudgetSpent { get; set; }
        public double HoursSpent { get; set; }
        public decimal? BudgetThreshold { get; set; }
        public Guid? ProjectManagerId { get; set; }
        public string? Currency { get; set; }
        public ICollection<ProjectTaskAsignee> Assignees { get; set; }

        public string GetStatus()
        {
            if (IsCompleted == true) return "Completed";
            if (DateTime.Now.Date >= StartDate.Date) return "Ongoing";
            return "Not Started";
        }
    }
}
