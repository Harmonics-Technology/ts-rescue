using System;

namespace TimesheetBE.Models.ViewModels
{
    public class TimeSheetApprovedView
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public Guid? EmployeeInformationId { get; set; }
        public EmployeeInformationView EmployeeInformation { get; set; }
        public double TotalHours { get; set; }
        public double NumberOfDays { get; set; }
        public double ApprovedNumberOfHours { get; set; }
        public double ExpectedHours { get; set; }
        public double? ExpectedPayout { get; set; }
        public double? ActualPayout { get; set; }
        public DateTime Date { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? DateModified { get; set; }
    }
}
