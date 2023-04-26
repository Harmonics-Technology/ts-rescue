using System.Linq;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Repositories.Interfaces
{
    public interface ILeaveTypeRepository
    {
        LeaveType CreateAndReturn(LeaveType leaveType);
        LeaveType Update(LeaveType leaveType);
        void Delete(LeaveType entity);
        IQueryable<LeaveType> Query();
    }
}
