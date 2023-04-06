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
        private readonly IPaymentScheduleRepository _paymentScheduleRepository;
        private readonly ITimeSheetRepository _timeSheetRepository;

        public ReminderService(IEmployeeInformationRepository employeeInformationRepository, INotificationRepository notificationRepository, IUserRepository userRepository, 
            IEmailHandler emailHandler, IPaymentScheduleRepository paymentScheduleRepository, ITimeSheetRepository timeSheetRepository)
        {
            _employeeInformationRepository = employeeInformationRepository;
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _emailHandler = emailHandler;
            _paymentScheduleRepository = paymentScheduleRepository;
            _timeSheetRepository = timeSheetRepository;
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

                //List<KeyValuePair<string, string>> EmailParameters = new()
                //{
                //    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, employeeInformation.User.FirstName),
                //};


                //var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.DEACTIVATE_USER_EMAIL_FILENAME, EmailParameters);
                //var SendEmail = _emailHandler.SendEmail(employeeInformation.User.Email, "Account Deactivation", EmailTemplate, "");
            }
        }

        public void SendFillTimesheetReminderToTeamMember()
        {
            var employees = _userRepository.Query().Include(x => x.EmployeeInformation).Where(x => x.EmployeeInformationId != null && x.IsActive == true && x.EmailConfirmed == true).ToList();
            var paymentScheduleForWeeklyUser = _paymentScheduleRepository.Query().Where(x => x.CycleType.ToLower() == "weekly").ToList();
            var paymentScheduleForBiWeeklyUser = _paymentScheduleRepository.Query().Where(x => x.CycleType.ToLower() == "bi-weekly").ToList();
            var paymentScheduleForMonthlyyUser = _paymentScheduleRepository.Query().Where(x => x.CycleType.ToLower() == "monthly").ToList();
            foreach (var employee in employees)
            {
                switch(employee?.EmployeeInformation?.PaymentFrequency.ToLower())
                {
                    case "weekly":
                        foreach (var paymentSchedule in paymentScheduleForWeeklyUser)
                        {
                            if (!_timeSheetRepository.Query().Any(x => x.DateModified > x.Date && paymentSchedule.WeekDate.Date <= x.Date.Date && x.Date.Date <= paymentSchedule.LastWorkDayOfCycle.Date)) continue;
                            if (!_notificationRepository.Query().Any(x => x.UserId == employee.Id && x.DateCreated == DateTime.Now))
                            {
                                if (paymentSchedule.LastWorkDayOfCycle == DateTime.Now)
                                {
                                    _notificationRepository.CreateAndReturn(
                                        new Notification
                                        {
                                            UserId = employee.Id,
                                            Title = "Reminder to fill timesheet",
                                            Type = "Reminder",
                                            Message = "You have a pending timesheet awaiting submission",
                                            IsRead = false
                                        }
                                    );

                                    List<KeyValuePair<string, string>> EmailParameters = new()
                                    {
                                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, employee.FirstName),
                                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_URL, "#"),
                                    };


                                    var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.TIMESHEET_FILLING_REMINDER_FILENAME, EmailParameters);
                                    var SendEmail = _emailHandler.SendEmail(employee.Email, "REMINDER FOR TIMESHEET FOR SUBMISSION", EmailTemplate, "");
                                }
                            }
                        }

                        break;
                    case "bi-weekly":
                        foreach (var paymentSchedule in paymentScheduleForBiWeeklyUser)
                        {
                            if (!_timeSheetRepository.Query().Any(x => x.DateModified > x.Date && paymentSchedule.WeekDate.Date <= x.Date.Date && x.Date.Date <= paymentSchedule.LastWorkDayOfCycle.Date)) continue;
                            if (!_notificationRepository.Query().Any(x => x.UserId == employee.Id && x.DateCreated == DateTime.Now))
                            {
                                if (paymentSchedule.LastWorkDayOfCycle == DateTime.Now)
                                {
                                    _notificationRepository.CreateAndReturn(
                                        new Notification
                                        {
                                            UserId = employee.Id,
                                            Title = "Reminder to fill timesheet",
                                            Type = "Reminder",
                                            Message = "You have a pending timesheet awaiting submission",
                                            IsRead = false
                                        }
                                    );

                                    List<KeyValuePair<string, string>> EmailParameters = new()
                                    {
                                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, employee.FirstName),
                                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_URL, "#"),
                                    };


                                    var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.TIMESHEET_FILLING_REMINDER_FILENAME, EmailParameters);
                                    var SendEmail = _emailHandler.SendEmail(employee.Email, "Reminder for TIMESHEET FOR SUBMISSION", EmailTemplate, "");
                                }
                            }
                        }

                        break;

                    case "monthly":
                        foreach (var paymentSchedule in paymentScheduleForMonthlyyUser)
                        {
                            if (!_timeSheetRepository.Query().Any(x => x.DateModified > x.Date && paymentSchedule.WeekDate.Date <= x.Date.Date && x.Date.Date <= paymentSchedule.LastWorkDayOfCycle.Date)) continue;
                            if (!_notificationRepository.Query().Any(x => x.UserId == employee.Id && x.DateCreated == DateTime.Now))
                            {
                                if (paymentSchedule.LastWorkDayOfCycle == DateTime.Now)
                                {
                                    _notificationRepository.CreateAndReturn(
                                        new Notification
                                        {
                                            UserId = employee.Id,
                                            Title = "Reminder to fill timesheet",
                                            Type = "Reminder",
                                            Message = "You have a pending timesheet awaiting submission",
                                            IsRead = false
                                        }
                                    );

                                    List<KeyValuePair<string, string>> EmailParameters = new()
                                    {
                                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, employee.FirstName),
                                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_URL, "#"),
                                    };


                                    var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.TIMESHEET_FILLING_REMINDER_FILENAME, EmailParameters);
                                    var SendEmail = _emailHandler.SendEmail(employee.Email, "Reminder for TIMESHEET FOR SUBMISSION", EmailTemplate, "");
                                }
                            }
                        }

                        break;
                }
                        

                    
            }
        }

        public void SendCutOffTimesheetReminderToTeamMember()
        {
            var employees = _userRepository.Query().Include(x => x.EmployeeInformation).Where(x => x.EmployeeInformationId != null && x.IsActive == true).ToList();
            var paymentScheduleForWeeklyUser = _paymentScheduleRepository.Query().Where(x => x.CycleType.ToLower() == "weekly").ToList();
            var paymentScheduleForBiWeeklyUser = _paymentScheduleRepository.Query().Where(x => x.CycleType.ToLower() == "bi-weekly").ToList();
            var paymentScheduleForMonthlyyUser = _paymentScheduleRepository.Query().Where(x => x.CycleType.ToLower() == "monthly").ToList();
            foreach (var employee in employees)
            {
                switch (employee?.EmployeeInformation?.PaymentFrequency.ToLower())
                {
                    case "weekly":
                        foreach (var paymentSchedule in paymentScheduleForWeeklyUser)
                        {
                            if (!_notificationRepository.Query().Any(x => x.UserId == employee.Id && x.DateCreated == DateTime.Now))
                            {
                                if (paymentSchedule.WeekDate == DateTime.Now)
                                {
                                    _notificationRepository.CreateAndReturn(
                                        new Notification
                                        {
                                            UserId = employee.Id,
                                            Title = "Reminder to fill timesheet",
                                            Type = "Reminder",
                                            Message = "You have a pending timesheet awaiting submission",
                                            IsRead = false
                                        }
                                    );

                                    List<KeyValuePair<string, string>> EmailParameters = new()
                                    {
                                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, employee.FirstName),
                                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_URL, "#"),
                                    };


                                    var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.TIMESHEET_CUTOFF_REMINDER_FILENAME, EmailParameters);
                                    var SendEmail = _emailHandler.SendEmail(employee.Email, "FINAL TIMESHEET REMINDER FOR SUBMISSION", EmailTemplate, "");
                                }
                            }
                        }

                        break;
                    case "bi-weekly":
                        foreach (var paymentSchedule in paymentScheduleForBiWeeklyUser)
                        {
                            if (!_notificationRepository.Query().Any(x => x.UserId == employee.Id && x.DateCreated == DateTime.Now))
                            {
                                if (paymentSchedule.WeekDate == DateTime.Now)
                                {
                                    _notificationRepository.CreateAndReturn(
                                        new Notification
                                        {
                                            UserId = employee.Id,
                                            Title = "Reminder to fill timesheet",
                                            Type = "Reminder",
                                            Message = "You have a pending timesheet awaiting submission",
                                            IsRead = false
                                        }
                                    );

                                    List<KeyValuePair<string, string>> EmailParameters = new()
                                    {
                                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, employee.FirstName),
                                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_URL, "#"),
                                    };


                                    var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.TIMESHEET_CUTOFF_REMINDER_FILENAME, EmailParameters);
                                    var SendEmail = _emailHandler.SendEmail(employee.Email, "FINAL TIMESHEET REMINDER FOR SUBMISSION", EmailTemplate, "");
                                }
                            }
                        }

                        break;

                    case "monthly":
                        foreach (var paymentSchedule in paymentScheduleForMonthlyyUser)
                        {
                            if (!_notificationRepository.Query().Any(x => x.UserId == employee.Id && x.DateCreated == DateTime.Now))
                            {
                                if (paymentSchedule.WeekDate == DateTime.Now)
                                {
                                    _notificationRepository.CreateAndReturn(
                                        new Notification
                                        {
                                            UserId = employee.Id,
                                            Title = "Reminder to fill timesheet",
                                            Type = "Reminder",
                                            Message = "You have a pending timesheet awaiting submission",
                                            IsRead = false
                                        }
                                    );

                                    List<KeyValuePair<string, string>> EmailParameters = new()
                                    {
                                        new KeyValuePair<string, string>(Constants.TIMESHEET_CUTOFF_REMINDER_FILENAME, employee.FirstName),
                                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_URL, "#"),
                                    };


                                    var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.TIMESHEET_FILLING_REMINDER_FILENAME, EmailParameters);
                                    var SendEmail = _emailHandler.SendEmail(employee.Email, "FINAL TIMESHEET REMINDER FOR SUBMISSION ", EmailTemplate, "");
                                }
                            }
                        }

                        break;
                }



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