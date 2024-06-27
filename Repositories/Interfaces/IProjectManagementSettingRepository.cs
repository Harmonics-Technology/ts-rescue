using System.Linq;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Repositories.Interfaces
{
    public interface IProjectManagementSettingRepository
    {
        ProjectManagementSetting CreateAndReturn(ProjectManagementSetting model);
        ProjectManagementSetting Update(ProjectManagementSetting model);
        void Delete(ProjectManagementSetting model);
        IQueryable<ProjectManagementSetting> Query();
    }
}
