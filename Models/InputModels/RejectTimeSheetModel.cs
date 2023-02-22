using System;

namespace TimesheetBE.Models.InputModels
{
    public class RejectTimeSheetModel
    {
        public Guid  EmployeeInformationId { get; set; }
        public DateTime Date { get; set; }
        public string Reason { get; set; }
    }
}