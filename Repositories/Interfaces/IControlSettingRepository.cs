using System.Linq;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Repositories.Interfaces
{
    public interface IControlSettingRepository
    {
        ControlSetting CreateAndReturn(ControlSetting setting);
        ControlSetting Update(ControlSetting setting);
        void Delete(ControlSetting setting);
        IQueryable<ControlSetting> Query();
    }
}
