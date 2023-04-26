using TimesheetBE.Context;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Repositories.Interfaces;

namespace TimesheetBE.Repositories
{
    public class LeaveRepository : BaseRepository<Leave>, ILeaveRepository
    {
        public LeaveRepository(AppDbContext context) : base(context)
        {

        }
    }
}
