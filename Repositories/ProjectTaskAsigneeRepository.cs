using TimesheetBE.Context;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Repositories.Interfaces;

namespace TimesheetBE.Repositories
{
    public class ProjectTaskAsigneeRepository : BaseRepository<ProjectTaskAsignee>, IProjectTaskAsigneeRepository
    {
        public ProjectTaskAsigneeRepository(AppDbContext context) : base(context)
        {
            
        }
    }
}
