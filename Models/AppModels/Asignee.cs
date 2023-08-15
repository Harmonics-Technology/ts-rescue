using System;

namespace TimesheetBE.Models.AppModels
{
    public class Asignee : BaseModel
    {
        public Guid UserId { get; set; }
        public Guid ProjectId { get; set; }
        public Guid TaskId { get; set; }
    }
}
