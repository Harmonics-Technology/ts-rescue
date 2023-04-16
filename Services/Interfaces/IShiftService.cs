using System;
using System.Threading.Tasks;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Utilities;

namespace TimesheetBE.Services.Interfaces
{
    public interface IShiftService
    {
        Task<StandardResponse<ShiftView>> CreateShift(ShiftModel model);
        Task<StandardResponse<PagedCollection<UsersShiftView>>> ListUsersShift(PagingOptions pagingOptions, UsersShiftModel model, Guid? filterUserId = null);
    }
}
