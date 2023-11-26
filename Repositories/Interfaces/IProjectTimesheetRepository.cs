using System.Linq;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Repositories.Interfaces
{
    public interface IProjectTimesheetRepository
    {
        ProjectTimesheet CreateAndReturn(ProjectTimesheet entity);
        ProjectTimesheet Update(ProjectTimesheet entity);
        void Delete(ProjectTimesheet entity);
        IQueryable<ProjectTimesheet> Query();
    }
}
