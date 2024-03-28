using System;
using System.Collections.Generic;
using TimesheetBE.Models.IdentityModels;

namespace TimesheetBE.Models.AppModels
{
    public class ProjectTaskAsignee : BaseModel
    {
        public Guid UserId { get; set; }
        public User User { get; set; }
        public Guid? ProjectId { get; set; }
        public Project Project { get; set; }
        public Guid? ProjectTaskId { get; set; }
        public ProjectTask ProjectTask { get; set; }
        public double HoursLogged { get; set; }
        public decimal? Budget { get; set; }
        public decimal? BudgetSpent { get; set; }
        public bool Disabled { get; set; }
        public ICollection<ProjectSubTask> SubTasks { get; set; }
    }
}
