using System.Linq;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Repositories.Interfaces
{
    public interface ILeaveConfigurationRepository
    {
        LeaveConfiguration CreateAndReturn(LeaveConfiguration leaveConfiguration);
        LeaveConfiguration Update(LeaveConfiguration leaveConfiguration);
        void Delete(LeaveConfiguration leaveConfiguration);
        IQueryable<LeaveConfiguration> Query();
    }
}
