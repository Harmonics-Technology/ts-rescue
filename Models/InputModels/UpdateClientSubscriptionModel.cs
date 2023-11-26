using System;

namespace TimesheetBE.Models.InputModels
{
    public class UpdateClientSubscriptionModel
    {
        public Guid CommandCenterClientId { get; set; }
        public Guid ClientSubscriptionId { get; set; }
        public bool SubscriptionStatus { get; set; }
    }
}
