using System;

namespace TimesheetBE.Models.ViewModels
{
    public class OnboardingFeeView
    {
        public Guid Id { get; set; }
        public double Fee { get; set; }
        public string OnbordingFeeType { get; set; }
        public string? Currency { get; set; }
    }
}
