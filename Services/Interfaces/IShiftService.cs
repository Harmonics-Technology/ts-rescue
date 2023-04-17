using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TimesheetBE.Models.IdentityModels;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Utilities;

namespace TimesheetBE.Services.Interfaces
{
    public interface IShiftService
    {
        Task<StandardResponse<ShiftView>> CreateShift(ShiftModel model);
        Task<StandardResponse<List<ShiftView>>> ListUsersShift(UsersShiftModel model, bool? isPublished = null);
        //Task<StandardResponse<PagedCollection<UsersShiftView>>> ListUsersShift(PagingOptions pagingOptions, UsersShiftModel model, Guid? filterUserId = null);
        //Task<StandardResponse<List<ShiftView>>> ListUsersShift(UsersShiftModel model);
        Task<StandardResponse<PagedCollection<UsersShiftView>>> GetUsersShift(PagingOptions pagingOptions, UsersShiftModel model, Guid? filterUserId = null);
        Task<StandardResponse<PagedCollection<ShiftView>>> GetUserShift(PagingOptions pagingOptions, UsersShiftModel model);
        Task<StandardResponse<bool>> DeleteShift(Guid id);
        Task<StandardResponse<bool>> PublishShifts(DateTime startDate, DateTime endDate);
        ShiftUsersListView GetUsersAndTotalHours(User user, DateTime StartDate, DateTime EndDate);
    }
}
