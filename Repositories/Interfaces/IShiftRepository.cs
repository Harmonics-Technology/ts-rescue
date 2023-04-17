using System.Linq;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Repositories.Interfaces
{
    public interface IShiftRepository
    {
        Shift CreateAndReturn(Shift shift);
        Shift Update(Shift shift);
        void Delete(Shift entity);
        IQueryable<Shift> Query();
    }
}
