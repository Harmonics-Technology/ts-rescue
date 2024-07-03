using System.Linq;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Repositories.Interfaces
{
    public interface IProjectTaskAsigneeRepository
    {
        ProjectTaskAsignee CreateAndReturn(ProjectTaskAsignee entity);
        ProjectTaskAsignee Update(ProjectTaskAsignee entity);
        void Delete(ProjectTaskAsignee entity);
        IQueryable<ProjectTaskAsignee> Query();
    }
}
