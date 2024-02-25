using System.Threading.Tasks;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Utilities;

namespace TimesheetBE.Services.Interfaces
{
    public interface IUtilityService
    {
        Task<StandardResponse<bool>> SendContactMessage(ContactMessageModel model);
        Task<StandardResponse<UserView>> UpdateClientSubscriptionMigration(UpdateClientSubscriptionModel model);
    }
}
