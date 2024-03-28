using System;
using System.Threading.Tasks;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Utilities;

namespace TimesheetBE.Services.Interfaces
{
    public interface IUserDraftService
    {
        Task<StandardResponse<bool>> CreateDraft(UserDraftModel model);
        Task<StandardResponse<bool>> UpdateDraft(UserDraftModel model);
        Task<StandardResponse<bool>> DeleteDraft(Guid id);
        Task<StandardResponse<PagedCollection<UserDraftView>>> ListDrafts(PagingOptions pagingOptions, Guid superAdminId, string role);
    }
}
