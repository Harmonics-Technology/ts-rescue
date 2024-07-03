using TimesheetBE.Context;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Repositories.Interfaces;

namespace TimesheetBE.Repositories
{
    public class LeaveConfigurationRepository : BaseRepository<LeaveConfiguration>, ILeaveConfigurationRepository
    {
        public LeaveConfigurationRepository(AppDbContext context) : base(context)
        {

        }
    }
}
