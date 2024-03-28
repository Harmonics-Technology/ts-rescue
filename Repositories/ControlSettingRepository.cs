using TimesheetBE.Context;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Repositories.Interfaces;

namespace TimesheetBE.Repositories
{
    public class ControlSettingRepository : BaseRepository<ControlSetting>, IControlSettingRepository
    {
        public ControlSettingRepository(AppDbContext context) : base(context)
        {

        }
    }
}
