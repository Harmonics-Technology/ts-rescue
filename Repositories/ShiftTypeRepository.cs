using TimesheetBE.Context;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Repositories.Interfaces;

namespace TimesheetBE.Repositories
{
    public class ShiftTypeRepository : BaseRepository<ShiftType>, IShiftTypeRepository
    {
        public ShiftTypeRepository(AppDbContext context) : base(context)
        {

        }
    }
}
