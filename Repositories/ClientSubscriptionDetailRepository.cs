using TimesheetBE.Context;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Repositories.Interfaces;

namespace TimesheetBE.Repositories
{
    public class ClientSubscriptionDetailRepository : BaseRepository<ClientSubscriptionDetail>, IClientSubscriptionDetailRepository
    {
        public ClientSubscriptionDetailRepository(AppDbContext context) : base(context)
        {
            
        }
    }
}
