using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Repositories.Interfaces;
using TimesheetBE.Services.Interfaces;
using TimesheetBE.Utilities.Abstrctions;
using TimesheetBE.Utilities.Constants;

namespace TimesheetBE.Services
{
    public class ReminderService : IReminderService
    {
        private readonly IEmployeeInformationRepository _employeeInformationRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEmailHandler _emailHandler;


        public ReminderService(IEmployeeInformationRepository employeeInformationRepository, INotificationRepository notificationRepository, IUserRepository userRepository, IEmailHandler emailHandler)
        {
            _employeeInformationRepository = employeeInformationRepository;
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _emailHandler = emailHandler;
        }

        /// <summary>
        /// Send reminder to fill timesheet to all employees every friday
        /// </summary>

        public void SendFillTimesheetReminder()
        {
            var employeeInformations = _employeeInformationRepository.Query().Include(x => x.User).ToList();
            foreach (var employeeInformation in employeeInformations)
            {
                if (!_notificationRepository.Query().Any(x => x.UserId == employeeInformation.UserId && x.DateCreated == DateTime.Now))
                    _notificationRepository.CreateAndReturn(
                        new Notification
                        {
                            UserId = employeeInformation.UserId,
                            Title = "Reminder to fill timesheet",
                            Type = "Reminder",
                            Message = "Please fill your timesheet for this week",
                            IsRead = false
                        }
                    );

                List<KeyValuePair<string, string>> EmailParameters = new()
                {
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, employeeInformation.User.FirstName),
                };


                var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.DEACTIVATE_USER_EMAIL_FILENAME, EmailParameters);
                var SendEmail = _emailHandler.SendEmail(employeeInformation.User.Email, "Account Deactivation", EmailTemplate, "");
            }
        }


        public void SendApproveTimesheetReminder()
        {
            var users = _userRepository.Query().Where(x => x.Role.ToLower() == "Supervisor").ToList();

            foreach (var user in users)
            {
                if (!_notificationRepository.Query().Any(x => x.UserId == user.Id && x.DateCreated == DateTime.Now))
                     _notificationRepository.CreateAndReturn(
                        new Notification
                        {
                            UserId = user.Id,
                            Title = "Reminder to approve timesheet",
                            Type = "Reminder",
                            Message = "Please approve timesheet for this week",
                            IsRead = false
                        }
                    );
            }
        }

    }
}