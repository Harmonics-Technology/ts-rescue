using System;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Models.ViewModels
{
    public class TrainingAssigneeView
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public TrainingAssigneeUserView User { get; set; }
        public Guid TrainingId { get; set; }
        public TrainingView Training { get; set; }
        public Guid? TrainingFileId { get; set; }
        public TrainingFileView TrainingFile { get; set; }
        public bool IsStarted { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime DateCompleted { get; set; }
        public string Status { get; set; }
        public string? LastRecordedProgress { get; set; }
    }
}
