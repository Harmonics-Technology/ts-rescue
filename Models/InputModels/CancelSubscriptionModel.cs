using System;

namespace TimesheetBE.Models.InputModels
{
    public class CancelSubscriptionModel
    {
        public Guid UserId { get; set; }
        public string Reason { get; set; }
    }
}
