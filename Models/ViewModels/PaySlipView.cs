using System;

namespace TimesheetBE.Models.ViewModels
{
    public class PaySlipView
    {
        public Guid Id { get; set; }
        public Guid InvoiceId { get; set; }
        public InvoiceView Invoice { get; set; }
        public Guid EmployeeInformationId { get; set; }
        public DateTime DateCreated { get; set; }
        public double TotalEarnings { get; set; }
    }
}
