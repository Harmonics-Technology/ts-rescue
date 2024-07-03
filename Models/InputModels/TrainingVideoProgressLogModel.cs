using System;

namespace TimesheetBE.Models.InputModels
{
    public class TrainingVideoProgressLogModel
    {
        public Guid TrainingId { get; set; }
        public Guid UserId { get; set; }
        public Guid TrainingFileId { get; set; }
        public string LastRecordedProgress { get; set; }
    }
}
