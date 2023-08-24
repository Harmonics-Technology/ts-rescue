using TimesheetBE.Context;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Repositories.Interfaces;

namespace TimesheetBE.Repositories
{
    public class ProjectSubTaskRepository : BaseRepository<ProjectSubTask>, IProjectSubTaskRepository
    {
        public ProjectSubTaskRepository(AppDbContext context) : base(context)
        {
                
        }
    }
}
