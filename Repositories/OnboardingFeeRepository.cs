using TimesheetBE.Context;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Repositories.Interfaces;

namespace TimesheetBE.Repositories
{
    public class OnboardingFeeRepository : BaseRepository<OnboardingFee>, IOnboardingFeeRepository
    {
        public OnboardingFeeRepository(AppDbContext context) : base(context)
        {

        }
    }
}
