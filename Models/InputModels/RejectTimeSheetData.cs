using System;

namespace TimesheetBE.Models.InputModels
{
    public class RejectTimeSheetData
    {
        public Guid  EmployeeInformationId { get; set; }
        public DateTime Date { get; set; }
    }
}