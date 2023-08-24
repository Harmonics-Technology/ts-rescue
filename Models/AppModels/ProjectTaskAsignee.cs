using System;

namespace TimesheetBE.Models.AppModels
{
    public class ProjectTaskAsignee : BaseModel
    {
        public Guid UserId { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid? TaskId { get; set; }
    }
}
