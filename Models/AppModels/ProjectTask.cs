using System;

namespace TimesheetBE.Models.AppModels
{
    public class ProjectTask : BaseModel
    {
        public Guid ProjectId { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Duration { get; set; }
        public string Note { get; set; }
    }
}
