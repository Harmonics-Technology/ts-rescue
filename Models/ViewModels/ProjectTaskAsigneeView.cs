using System;
using System.Collections.Generic;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Models.IdentityModels;

namespace TimesheetBE.Models.ViewModels
{
    public class ProjectTaskAsigneeView
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public UserView User { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid? ProjectTaskId { get; set; }
        public ProjectTaskView ProjectTask { get; set; }
        public double HoursLogged { get; set; }
        public decimal Budget { get; set; }
        public decimal BudgetSpent { get; set; }
        public ICollection<ProjectSubTaskView> SubTasks { get; set; }
    }
}
