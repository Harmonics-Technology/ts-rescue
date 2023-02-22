using TimesheetBE.Context;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Repositories.Interfaces;

namespace TimesheetBE.Repositories
{
    public class TimeSheetRepository : BaseRepository<TimeSheet>, ITimeSheetRepository
    {
        public TimeSheetRepository(AppDbContext context) : base(context)
        {
        }
    }
}