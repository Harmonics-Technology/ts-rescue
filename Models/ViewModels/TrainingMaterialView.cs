using System;

namespace TimesheetBE.Models.ViewModels
{
    public class TrainingMaterialView
    {
        public Guid TrainingId { get; set; }
        public string Name { get; set; }
        public int NoOfTrainingFile { get; set; }
        public double Progress { get; set; }
        public DateTime? DateCompleted { get; set; }
        public string Status { get; set; }
    }
}
