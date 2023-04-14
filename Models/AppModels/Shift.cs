using System;

namespace TimesheetBE.Models.AppModels
{
    public class Shift : BaseModel
    {
        public Guid EmployeeInformationId { get; set; }
        public EmployeeInformation EmployeeInformation { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Color { get; set; }
        public string Repeat { get; set; }
        public string Note { get; set; }
        public bool IsPublished { get; set; }
        public bool IsSwapped { get; set; }
        public int? SwapStatusId { get; set; }
    }
}
