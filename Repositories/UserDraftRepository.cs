using TimesheetBE.Context;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Repositories.Interfaces;

namespace TimesheetBE.Repositories
{
    public class UserDraftRepository : BaseRepository<UserDraft>, IUserDraftRepository
    {
        public UserDraftRepository(AppDbContext context) : base(context)
        {
            
        }
    }
}
