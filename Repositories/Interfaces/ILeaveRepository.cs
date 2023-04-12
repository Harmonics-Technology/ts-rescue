using System.Linq;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Repositories.Interfaces
{
    public interface ILeaveRepository
    {
        Leave CreateAndReturn(Leave leave);
        Leave Update(Leave leave);
        void Delete(Leave entity);
        IQueryable<Leave> Query();
    }
}
