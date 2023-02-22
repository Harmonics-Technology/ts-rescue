using System.Linq;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Repositories.Interfaces
{
    public interface ITimeSheetRepository
    {
        TimeSheet CreateAndReturn(TimeSheet timeSheet);
        IQueryable<TimeSheet> Query();
        TimeSheet Update(TimeSheet timeSheet);
    }
}