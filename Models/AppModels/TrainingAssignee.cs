using System;
using TimesheetBE.Models.IdentityModels;

namespace TimesheetBE.Models.AppModels
{
    public class TrainingAssignee : BaseModel
    {
        public Guid UserId { get; set; }
        public User User { get; set; }
        public Guid TrainingId { get; set; }
        public Training Training { get; set; }
        public Guid? TrainingFileId { get; set; }
        public TrainingFile TrainingFile { get; set; }
        public bool IsStarted { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? DateCompleted { get; set; }
        public string? LastRecordedProgress { get; set; }

        public string GetStatus()
        {
            if (IsCompleted == true) return "Completed";
            if (IsStarted && !IsCompleted) return "Ongoing";
            return "Not Started";
        }
    }
}
