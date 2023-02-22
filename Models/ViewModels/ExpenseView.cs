using System;

namespace TimesheetBE.Models.ViewModels
{
    public class ExpenseView
    {
        public Guid Id { get; set; }
        public Guid TeamMemberId { get; set; }
        public UserView TeamMember { get; set; }
        public string Description { get; set; }
        public string ExpenseType { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? ExpenseDate { get; set; }
    }
}