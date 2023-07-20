using TimesheetBE.Context;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Repositories.Interfaces;

namespace TimesheetBE.Repositories
{
    public class LeaveTypeRepository : BaseRepository<LeaveType>, ILeaveTypeRepository
    {
        public LeaveTypeRepository(AppDbContext context) : base(context)
        {

        }
    }
}
