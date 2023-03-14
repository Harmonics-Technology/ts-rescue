using System;
using System.Collections.Generic;

namespace TimesheetBE.Models.ViewModels
{
    public class InvoiceView
    {
        public Guid Id { get; set; }
        public Guid? EmployeeInformationId { get; set; }
        public EmployeeInformationView EmployeeInformation { get; set; }
        public string Name { get; set; }
        public string PaymentPartnerName { get; set; }
        public string PayrollGroupName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string InvoiceReference { get; set; }
        public int TotalHours { get; set; }
        public double TotalAmount { get; set; }
        public string Rate { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; }
        public string InvoiceType { get; set; }
        public string RejectionReason { get; set; }
        public string HST { get; set; }
        public IEnumerable<PayrollView> Payrolls { get; set; }
        public IEnumerable<ExpenseView> Expenses { get; set; }
        public ICollection<InvoiceView> Children { get; set; }
        public DateTime DateCreated { get; set; }
        public double TotalPay { get; set; }
    }
}