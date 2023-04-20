using System.Linq;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Repositories.Interfaces
{
    public interface ISwapRepository
    {
        Swap CreateAndReturn(Swap swap);
        Swap Update(Swap swap);
        void Delete(Swap entity);
        IQueryable<Swap> Query();
    }
}
