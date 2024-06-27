using System.Linq;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Repositories.Interfaces
{
    public interface IShiftTypeRepository
    {
        ShiftType CreateAndReturn(ShiftType shiftType);
        ShiftType Update(ShiftType shiftType);
        void Delete(ShiftType shiftType);
        IQueryable<ShiftType> Query();
    }
}
