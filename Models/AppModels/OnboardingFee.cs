namespace TimesheetBE.Models.AppModels
{
    public class OnboardingFee : BaseModel
    {
        public double Fee { get; set; }
        public int OnboardingFeeTypeId { get; set; }
        public OnboardingFeeType OnboardingFeeType { get; set; }
    }
}
