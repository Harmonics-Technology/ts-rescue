using System;

namespace TimesheetBE.Models.InputModels
{
    public class UpdateClientSubscriptionModel
    {
        public Guid CommandCenterClientId { get; set; }
        public Guid ClientSubscriptionId { get; set; }
        public bool SubscriptionStatus { get; set; }
        public int NoOfLicense { get; set; }
        public string SubscriptionType { get; set; }
        public bool AnnualBilling { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal SubscriptionPrice { get; set; }
    }
}
