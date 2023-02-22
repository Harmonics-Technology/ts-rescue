using System;

namespace TimesheetBE.Models.ViewModels
{
    public class PayrollView
    {
        public Guid PayrollId { get; set; }
        public Guid EmployeeInformationId { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime PaymentDate { get; set; }
        public int TotalHours { get; set; }
        public double Rate { get; set; }
        public double TotalAmount { get; set; }
    }
}
