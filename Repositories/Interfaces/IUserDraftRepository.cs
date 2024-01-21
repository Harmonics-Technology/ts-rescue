using System.Linq;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Repositories.Interfaces
{
    public interface IUserDraftRepository
    {
        UserDraft CreateAndReturn(UserDraft model);
        UserDraft Update(UserDraft model);
        void Delete(UserDraft model);
        IQueryable<UserDraft> Query();
    }
}
