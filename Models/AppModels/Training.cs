using System;
using System.Collections.Generic;

namespace TimesheetBE.Models.AppModels
{
    public class Training : BaseModel
    {
        public Guid SuperAdminId { get; set; }
        public string Name { get; set; }
        public bool IsAllParticipant { get; set; }
        public string Note { get; set; }
        public ICollection<TrainingAssignee> Assignees { get; set; }
        public ICollection<TrainingFile> Files { get; set; }
    }
}
