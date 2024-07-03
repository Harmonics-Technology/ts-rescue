using System;

namespace TimesheetBE.Models.AppModels
{
    public class OnboardingFee : BaseModel
    {
        public Guid? SuperAdminId { get; set; }
        public Guid? PaymentPartnerId { get; set; }
        public double Fee { get; set; }
        public string OnboardingFeeType { get; set; }
        //public OnboardingFeeType OnboardingFeeType { get; set; }
    }
}
