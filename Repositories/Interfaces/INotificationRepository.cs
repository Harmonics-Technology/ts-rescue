using System.Linq;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Repositories.Interfaces
{
    public interface INotificationRepository
    {
         Notification CreateAndReturn(Notification notification);
         IQueryable<Notification> Query();
         Notification Update(Notification notification);
    }
}