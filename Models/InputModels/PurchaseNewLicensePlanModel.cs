using System;

namespace TimesheetBE.Models.InputModels
{
    public class PurchaseNewLicensePlanModel
    {
        public int NoOfLicense { get; set; }
        public Guid SuperAdminId { get; set; }
        public Guid SubscriptionId { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
