using System;

namespace TimesheetBE.Models.ViewModels
{
    public class ResourceCapacityDetailView
    {
        public string TaskName { get; set; }
        public string ProjectName { get; set; }
        public double TotalHours { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
    }
}
