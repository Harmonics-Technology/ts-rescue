using System;
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
    }
}
