using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TimesheetBE.Controllers;
using TimesheetBE.Models;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Repositories.Interfaces;
using TimesheetBE.Services.Interfaces;
using TimesheetBE.Utilities;
using TimesheetBE.Utilities.Extentions;

namespace TimesheetBE.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IMapper _mapper;

        public NotificationService(INotificationRepository notificationRepository, IHttpContextAccessor httpContextAccessor, IConfigurationProvider configurationProvider, IMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _httpContextAccessor = httpContextAccessor;
            _configurationProvider = configurationProvider;
            _mapper = mapper;
        }

        public async Task<StandardResponse<NotificationModel>> SendNotification(NotificationModel notification)
        {
            try
            {
                var mappedNotification = _mapper.Map<Notification>(notification);
                var createdNotification = _notificationRepository.CreateAndReturn(mappedNotification);
                return StandardResponse<NotificationModel>.Ok();
            }
            catch (Exception ex)
            {
                return StandardResponse<NotificationModel>.Error(ex.Message);
            }
        }
        public async Task<StandardResponse<PagedCollection<NotificationView>>> ListMyNotifications(PagingOptions options)
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();

                var notifications = _notificationRepository.Query().Where(n => n.UserId == userId).OrderByDescending(n => n.DateCreated);

                var pagedNotifications = notifications.Skip(options.Offset.Value).Take(options.Limit.Value);

                var mappedNotifications = pagedNotifications.ProjectTo<NotificationView>(_configurationProvider).ToArray();

                var pagedCollection = PagedCollection<NotificationView>.Create(Link.ToCollection(nameof(NotificationController.ListMyNotifications)), mappedNotifications, notifications.Count(), options);

                return StandardResponse<PagedCollection<NotificationView>>.Ok(pagedCollection);

            }
            catch (Exception ex)
            {
                return StandardResponse<PagedCollection<NotificationView>>.Error(ex.Message);
            }
        }

        public async Task<StandardResponse<NotificationView>> GetNotification(Guid id)
        {
            try
            {
                var notification = _notificationRepository.Query().FirstOrDefault(n => n.Id == id);

                if (notification == null)
                    return StandardResponse<NotificationView>.NotFound("Notification not found");
                

                var mappedNotification = _mapper.Map<NotificationView>(notification);

                return StandardResponse<NotificationView>.Ok(mappedNotification);
            }
            catch (Exception ex)
            {
                return StandardResponse<NotificationView>.Error(ex.Message);
            }
        }

        //mark a notification as read
        public async Task<StandardResponse<NotificationView>> MarkAsRead(Guid id)
        {
            try
            {
                var notification = _notificationRepository.Query().FirstOrDefault(n => n.Id == id);

                if (notification == null)
                    return StandardResponse<NotificationView>.NotFound("Notification not found");

                notification.IsRead = true;

                _notificationRepository.Update(notification);

                var mappedNotification = _mapper.Map<NotificationView>(notification);

                return StandardResponse<NotificationView>.Ok(mappedNotification);
            }
            catch (Exception ex)
            {
                return StandardResponse<NotificationView>.Error(ex.Message);
            }
        }
    }
}