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
using TimesheetBE.Repositories;
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
        private readonly IUserRepository _userRepository;
        private readonly IContractRepository _contractRepository;
        private readonly IControlSettingRepository _controlSettingRepository;
        public NotificationService(INotificationRepository notificationRepository, IHttpContextAccessor httpContextAccessor, 
            IConfigurationProvider configurationProvider, IMapper mapper, IUserRepository userRepository, IContractRepository contractRepository, 
            IControlSettingRepository controlSettingRepository)
        {
            _notificationRepository = notificationRepository;
            _httpContextAccessor = httpContextAccessor;
            _configurationProvider = configurationProvider;
            _mapper = mapper;
            _userRepository = userRepository;
            _contractRepository = contractRepository;
            _controlSettingRepository = controlSettingRepository;
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

        public void SendBirthDayNotificationMessage()
        {
            var celebrants = _userRepository.Query().Where(x => x.DateOfBirth.Date.Day == DateTime.Now.Date.Day && 
            x.DateOfBirth.Date.Month == DateTime.Now.Date.Month && x.Role.ToLower() == "team member").ToList();

            var users = _userRepository.Query().Where(x => x.IsActive == true).ToList();

            foreach (var celebrant in celebrants)
            {
                var settings = _controlSettingRepository.Query().FirstOrDefault(x => x.SuperAdminId == celebrant.SuperAdminId);
                foreach (var user in users)
                {
                    if (celebrant.SuperAdminId == user.SuperAdminId && settings.AllowBirthdayNotification == true &&
                        settings.NotifyEveryoneAboutCelebrant == true)
                    {
                        _notificationRepository.CreateAndReturn(new Notification
                        {
                            UserId = user.Id,
                            Title = "Birthday Alert",
                            Type = "Notification",
                            Message = $"Happy birthday to our colleague {celebrant.FullName}! Let's celebrate and make their day special!"
                        });
                    }
                }
            }

        }

        public void SendWorkAnniversaryNotificationMessage()
        {
            var celebrants = _contractRepository.Query().Where(x => x.StartDate.Date.Day == DateTime.Now.Date.Day && x.StartDate.Date.Month == DateTime.Now.Date.Month 
            && x.StatusId == (int)Statuses.ACTIVE)
                .ToList();

            var users = _userRepository.Query().Where(x => x.IsActive == true).ToList();

            foreach (var celebrant in celebrants)
            {
                var clebrantDetail = _userRepository.Query().FirstOrDefault(x => x.EmployeeInformationId == celebrant.EmployeeInformationId);

                var settings = _controlSettingRepository.Query().FirstOrDefault(x => x.SuperAdminId == clebrantDetail.SuperAdminId);

                foreach (var user in users)
                {
                    if (clebrantDetail.SuperAdminId == user.SuperAdminId && settings.AllowWorkAnniversaryNotification == true && 
                        settings.NotifyEveryoneAboutCelebrant == true)
                    {
                        _notificationRepository.CreateAndReturn(new Notification
                        {
                            UserId = user.Id,
                            Title = "Work Anniversary Alert",
                            Type = "Notification",
                            Message = $"Happy Anniversary to our colleague {clebrantDetail.FullName}! Let's celebrate and make their day special!"
                        });
                    }
                }
            }

        }
    }
}