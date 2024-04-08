using System;
using System.Threading.Tasks;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Utilities;

namespace TimesheetBE.Services.Interfaces
{
    public interface INotificationService
    {
         Task<StandardResponse<NotificationModel>> SendNotification(NotificationModel notification);
         Task<StandardResponse<PagedCollection<NotificationView>>> ListMyNotifications(PagingOptions options);
         Task<StandardResponse<NotificationView>> GetNotification(Guid id);
         Task<StandardResponse<NotificationView>> MarkAsRead(Guid id);
         void SendBirthDayNotificationMessage();
         void SendWorkAnniversaryNotificationMessage();
    }
}