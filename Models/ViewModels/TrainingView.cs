using System;
using System.Collections.Generic;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Models.ViewModels
{
    public class TrainingView
    {
        public Guid Id { get; set; }
        public Guid SuperAdminId { get; set; }
        public string Name { get; set; }
        public bool IsAllParticipant { get; set; }
        public string Note { get; set; }
        public DateTime DateCreated { get; set; } 
        public DateTime DateModified { get; set; }
        public double? Progress { get; set; }
        public ICollection<TrainingAssigneeView> Assignees { get; set; }
        public ICollection<TrainingFileView> Files { get; set; }
    }
}
