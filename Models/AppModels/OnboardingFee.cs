using System;

namespace TimesheetBE.Models.AppModels
{
    public class OnboardingFee : BaseModel
    {
        public Guid? SuperAdminId { get; set; }
        public double Fee { get; set; }
        public int OnboardingFeeTypeId { get; set; }
        public OnboardingFeeType OnboardingFeeType { get; set; }
    }
}
