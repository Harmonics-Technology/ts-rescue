using System;
using System.Collections.Generic;
using TimesheetBE.Models.IdentityModels;

namespace TimesheetBE.Models.AppModels
{
    public class Invoice : BaseModel
    {
        public Guid? EmployeeInformationId { get; set; }
        public EmployeeInformation EmployeeInformation { get; set; }
        public Expense Expense { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string InvoiceReference { get; set; }
        public double TotalHours { get; set; }
        public double TotalAmount { get; set; }
        public double? ClientTotalAmount { get; set; }
        public string Rate { get; set; }
        public DateTime PaymentDate { get; set; }
        public int StatusId { get; set; }
        public Status Status { get; set; }
        public Guid? PaymentPartnerId { get; set; }
        public User PaymentPartner { get; set; }
        public int InvoiceTypeId { get; set; }
        public Guid? CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; }
        public bool Rejected { get; set; }
        public string? RejectionReason { get; set; }
        public double? HST { get; set; }
        public InvoiceType InvoiceType { get; set; }
        public ICollection<Expense> Expenses { get; set; }
        public ICollection<Payroll> Payrolls { get; set; }
        public ICollection<Invoice> Children { get; set; } 
        public Guid? ParentId { get; set; }
        public Invoice Parent { get; set; }
        //public int? PayrollGroupId { get; set; }
        //public PayrollGroup PayrollGroup { get; set; }
        public Guid? ClientId { get; set; }
        public User Client { get; set; }
        public Guid? ClientInvoiceId { get; set; }
        public Invoice ClientInvoice { get; set; }
        public double? RateForConvertedIvoice { get; set; }
        public double? ConvertedAmount { get; set; }
        public ICollection<Invoice> ClientInvoiceChildren { get; set; }

        public double GetTotalPay()
        {
            if (EmployeeInformation?.FixedAmount == true)
            {
                return TotalAmount + EmployeeInformation.OnBoradingFee;
            }else if(EmployeeInformation?.FixedAmount == false)
            {
                return (EmployeeInformation.OnBoradingFee / 100) + TotalAmount;
            }
            else
            {
                return TotalAmount;
            }
            
        }
    }
}