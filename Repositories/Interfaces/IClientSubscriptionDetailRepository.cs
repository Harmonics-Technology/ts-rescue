using System.Linq;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Repositories.Interfaces
{
    public interface IClientSubscriptionDetailRepository
    {
        ClientSubscriptionDetail CreateAndReturn(ClientSubscriptionDetail model);
        ClientSubscriptionDetail Update(ClientSubscriptionDetail model);
        void Delete(ClientSubscriptionDetail model);
        IQueryable<ClientSubscriptionDetail> Query();
    }
}
