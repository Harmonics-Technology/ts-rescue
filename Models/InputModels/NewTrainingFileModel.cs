using System;

namespace TimesheetBE.Models.InputModels
{
    public class NewTrainingFileModel
    {
        public Guid TrainingId { get; set; }
        public string FileURL { get; set; }
        public string Category { get; set; }
    }
}
