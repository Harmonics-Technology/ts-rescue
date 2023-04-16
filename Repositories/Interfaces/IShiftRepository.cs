using System.Linq;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Repositories.Interfaces
{
    public interface IShiftRepository
    {
        Shift CreateAndReturn(Shift leave);
        Shift Update(Shift leave);
        void Delete(Shift entity);
        IQueryable<Shift> Query();
    }
}
