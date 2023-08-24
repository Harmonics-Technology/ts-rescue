using System;

namespace TimesheetBE.Models.InputModels
{
    public class ProjectTaskAsigneeModel
    {
        public Guid? Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid? TaskId { get; set; }
    }
}
