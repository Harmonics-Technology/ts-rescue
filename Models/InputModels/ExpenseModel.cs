using System;

namespace TimesheetBE.Models.InputModels
{
    public class ExpenseModel
    {
        public Guid TeamMemberId { get; set; }
        public string Description { get; set; }
        public Guid ExpenseTypeId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public DateTime? ExpenseDate { get; set; }
    }
}