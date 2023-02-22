using System;

namespace TimesheetBE.Models.AppModels
{
    public class Contract : BaseModel
    {
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Document { get; set; }
        public int StatusId { get; set; }
        public Status Status { get; set; }
        public Guid EmployeeInformationId { get; set; }
        public EmployeeInformation EmployeeInformation { get; set; }
    }
}