using System;
using TimesheetBE.Models.IdentityModels;

namespace TimesheetBE.Models.AppModels
{
    public class Expense : BaseModel
    {
        public Guid TeamMemberId { get; set; }
        public User TeamMember { get; set; }
        public string Description { get; set; }
        public Guid ExpenseTypeId { get; set; }
        public ExpenseType ExpenseType { get; set; }
        public DateTime? ExpenseDate { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public int StatusId { get; set; }
        public Status Status { get; set; }
        public Guid CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; }
        public Guid? InvoiceId { get; set; }
        public Invoice Invoice { get; set; }
    }
}