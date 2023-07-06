using System;

namespace TimesheetBE.Models.InputModels
{
    public class OnboardingFeeModel
    {
        public Guid? SuperAdminId { get; set; }
        public double Fee { get; set; }
        public int OnboardingTypeId { get; set; }
    }
}
