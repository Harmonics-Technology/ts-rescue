using System;

namespace TimesheetBE.Models.ViewModels
{
    public class OnboardingFeeView
    {
        public Guid Id { get; set; }
        public double Fee { get; set; }
        public int OnbordingFeeTypeId { get; set; }
    }
}
