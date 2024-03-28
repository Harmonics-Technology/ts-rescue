using System;

namespace TimesheetBE.Models.InputModels
{
    public class UpdateClientStripeSubscriptionModel
    {
        public Guid UserId { get; set; }
        public Guid SubscriptionId { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
