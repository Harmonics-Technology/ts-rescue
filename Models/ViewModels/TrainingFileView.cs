using System;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Models.ViewModels
{
    public class TrainingFileView
    {
        public Guid Id { get; set; }
        public string Category { get; set; }
        public string FileUrl { get; set; }
        public Guid TrainingId { get; set; }
        public TrainingView Training { get; set; }
    }
}
