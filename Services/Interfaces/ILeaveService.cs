using System;
using System.Threading.Tasks;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Utilities;

namespace TimesheetBE.Services.Interfaces
{
    public interface ILeaveService
    {
        Task<StandardResponse<LeaveTypeView>> AddLeaveType(LeaveTypeModel model);
        Task<StandardResponse<LeaveTypeView>> UpdateLeaveType(Guid id, LeaveTypeModel model);
        Task<StandardResponse<bool>> DeleteLeaveType(Guid id);
        Task<StandardResponse<PagedCollection<LeaveTypeView>>> LeaveTypes(PagingOptions pagingOptions);
        Task<StandardResponse<LeaveView>> CreateLeave(LeaveModel model);
        Task<StandardResponse<PagedCollection<LeaveView>>> ListLeaves(PagingOptions pagingOptions, string search = null, DateFilter dateFilter = null);
        Task<StandardResponse<bool>> TreatLeave(Guid leaveId, LeaveStatuses status);
        Task<StandardResponse<bool>> DeleteLeave(Guid id);
    }
}
