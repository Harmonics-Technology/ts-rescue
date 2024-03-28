using TimesheetBE.Context;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Repositories.Interfaces;

namespace TimesheetBE.Repositories
{
    public class ProjectManagementSettingRepository : BaseRepository<ProjectManagementSetting>, IProjectManagementSettingRepository
    {
        public ProjectManagementSettingRepository(AppDbContext context) : base(context)
        {
            
        }
    }
}
