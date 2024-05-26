using System;

namespace TimesheetBE.Models.AppModels
{
    public class TrainingVideoProgressLog : BaseModel
    {
        public Guid TrainingId { get; set; }
        public Guid UserId { get; set; }
        public Guid TrainingFileId { get; set; }
        public string LastRecordedProgress { get; set; }
    }
}
