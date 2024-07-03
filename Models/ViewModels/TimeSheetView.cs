using System;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Models.ViewModels
{
    public class TimeSheetView
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public bool IsApproved { get; set; }
        public Guid EmployeeInformationId { get; set; }
        public EmployeeInformationView EmployeeInformation { get; set; }
        public double Hours { get; set; }
        public string Status { get; set; }
        public string RejectionReason { get; set; }
        public double? ExpectedHours { get; set; }
        public double? TotalHours { get; set; }
        public double? ExpectedPayout { get; set; }
        public double? ActualPayout { get; set; }
        public bool OnLeaveAndEligibleForLeave { get; set; }
        public bool OnLeave { get; set; }
        public DateTime DateModified { get; set; }
    }
}
