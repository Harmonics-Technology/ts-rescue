using System.Linq;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Repositories.Interfaces
{
    public interface IProjectTaskRepository
    {
        ProjectTask CreateAndReturn(ProjectTask entity);
        ProjectTask Update(ProjectTask entity);
        void Delete(ProjectTask entity);
        IQueryable<ProjectTask> Query();
    }
}
