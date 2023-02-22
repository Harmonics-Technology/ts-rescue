using System;

namespace TimesheetBE.Models.ViewModels
{
    public class RecentTimeSheetView
    {
        public string Name { get; set; }
        public string Year { get; set; }
        public string Month { get; set; }
        public double Hours { get; set; }
        public double NumberOfDays { get; set; }
        public Guid EmployeeInformationId { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
