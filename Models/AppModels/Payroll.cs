using System;
using TimesheetBE.Models.IdentityModels;

namespace TimesheetBE.Models.AppModels
{
    public class Payroll : BaseModel
    {
        public Guid EmployeeInformationId { get; set; }
        public EmployeeInformation EmployeeInformation { get; set; }
        public string Name  { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalHours { get; set; }
        public double TotalAmount { get; set; }
        public double Rate { get; set; }
        public DateTime PaymentDate { get; set; }
        public int PayRollTypeId { get; set; }
        public PayRollType PayRollType { get; set; }
        public int StatusId { get; set; }
        public Status Status { get; set; }
        public Guid? PaymentPartnerId { get; set; }
        public User PaymentPartner { get; set; }
        public Guid? InvoiceId { get; set; }
        public Invoice Invoice { get; set; }
    }
}