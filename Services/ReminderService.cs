using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Models.IdentityModels;
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
        private readonly IProjectManagementService _projectManagementService;
        private readonly IProjectTaskRepository _projectTaskRepository;
        private readonly IProjectSubTaskRepository _projectSubTaskRepository;
        private readonly IControlSettingRepository _controlSettingRepository;
        private readonly IProjectTimesheetRepository _projectTimesheetRepository;


        public ReminderService(IEmployeeInformationRepository employeeInformationRepository, INotificationRepository notificationRepository, IUserRepository userRepository, 
            IEmailHandler emailHandler, IPaymentScheduleRepository paymentScheduleRepository, ITimeSheetRepository timeSheetRepository, IProjectManagementService projectManagementService,
            IProjectTaskRepository projectTaskRepository, IProjectSubTaskRepository projectSubTaskRepository, IControlSettingRepository controlSettingRepository, IProjectTimesheetRepository projectTimesheetRepository)
        {
            _employeeInformationRepository = employeeInformationRepository;
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _emailHandler = emailHandler;
            _paymentScheduleRepository = paymentScheduleRepository;
            _timeSheetRepository = timeSheetRepository;
            _projectManagementService = projectManagementService;
            _projectTaskRepository = projectTaskRepository;
            _projectSubTaskRepository = projectSubTaskRepository;
            _controlSettingRepository = controlSettingRepository;
            _projectTimesheetRepository = projectTimesheetRepository;
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

        public void SendOverdueTaskReminder()
        {
            var tasks = _projectTaskRepository.Query().Include(x => x.Assignees).ThenInclude(x => x.User).ToList();
            foreach (var task in tasks)
            {
                if (task.Assignees.Any())
                {
                    foreach (var assignee in task.Assignees)
                    {
                        List<KeyValuePair<string, string>> EmailParams = new()
                        {
                            new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, assignee.User.FullName),
                            new KeyValuePair<string, string>(Constants.TASK_OVERDUE_REPLACEMENT_DATE, task.EndDate.Date.ToString()),
                        };

                        var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.TASK_OVERDUE_FILENAME, EmailParams);
                        var SendEmail = _emailHandler.SendEmail(assignee.User.Email, "Overdue Task Notification", EmailTemplate, "");
                    }
                }
                
            }
        }

        public void SendOverdueSubTaskReminder()
        {
            var tasks = _projectSubTaskRepository.Query().Include(x => x.ProjectTaskAsignee).ThenInclude(x => x.User).ToList();
            foreach (var task in tasks)
            {
                List<KeyValuePair<string, string>> EmailParams = new()
                {
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, task.ProjectTaskAsignee.User.FullName),
                    new KeyValuePair<string, string>(Constants.TASK_OVERDUE_REPLACEMENT_DATE, task.EndDate.Date.ToString()),
                };

                var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.TASK_OVERDUE_FILENAME, EmailParams);
                var SendEmail = _emailHandler.SendEmail(task.ProjectTaskAsignee.User.Email, "Overdue Task Notification", EmailTemplate, "");
     
            }
        }

        public void SendProjectTimesheetReminder()
        {
            var superAdmins = _userRepository.Query().Where(x => x.Role.ToLower() == "super admin" && x.Email == "ade.adeyemi@oladeconsulting.com").ToList();
            //var timeSheets = _projectTimesheetRepository.Query().ToList(); 
            foreach (var superAdmin in superAdmins)
            {
                var setting = _controlSettingRepository.Query().FirstOrDefault(x => x.SuperAdminId == superAdmin.Id);
                var teammembers = _userRepository.Query().Include(x => x.EmployeeInformation).Where(x => x.SuperAdminId == superAdmin.Id && x.Role.ToLower() == "team member").ToList();

                if(setting.TimesheetFillingReminderDay == null && DateTime.Now.DayOfWeek == DayOfWeek.Friday)
                {
                    foreach (var teammember in teammembers)
                    {
                        var lastReminder = _notificationRepository.Query().OrderBy(x => x.DateCreated).LastOrDefault(x => x.Type.ToLower() == "timesheet reminder" && x.UserId == teammember.Id);
                        if(teammember.EmployeeInformation.PaymentFrequency.ToLower() == "weekly")
                        {
                            if(lastReminder == null)
                            {
                                SendTimesheetDueDateReminder(teammember);
                            }
                            if(lastReminder != null && lastReminder.DateCreated.AddDays(7).Date == DateTime.Now.Date)
                            {
                                SendTimesheetDueDateReminder(teammember);
                            }
                            
                        }
                        else if(teammember.EmployeeInformation.PaymentFrequency.ToLower() == "bi-weekly")
                        {
                            if(lastReminder == null)
                            {
                                SendTimesheetDueDateReminder(teammember);
                            }
                            if (lastReminder != null && lastReminder.DateCreated.AddDays(14).Date == DateTime.Now.Date)
                            {
                                SendTimesheetDueDateReminder(teammember);
                            }
                        }
                        else if (teammember.EmployeeInformation.PaymentFrequency.ToLower() == "monthly")
                        {
                            if (lastReminder == null)
                            {
                                SendTimesheetDueDateReminder(teammember);
                            }
                            if (lastReminder != null && lastReminder.DateCreated.AddDays(28).Date == DateTime.Now.Date)
                            {
                                SendTimesheetDueDateReminder(teammember);
                            }
                        }

                    }
                }

                if(setting.TimesheetFillingReminderDay != null &&(DayOfWeek)setting.TimesheetFillingReminderDay == DateTime.Now.DayOfWeek)
                {
                    foreach (var teammember in teammembers)
                    {
                        var lastReminder = _notificationRepository.Query().OrderBy(x => x.DateCreated).LastOrDefault(x => x.Type.ToLower() == "timesheet reminder" && x.UserId == teammember.Id);
                        if (teammember.EmployeeInformation.PaymentFrequency.ToLower() == "weekly")
                        {
                            if (lastReminder == null)
                            {
                                SendTimesheetDueDateReminder(teammember);
                            }
                            if (lastReminder != null && lastReminder.DateCreated.AddDays(7).Date == DateTime.Now.Date)
                            {
                                SendTimesheetDueDateReminder(teammember);
                            }

                        }
                        else if (teammember.EmployeeInformation.PaymentFrequency.ToLower() == "bi-weekly")
                        {
                            if (lastReminder == null)
                            {
                                SendTimesheetDueDateReminder(teammember);
                            }
                            if (lastReminder != null && lastReminder.DateCreated.AddDays(14).Date == DateTime.Now.Date)
                            {
                                SendTimesheetDueDateReminder(teammember);
                            }
                        }
                        else if (teammember.EmployeeInformation.PaymentFrequency.ToLower() == "monthly")
                        {
                            if (lastReminder == null)
                            {
                                SendTimesheetDueDateReminder(teammember);
                            }
                            if (lastReminder != null && lastReminder.DateCreated.AddDays(28).Date == DateTime.Now.Date)
                            {
                                SendTimesheetDueDateReminder(teammember);
                            }
                        }
                    }
                }
            }
        }

        public void SendProjectTimesheetOverdueReminder()
        {
            var superAdmins = _userRepository.Query().Where(x => x.Role.ToLower() == "super admin").ToList();
            var timeSheets = _projectTimesheetRepository.Query().ToList();
            foreach (var superAdmin in superAdmins)
            {
                var setting = _controlSettingRepository.Query().FirstOrDefault(x => x.SuperAdminId == superAdmin.Id);
                var teammembers = _userRepository.Query().Where(x => x.SuperAdminId == superAdmin.Id).ToList();

                var overDueReminderDay = setting.TimesheetOverdueReminderDay.HasValue ? setting.TimesheetOverdueReminderDay.Value : 2;

                foreach (var teammember in teammembers)
                {
                    var lastReminder = _notificationRepository.Query().OrderBy(x => x.DateCreated).LastOrDefault(x => x.Type.ToLower() == "timesheet reminder" && x.UserId == teammember.Id);
                    if (teammember.EmployeeInformation.PaymentFrequency.ToLower() == "weekly")
                    {
                        if (lastReminder != null && AddBusinessDays(lastReminder.DateCreated.Date, overDueReminderDay) == DateTime.Now.Date)
                        {
                            SendTimesheetOverdueDateReminder(teammember);
                        }

                    }
                    else if (teammember.EmployeeInformation.PaymentFrequency.ToLower() == "bi-weekly")
                    {

                        if (lastReminder != null && AddBusinessDays(lastReminder.DateCreated.Date, overDueReminderDay) == DateTime.Now.Date)
                        {
                            SendTimesheetOverdueDateReminder(teammember);
                        }
                    }
                    else if (teammember.EmployeeInformation.PaymentFrequency.ToLower() == "monthly")
                    {
                        if (lastReminder != null && AddBusinessDays(lastReminder.DateCreated.Date, overDueReminderDay) == DateTime.Now.Date)
                        {
                            SendTimesheetOverdueDateReminder(teammember);
                        }
                    }
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

        private void SendTimesheetDueDateReminder(User user)
        {
            _notificationRepository.CreateAndReturn(
                                      new Notification
                                      {
                                          UserId = user.Id,
                                          Title = "Reminder to fill timesheet",
                                          Type = "Timesheet Reminder",
                                          Message = "This is a reminder to fill in timesheet",
                                          IsRead = false
                                      }
                                  );

            List<KeyValuePair<string, string>> EmailParameters = new()
                                    {
                                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, user.FirstName),
                                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_URL, "#"),
                                    };


            var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.TIMESHEET_FILLING_REMINDER_FILENAME, EmailParameters);
            var SendEmail = _emailHandler.SendEmail(user.Email, "Reminder FOR TIMESHEET FILLING", EmailTemplate, "");
        }

        private void SendTimesheetOverdueDateReminder(User user)
        {
            _notificationRepository.CreateAndReturn(
                                       new Notification
                                       {
                                           UserId = user.Id,
                                           Title = "Timesheet Overdue",
                                           Type = "Reminder",
                                           Message = "Your timesheet is overdue, Kindly fill it",
                                           IsRead = false
                                       }
                                   );

            List<KeyValuePair<string, string>> EmailParameters = new()
                                    {
                                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, user.FirstName),
                                        new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_URL, "#"),
                                    };


            var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.TIMESHEET_FILLING_REMINDER_FILENAME, EmailParameters);
            var SendEmail = _emailHandler.SendEmail(user.Email, "Reminder FOR TIMESHEET OVERDUE", EmailTemplate, "");
        }

        private DateTime AddBusinessDays(DateTime date, int days)
        {
            if (date.DayOfWeek == DayOfWeek.Saturday)
            {
                date = date.AddDays(2);
                days -= 1;
            }
            else if (date.DayOfWeek == DayOfWeek.Sunday)
            {
                date = date.AddDays(1);
                days -= 1;
            }

            date = date.AddDays(days / 5 * 7);
            int extraDays = days % 5;

            if ((int)date.DayOfWeek + extraDays > 5)
            {
                extraDays += 2;
            }

            return date.AddDays(extraDays);
        }

    }
}