using System;

namespace TimesheetBE.Models.ViewModels
{
    public class ClientSubscriptionDetailView
    {
        public Guid SuperAdminId { get; set; }
        public int NoOfLicensePurchased { get; set; }
        public int NoOfLicenceUsed { get; set; }
        public Guid? SubscriptionId { get; set; }
        public bool SubscriptionStatus { get; set; }
        public string? SubscriptionType { get; set; }
        public string? PaymentMethod { get; set; }
        public bool AnnualBilling { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
