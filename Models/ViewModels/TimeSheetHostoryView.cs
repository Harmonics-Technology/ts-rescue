using System;

namespace TimesheetBE.Models.ViewModels
{
    public class TimeSheetHistoryView
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public Guid? EmployeeInformationId { get; set; }
        public EmployeeInformationView EmployeeInformation { get; set; }
        public int TotalHours { get; set; }
        public int NumberOfDays { get; set; }
        public int ApprovedNumberOfHours { get; set; }
        public DateTime Date { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? DateModified { get; set; }
    }
}