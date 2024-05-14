using System;

namespace TimesheetBE.Models.AppModels
{
    public class TrainingFile : BaseModel
    {
        public string Category { get; set; }
        public string FileUrl { get; set; }
        public Guid TrainingId { get; set; }
        public Training Training { get; set; }
    }
}
