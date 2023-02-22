using System;

namespace TimesheetBE.Models.AppModels
{
    public class TimeSheet : BaseModel
    {
        public DateTime Date { get; set; }
        public bool IsApproved { get; set; }
        public Guid EmployeeInformationId { get; set; }
        public EmployeeInformation EmployeeInformation { get; set; }
        public int Hours { get; set; }
        public int? StatusId { get; set; }
        public Status Status { get; set; }
        public string? RejectionReason { get; set; }
    }
}