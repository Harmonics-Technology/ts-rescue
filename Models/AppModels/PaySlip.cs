using System;

namespace TimesheetBE.Models.AppModels
{
    public class PaySlip : BaseModel
    {
        public Guid? EmployeeInformationId { get; set; }
        public EmployeeInformation EmployeeInformation { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double TotalHours { get; set; }
        public double TotalAmount { get; set; }
        public string Rate { get; set; }
        public DateTime PaymentDate { get; set; }
        public Guid InvoiceId { get; set; }
        public Invoice Invoice { get; set; }
        public double TotalEarnings { get; set; }

    }
}