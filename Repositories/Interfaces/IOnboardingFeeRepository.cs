using System.Linq;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Repositories.Interfaces
{
    public interface IOnboardingFeeRepository
    {
        OnboardingFee CreateAndReturn(OnboardingFee onBoardingFee);
        IQueryable<OnboardingFee> Query();
        OnboardingFee Update(OnboardingFee onboradingFee);
        void Delete(OnboardingFee onboradingFee);
    }
}
