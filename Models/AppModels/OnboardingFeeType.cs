using System.Collections.Generic;

namespace TimesheetBE.Models.AppModels
{
    public class OnboardingFeeType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<OnboardingFee> OnboradingFees { get; set; }
    }

    enum OnboradingFeeTypes
    {
        PERCENTAGE = 1,
        FIXEDAMOUNT,
        HST
    }
}
