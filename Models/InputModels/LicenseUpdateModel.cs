using System;

namespace TimesheetBE.Models.InputModels
{
    public class LicenseUpdateModel
    {
        public Guid SuperAdminId { get; set; }
        public Guid SubscriptionId { get; set; }
        public int NoOfLicense { get; set; }
        public decimal TotalAmount { get; set; }
        public bool Remove { get; set; }
    }
}
