using System.Linq;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Repositories.Interfaces
{
    public interface IProjectSubTaskRepository
    {
        ProjectSubTask CreateAndReturn(ProjectSubTask entity);
        ProjectSubTask Update(ProjectSubTask entity);
        void Delete(ProjectSubTask entity);
        IQueryable<ProjectSubTask> Query();
    }
}
