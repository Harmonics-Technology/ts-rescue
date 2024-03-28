using System.Linq;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Repositories.Interfaces
{
    public interface IProjectRepository
    {
        Project CreateAndReturn(Project entity);
        Project Update(Project entity);
        void Delete(Project entity);
        IQueryable<Project> Query();
    }
}
