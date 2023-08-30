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
        public ICollection<ProjectTaskAsignee> Assignees { get; set; }
    }
}
