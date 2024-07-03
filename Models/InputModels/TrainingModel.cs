using System;
using System.Collections.Generic;

namespace TimesheetBE.Models.InputModels
{
    public class TrainingModel
    {
        public Guid? Id { get; set; }
        public Guid SuperAdminId { get; set; }
        public string Name { get; set; }
        public bool IsAllParticipant { get; set; }
        public string Note { get; set; }
        public List<Guid> AssignedUsers { get; set; }
        public List<TrainingFileModel> TrainingFiles { get; set; }
    }

    public class TrainingFileModel
    {
        public string Title { get; set; }
        public string Category { get; set; }
        public string FileUrl { get; set; }
    }
}
