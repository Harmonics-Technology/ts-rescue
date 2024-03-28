using TimesheetBE.Context;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Repositories.Interfaces;

namespace TimesheetBE.Repositories
{
    public class ProjectTimesheetRepository : BaseRepository<ProjectTimesheet>, IProjectTimesheetRepository
    {
        public ProjectTimesheetRepository(AppDbContext context) : base(context)
        {
                
        }
    }
}
