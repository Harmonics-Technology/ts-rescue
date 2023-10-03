using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TimesheetBE.Controllers;
using TimesheetBE.Models;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Models.IdentityModels;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Repositories;
using TimesheetBE.Repositories.Interfaces;
using TimesheetBE.Services.Interfaces;
using TimesheetBE.Utilities;
using TimesheetBE.Utilities.Abstrctions;
using TimesheetBE.Utilities.Constants;
using TimesheetBE.Utilities.Extentions;

namespace TimesheetBE.Services
{
    public class TimeSheetService : ITimeSheetService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITimeSheetRepository _timeSheetRepository;
        private readonly IMapper _mapper;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly ICustomLogger<TimeSheetService> _logger;
        private readonly IEmployeeInformationRepository _employeeInformationRepository;
        private readonly IPayrollRepository _payrollRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPaymentScheduleRepository _paymentScheduleRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IUtilityMethods _utilityMethods;
        private readonly IEmailHandler _emailHandler;
        private readonly INotificationService _notificationService;
        private readonly ILeaveService _leaveService;
        private readonly ILeaveRepository _leaveRepository;
        private readonly IDataExport _dataExport;

        public TimeSheetService(IUserRepository userRepository, ITimeSheetRepository timeSheetRepository, IMapper mapper, IConfigurationProvider configurationProvider, IEmployeeInformationRepository employeeInformationRepository, ICustomLogger<TimeSheetService> logger, 
            IPayrollRepository payrollRepository, IHttpContextAccessor httpContextAccessor, 
            IPaymentScheduleRepository paymentScheduleRepository, IInvoiceRepository invoiceRepository, IUtilityMethods utilityMethods, IEmailHandler emailHandler, INotificationService notificationService, ILeaveService leaveService,
            ILeaveRepository leaveRepository, IDataExport dataExport)
        {
            _userRepository = userRepository;
            _timeSheetRepository = timeSheetRepository;
            _mapper = mapper;
            _configurationProvider = configurationProvider;
            _employeeInformationRepository = employeeInformationRepository;
            _logger = logger;
            _payrollRepository = payrollRepository;
            _httpContextAccessor = httpContextAccessor;
            _paymentScheduleRepository = paymentScheduleRepository;
            _invoiceRepository = invoiceRepository;
            _utilityMethods = utilityMethods;
            _emailHandler = emailHandler;
            _notificationService = notificationService;
            _leaveService = leaveService;
            _leaveRepository = leaveRepository;
            _dataExport = dataExport;
        }


        
        /// <summary>
        /// Get a paginated collection of timesheet history for all user 
        /// </summary>
        /// <param name="pagingOptions">The page number</param>
        /// <returns></returns>
        public async Task<StandardResponse<PagedCollection<TimeSheetHistoryView>>> ListTimeSheetHistories(PagingOptions pagingOptions, Guid superAdminId, string search = null, DateFilter dateFilter = null)
        {
            try
            {
                var loggedInUserRole = _httpContextAccessor.HttpContext.User.GetLoggedInUserRole();

                var allUsers = _userRepository.Query().Include(u => u.EmployeeInformation).Where(user => (user.Role.ToLower() == "team member" || user.Role.ToLower() == "internal supervisor" || user.Role.ToLower() == "internal admin") && user.SuperAdminId == superAdminId);

                if (!string.IsNullOrEmpty(search))
                {
                    allUsers = allUsers.Where(user => user.FirstName.ToLower().Contains(search.ToLower()) || user.LastName.ToLower().Contains(search.ToLower())
                    || (user.FirstName.ToLower() + " " + user.LastName.ToLower()).Contains(search.ToLower()));
                }

                //if (loggedInUserRole == "Super Admin") pagingOptions.Limit = allUsers.Count();

                var pageUsers = allUsers.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value).ToList().AsQueryable();

                var allTimeSheetHistory = new List<TimeSheetHistoryView>();

                foreach (var user in pageUsers)
                {
                    if (user.IsActive == false) continue;
                    var timeSheetHistory = GetTimeSheetHistory(user, dateFilter);

                    if (timeSheetHistory == null) continue;

                    allTimeSheetHistory.Add(timeSheetHistory);
                }

                var timeSheetHistories = allTimeSheetHistory.OrderByDescending(x => x.DateModified); 

                var pagedCollection = PagedCollection<TimeSheetHistoryView>.Create(Link.ToCollection(nameof(TimeSheetController.ListTimeSheetHistories)), timeSheetHistories.ToArray(), allUsers.Count(), pagingOptions);
                return StandardResponse<PagedCollection<TimeSheetHistoryView>>.Ok(pagedCollection);

            }
            catch (Exception ex)
            {
                return _logger.Error<PagedCollection<TimeSheetHistoryView>>(_logger.GetMethodName(), ex);
            }
        }

        /// <summary>
        /// Get timesheet fro a particular user for a month.
        /// </summary>
        /// <param name="employeeInformationId">The employee information id</param>
        /// <param name="date">The date with the month and year fro the record needed</param>
        /// <returns></returns>
        public async Task<StandardResponse<TimeSheetMonthlyView>> GetTimeSheet(Guid employeeInformationId, DateTime date, DateTime? endDate)
        {
            try
            {
                var timeSheet = _timeSheetRepository.Query()
                .Where(timeSheet => timeSheet.EmployeeInformationId == employeeInformationId && timeSheet.Date.Month == date.Month && timeSheet.Date.Year == date.Year);

                if (endDate.HasValue)
                {
                    timeSheet = timeSheet
                      .Where(timeSheet => timeSheet.EmployeeInformationId == employeeInformationId && timeSheet.Date.Date >= date.Date && timeSheet.Date.Date <= endDate.Value.Date);
                }

                var firstDay = new DateTime(date.Year, date.Month, 1);
                var lastDay = endDate.HasValue ? endDate.Value : firstDay.Date.AddMonths(1).AddDays(-1);

                var totalHoursWorked = timeSheet
                .Where(timeSheet => timeSheet.EmployeeInformationId == employeeInformationId && timeSheet.Date.Date >= date.Date && timeSheet.Date.Date <= lastDay.Date)
                .AsQueryable().Sum(timeSheet => timeSheet.Hours);

                var totalApprovedHours = _timeSheetRepository.Query()
                .Where(timeSheet => timeSheet.EmployeeInformationId == employeeInformationId && timeSheet.IsApproved == true && timeSheet.Date.Date >= date.Date && timeSheet.Date.Date <= lastDay.Date)
                .AsQueryable().Sum(timeSheet => timeSheet.Hours);

                //if (timeSheet.Count() == 0)
                //    return StandardResponse<TimeSheetMonthlyView>.NotFound("No time sheet found for this user for the date requested");

                var expectedEarnings = GetExpectedWorkHoursAndPay(employeeInformationId, date);

                var employeeInformation = _userRepository.Query().Include(u => u.EmployeeInformation).FirstOrDefault(user => user.EmployeeInformationId == employeeInformationId);

                var timeSheetView = timeSheet.ProjectTo<TimeSheetView>(_configurationProvider).ToList();
                var startDate = new DateTime(date.Year, date.Month, 1);
                var endingDate = endDate.HasValue ? endDate.Value : startDate.AddMonths(1).AddSeconds(-1);
                var timeSheetMonthlyView = new TimeSheetMonthlyView
                {
                    TimeSheet = timeSheetView,
                    ExpectedPay = expectedEarnings.ExpectedPay,
                    ExpectedWorkHours = expectedEarnings.ExpectedWorkHours,
                    TotalHoursWorked = totalHoursWorked,
                    TotalApprovedHours = totalApprovedHours,
                    FullName = employeeInformation.FullName,
                    Currency = employeeInformation.EmployeeInformation.Currency,
                    StartDate = startDate,
                    EndDate = endingDate
                };

                return StandardResponse<TimeSheetMonthlyView>.Ok(timeSheetMonthlyView);
            }
            catch (Exception ex)
            {
                return _logger.Error<TimeSheetMonthlyView>(_logger.GetMethodName(), ex);
            }
        }

        /// <summary>
        /// Get timesheet by pay schedule
        /// </summary>
        /// <param name="employeeInformationId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public async Task<StandardResponse<TimeSheetMonthlyView>> GetTimesheetByPaySchedule(Guid employeeInformationId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var timeSheet = _timeSheetRepository.Query()
                .Where(timeSheet => timeSheet.EmployeeInformationId == employeeInformationId && timeSheet.Date.Date >= startDate.Date && timeSheet.Date.Date <= endDate.Date);

                var totalHoursWorked = _timeSheetRepository.Query()
                .Where(timeSheet => timeSheet.EmployeeInformationId == employeeInformationId && timeSheet.Date.Date >= startDate.Date && timeSheet.Date.Date <= endDate.Date)
                .AsQueryable().Sum(timeSheet => timeSheet.Hours);

                var totalApprovedHours = _timeSheetRepository.Query()
                .Where(timeSheet => timeSheet.EmployeeInformationId == employeeInformationId && timeSheet.IsApproved == true && timeSheet.Date.Date >= startDate.Date && timeSheet.Date.Date <= endDate.Date)
                .AsQueryable().Sum(timeSheet => timeSheet.Hours);

                if (timeSheet.Count() == 0)
                    return StandardResponse<TimeSheetMonthlyView>.NotFound("No time sheet found for this user for the date requested");

                var expectedEarnings = GetExpectedWorkHoursAndPay(employeeInformationId, startDate, endDate);

                var employeeInformation = _userRepository.Query().Include(u => u.EmployeeInformation).FirstOrDefault(user => user.EmployeeInformationId == employeeInformationId);

                var timeSheetView = timeSheet.ProjectTo<TimeSheetView>(_configurationProvider).ToList();

                var timeSheetMonthlyView = new TimeSheetMonthlyView
                {
                    TimeSheet = timeSheetView,
                    ExpectedPay = expectedEarnings.ExpectedPay,
                    ExpectedWorkHours = expectedEarnings.ExpectedWorkHours,
                    TotalHoursWorked = totalHoursWorked,
                    TotalApprovedHours = totalApprovedHours,
                    FullName = employeeInformation.FullName,
                    Currency = employeeInformation.EmployeeInformation.Currency,
                    StartDate = startDate,
                    EndDate = endDate
                };

                return StandardResponse<TimeSheetMonthlyView>.Ok(timeSheetMonthlyView);


            }
            catch(Exception ex)
            {
                return _logger.Error<TimeSheetMonthlyView>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<TimeSheetMonthlyView>> GetTimeSheet2(Guid employeeInformationId, DateTime date)
        {
            try
            {
                var employee = _employeeInformationRepository.Query().FirstOrDefault(x => x.Id == employeeInformationId);
                var timesheetForTheDate = _timeSheetRepository.Query().FirstOrDefault(x => x.Date.Date == date.Date);

                if (timesheetForTheDate == null)
                    return StandardResponse<TimeSheetMonthlyView>.NotFound("No time sheet found for this user for the date requested");

                var period = _paymentScheduleRepository.Query().FirstOrDefault(x => date.Date.Date >= x.WeekDate.Date && date.Date <= x.LastWorkDayOfCycle.Date && timesheetForTheDate.Date.Date >= x.WeekDate.Date && x.CycleType.ToLower() == employee.PaymentFrequency.ToLower());
                var timeSheet = _timeSheetRepository.Query()
                .Where(timeSheet => timeSheet.EmployeeInformationId == employeeInformationId && timeSheet.Date.Date >= period.WeekDate.Date  && timeSheet.Date.Date <= period.LastWorkDayOfCycle.Date.Date);

                var totalHoursWorked = _timeSheetRepository.Query()
                .Where(timeSheet => timeSheet.EmployeeInformationId == employeeInformationId && timeSheet.Date.Date >= period.WeekDate.Date && timeSheet.Date.Date <= period.LastWorkDayOfCycle.Date.Date)
                .AsQueryable().Sum(timeSheet => timeSheet.Hours);

                var totalApprovedHours = _timeSheetRepository.Query()
                .Where(timeSheet => timeSheet.EmployeeInformationId == employeeInformationId && timeSheet.IsApproved == true && timeSheet.Date.Date >= period.WeekDate.Date && timeSheet.Date.Date <= period.LastWorkDayOfCycle.Date.Date)
                .AsQueryable().Sum(timeSheet => timeSheet.Hours);

                if (timeSheet.Count() == 0)
                    return StandardResponse<TimeSheetMonthlyView>.NotFound("No time sheet found for this user for the date requested");

                var expectedEarnings = GetExpectedWorkHoursAndPay2(employeeInformationId, period.WeekDate, period.LastWorkDayOfCycle);

                var employeeInformation = _userRepository.Query().Include(u => u.EmployeeInformation).FirstOrDefault(user => user.EmployeeInformationId == employeeInformationId);

                var timeSheetView = timeSheet.ProjectTo<TimeSheetView>(_configurationProvider).ToList();
                //var startDate = new DateTime(date.Year, date.Month, 1);
                //var endDate = startDate.AddMonths(1).AddSeconds(-1);

                var timeSheetMonthlyView = new TimeSheetMonthlyView
                {
                    TimeSheet = timeSheetView,
                    ExpectedPay = expectedEarnings.ExpectedPay,
                    ExpectedWorkHours = expectedEarnings.ExpectedWorkHours,
                    TotalHoursWorked = totalHoursWorked,
                    TotalApprovedHours = totalApprovedHours,
                    FullName = employeeInformation.FullName,
                    Currency = employeeInformation.EmployeeInformation.Currency,
                    StartDate = period.WeekDate,
                    EndDate = period.LastWorkDayOfCycle,
                };

                return StandardResponse<TimeSheetMonthlyView>.Ok(timeSheetMonthlyView);
            }
            catch (Exception ex)
            {
                return _logger.Error<TimeSheetMonthlyView>(_logger.GetMethodName(), ex);
            }
        }

        /// <summary>
        /// Approve timesheet for a particular user for a month.
        /// </summary>
        /// <param name="employeeInformationId">The employee information id</param>
        /// <param name="date">The date with the month and year fro the record needed</param>
        /// <returns></returns>
        public async Task<StandardResponse<bool>> ApproveTimeSheetForAWholeMonth(Guid employeeInformationId, DateTime date)
        {
            try
            {
                var employee = _employeeInformationRepository.Query().Include(x => x.User).FirstOrDefault(x => x.Id == employeeInformationId);
                var timeSheet = _timeSheetRepository.Query().Include(x => x.EmployeeInformation).ThenInclude(x => x.User)
                .Where(timeSheet => timeSheet.EmployeeInformationId == employeeInformationId && timeSheet.Date.Month == date.Month && timeSheet.Date.Year == date.Year);

                if (timeSheet.Count() == 0)
                    return StandardResponse<bool>.NotFound("No time sheet found for this user for the date requested");

                foreach (var timeSheetRecord in timeSheet)
                {
                    timeSheetRecord.IsApproved = true;
                    timeSheetRecord.StatusId = (int)Statuses.APPROVED;
                    timeSheetRecord.DateModified = DateTime.Now;
                    timeSheetRecord.EmployeeInformation.User.DateModified = DateTime.Now;

                    _timeSheetRepository.Update(timeSheetRecord);
                }

                await _notificationService.SendNotification(new NotificationModel { UserId = employee.UserId, Title = "Timesheet Approved", Type = "Notification", Message = $"Your timesheet for {date.Month.ToString("MMMM")} has been approved" });

                List<KeyValuePair<string, string>> EmailParameters = new()
                {
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, employee.User.FirstName),
                };

                var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.TIMESHEET_APPROVAL_EMAIL_FILENAME, EmailParameters);
                var SendEmail = _emailHandler.SendEmail(employee.User.Email, "YOUR TIMESHEET HAS BEEN APPROVED", EmailTemplate, "");

                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                return _logger.Error<bool>(_logger.GetMethodName(), ex);
            }
        }

        /// <summary>
        /// Approve timesheet for a particular user for a day.
        /// </summary>
        /// <param name="employeeInformationId">The employee information id</param>
        /// <param name="date">The date with the month and year fro the record needed</param>
        /// <returns></returns>
        public async Task<StandardResponse<bool>> ApproveTimeSheetForADay(List<TimesheetHoursApprovalModel> model, Guid employeeInformationId, DateTime date)
        {
            try
            {
                foreach(var record in model)
                {
                    var timeSheet = _timeSheetRepository.Query().Include(x => x.EmployeeInformation).ThenInclude(x => x.User)
                        .FirstOrDefault(timeSheet => timeSheet.EmployeeInformationId == employeeInformationId && timeSheet.Date.Day == record.Date.Day && timeSheet.Date.Month == record.Date.Month && timeSheet.Date.Year == record.Date.Year);

                    if (timeSheet == null)
                        return StandardResponse<bool>.NotFound("No time sheet found for this user for the date requested");

                    timeSheet.IsApproved = true;
                    timeSheet.StatusId = (int)Statuses.APPROVED;
                    timeSheet.DateModified = DateTime.Now;
                    timeSheet.EmployeeInformation.User.DateModified = DateTime.Now;
                    _timeSheetRepository.Update(timeSheet);
                }

                var timeSheetLink = $"{Globals.FrontEndBaseUrl}TeamMember/timesheets/{employeeInformationId}?date={date.ToString("yyyy-MM-dd")}";
                var employee = _employeeInformationRepository.Query().Include(x => x.User).FirstOrDefault(x => x.Id == employeeInformationId);

                await _notificationService.SendNotification(new NotificationModel { UserId = employee.UserId, Title = "Timesheet Approved", Type = "Notification", Message = $"Your timesheet has been approved" });

                List<KeyValuePair<string, string>> EmailParameters = new()
                {
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, employee.User.FirstName),
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_URL, timeSheetLink)
                };

                var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.TIMESHEET_APPROVAL_EMAIL_FILENAME, EmailParameters);
                var SendEmail = _emailHandler.SendEmail(employee.User.Email, "YOUR TIMESHEET HAS BEEN APPROVED", EmailTemplate, "");


                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                return _logger.Error<bool>(_logger.GetMethodName(), ex);
            }
        }

        //public async Task<StandardResponse<bool>> ApproveTimeSheetForCycle(Guid employeeInformationId, DateTime date)
        //{
        //    try
        //    {

        //        var timeSheet = _timeSheetRepository.Query()
        //        .FirstOrDefault(timeSheet => timeSheet.EmployeeInformationId == employeeInformationId && timeSheet.Date.Day == date.Day && timeSheet.Date.Month == date.Month && timeSheet.Date.Year == date.Year);

        //        var currentCycle = GetLastCycle(employeeInformationId);

        //        //var currentCycle = _paymentScheduleRepository.Query().Where(x => date >= x.WeekDate && date <= x.LastWorkDayOfCycle.AddDays(3)).FirstOrDefault();

        //        if (currentCycle.LastWorkDayOfCycle.AddDays(3) < DateTime.Now || DateTime.Now > currentCycle.LastWorkDayOfCycle.AddDays(4))
        //            return StandardResponse<bool>.NotFound("You cannot approve this timesheet again");


        //        timeSheet.IsApproved = true;
        //        timeSheet.StatusId = (int)Statuses.APPROVED;
        //        _timeSheetRepository.Update(timeSheet);


        //        return StandardResponse<bool>.Ok(true);
        //    }
        //    catch (Exception ex)
        //    {
        //        return _logger.Error<bool>(_logger.GetMethodName(), ex);
        //    }
        //}

        /// <summary>
        /// reject timesheet for a particular user for a day.
        /// </summary>
        /// <param name="employeeInformationId">The employee information id</param>
        /// <param name="date">The date with the month and year fro the record needed</param>
        /// <returns></returns>
        public async Task<StandardResponse<bool>> RejectTimeSheetForADay(RejectTimesheetModel model, Guid employeeInformationId, DateTime date)
        {
            try
            {
                foreach(var record in model.timeSheets)
                {
                    var timeSheet = _timeSheetRepository.Query().Include(x => x.EmployeeInformation).ThenInclude(x => x.User)
                       .FirstOrDefault(timeSheet => timeSheet.EmployeeInformationId == record.EmployeeInformationId && timeSheet.Date.Day == record.Date.Day && timeSheet.Date.Month == record.Date.Month && timeSheet.Date.Year == record.Date.Year);

                    if (timeSheet == null)
                        return StandardResponse<bool>.NotFound("No time sheet found for this user for the date requested");

                    timeSheet.IsApproved = false;
                    timeSheet.StatusId = (int)Statuses.REJECTED;
                    timeSheet.RejectionReason = model.Reason;
                    timeSheet.DateModified = DateTime.Now;
                    timeSheet.EmployeeInformation.User.DateModified = DateTime.Now;
                    _timeSheetRepository.Update(timeSheet);
                }

                var employee = _employeeInformationRepository.Query().Include(x => x.User).FirstOrDefault(x => x.Id == employeeInformationId);

                await _notificationService.SendNotification(new NotificationModel { UserId = employee.UserId, Title = "Timesheet Rejected", Type = "Notification", Message = $"Your timesheet(s) was rejected" });

                var timeSheetLink = $"{Globals.FrontEndBaseUrl}TeamMember/timesheets/{employeeInformationId}?date={date.ToString("yyyy-MM-dd")}";

                List<KeyValuePair<string, string>> EmailParameters = new()
                {
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, employee.User.FirstName),
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_COMMENT, model.Reason),
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_URL, timeSheetLink)
                };

                var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.TIMESHEET_DECLINED_EMAIL_FILENAME, EmailParameters);
                var SendEmail = _emailHandler.SendEmail(employee.User.Email, "YOUR TIMESHEET(S) HAS BEEN DECLINED", EmailTemplate, "");


                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                return _logger.Error<bool>(_logger.GetMethodName(), ex);
            }
        }

        /// <summary>
        /// Add work hour for a day.
        /// </summary>
        /// <param name="employeeInformationId">The employee information id</param>
        /// <param name="date">The date with the month and year fro the record needed</param>
        /// <param name="hours">The number hours worked for a particular day</param>
        /// <returns>bool</returns>
        public async Task<StandardResponse<bool>> AddWorkHoursForADay(List<TimesheetHoursAdditionModel> model, Guid employeeInformationId, DateTime date)
        {
            try
            {
                foreach(var record in model)
                {
                    if (record.Date.DayOfWeek == DayOfWeek.Saturday || record.Date.DayOfWeek == DayOfWeek.Sunday)
                        return StandardResponse<bool>.NotFound("You cannot add time sheet for weekends");

                    var timeSheet = _timeSheetRepository.Query().Include(x => x.EmployeeInformation).ThenInclude(x => x.User)
                    .FirstOrDefault(timeSheet => timeSheet.EmployeeInformationId == employeeInformationId && timeSheet.Date.Day == record.Date.Day && timeSheet.Date.Month == record.Date.Month && timeSheet.Date.Year == record.Date.Year);

                    if (timeSheet == null)
                        return StandardResponse<bool>.NotFound("No time sheet found for this user for the date requested");

                    timeSheet.Hours = record.Hours;
                    timeSheet.IsApproved = false;
                    timeSheet.StatusId = (int)Statuses.PENDING;
                    timeSheet.DateModified = DateTime.Now;
                    timeSheet.EmployeeInformation.User.DateModified = DateTime.Now;
                    _timeSheetRepository.Update(timeSheet);
                }

                var timeSheetLink = $"{Globals.FrontEndBaseUrl}Supervisor/timesheets/{employeeInformationId}?date={date.ToString("yyyy-MM-dd")}";
                var employee = _employeeInformationRepository.Query().FirstOrDefault(x => x.Id == employeeInformationId);
                var supervisor = _userRepository.Query().FirstOrDefault(x => x.Id == employee.SupervisorId);

                await _notificationService.SendNotification(new NotificationModel { UserId = supervisor.Id, Title = "Timesheet", Type = "Notification", Message = "Your have pending timesheet that needs your approval" });

                List<KeyValuePair<string, string>> EmailParameters = new()
                {
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, supervisor.FirstName),
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_URL, timeSheetLink)
                };

                var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.TIMESHEET_PENDING_APPROVAL_EMAIL_FILENAME, EmailParameters);
                var SendEmail = _emailHandler.SendEmail(supervisor.Email, "YOU HAVE PENDING TIMESHEET THAT NEEDS YOUR APPROVAL", EmailTemplate, "");

                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                return _logger.Error<bool>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<bool>> AddProjectManagementTimeSheet(Guid userId, DateTime startDate, DateTime endDate)
        {
            try
            {
                if (startDate.Date != endDate.Date) return StandardResponse<bool>.Failed("start date and endate should be the same day");

                if (startDate.Date.DayOfWeek == DayOfWeek.Saturday || startDate.Date.DayOfWeek == DayOfWeek.Sunday)
                    return StandardResponse<bool>.NotFound("You cannot add time sheet for weekends");

                var user = _userRepository.Query().Include(x => x.EmployeeInformation).FirstOrDefault(x => x.Id == userId);
                if (user == null) return StandardResponse<bool>.NotFound("user not found");

                var hours = (endDate - startDate).TotalHours;


                var timeSheet = _timeSheetRepository.Query().Include(x => x.EmployeeInformation).ThenInclude(x => x.User)
                    .FirstOrDefault(timeSheet => timeSheet.EmployeeInformationId == user.EmployeeInformationId && timeSheet.Date.Day == startDate.Date.Day && timeSheet.Date.Month == startDate.Date.Month && timeSheet.Date.Year == startDate.Date.Year);

                if (timeSheet == null)
                    return StandardResponse<bool>.NotFound("No time sheet found for this user for the date requested");

                //if(timeSheet.Hours + hours > 8) return StandardResponse<bool>.NotFound("you filled more than eight hours for this day");

                timeSheet.Hours += (int)hours;
                timeSheet.IsApproved = false;
                timeSheet.StatusId = (int)Statuses.PENDING;
                timeSheet.DateModified = DateTime.Now;
                timeSheet.EmployeeInformation.User.DateModified = DateTime.Now;
                _timeSheetRepository.Update(timeSheet);

                var timeSheetLink = $"{Globals.FrontEndBaseUrl}Supervisor/timesheets/{user.EmployeeInformationId}?date={startDate.ToString("yyyy-MM-dd")}";
                var employee = _employeeInformationRepository.Query().FirstOrDefault(x => x.Id == user.EmployeeInformationId);
                var supervisor = _userRepository.Query().FirstOrDefault(x => x.Id == employee.SupervisorId);

                await _notificationService.SendNotification(new NotificationModel { UserId = supervisor.Id, Title = "Timesheet", Type = "Notification", Message = "Your have pending timesheet that needs your approval" });

                List<KeyValuePair<string, string>> EmailParameters = new()
                {
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, supervisor.FirstName),
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_URL, timeSheetLink)
                };

                var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.TIMESHEET_PENDING_APPROVAL_EMAIL_FILENAME, EmailParameters);
                var SendEmail = _emailHandler.SendEmail(supervisor.Email, "YOU HAVE PENDING TIMESHEET THAT NEEDS YOUR APPROVAL", EmailTemplate, "");

                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                return _logger.Error<bool>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<bool>> TreatProjectManagementTimeSheet(Guid userId, bool isApproved, DateTime startDate, DateTime endDate, string reason = null)
        {
            try
            {
                if (startDate.Date != endDate.Date) return StandardResponse<bool>.Failed("start date and endate should be the same day");

                if (startDate.Date.DayOfWeek == DayOfWeek.Saturday || startDate.Date.DayOfWeek == DayOfWeek.Sunday)
                    return StandardResponse<bool>.NotFound("You cannot add time sheet for weekends");

                var user = _userRepository.Query().Include(x => x.EmployeeInformation).FirstOrDefault(x => x.Id == userId);
                if (user == null) return StandardResponse<bool>.NotFound("user not found");

                var hours = (endDate - startDate).TotalHours;


                var timeSheet = _timeSheetRepository.Query().Include(x => x.EmployeeInformation).ThenInclude(x => x.User)
                    .FirstOrDefault(timeSheet => timeSheet.EmployeeInformationId == user.EmployeeInformationId && timeSheet.Date.Day == startDate.Date.Day && timeSheet.Date.Month == startDate.Date.Month && timeSheet.Date.Year == startDate.Date.Year);

                if (timeSheet == null)
                    return StandardResponse<bool>.NotFound("No time sheet found for this user for the date requested");

                if (isApproved)
                {
                    timeSheet.IsApproved = true;
                    timeSheet.StatusId = (int)Statuses.APPROVED;
                    timeSheet.DateModified = DateTime.Now;
                    timeSheet.EmployeeInformation.User.DateModified = DateTime.Now;
                }
                else
                {
                    timeSheet.IsApproved = false;
                    timeSheet.StatusId = (int)Statuses.REJECTED;
                    timeSheet.RejectionReason = reason;
                    timeSheet.DateModified = DateTime.Now;
                    timeSheet.EmployeeInformation.User.DateModified = DateTime.Now;
                }

                _timeSheetRepository.Update(timeSheet);



                if (isApproved)
                {
                    var timeSheetLink = $"{Globals.FrontEndBaseUrl}TeamMember/timesheets/{user.EmployeeInformationId}?date={startDate.ToString("yyyy-MM-dd")}";

                    await _notificationService.SendNotification(new NotificationModel { UserId = user.Id, Title = "Timesheet Approved", Type = "Notification", Message = $"Your timesheet has been approved" });

                    List<KeyValuePair<string, string>> EmailParameters = new()
                    {
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, user.FirstName),
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_URL, timeSheetLink)
                    };

                    var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.TIMESHEET_APPROVAL_EMAIL_FILENAME, EmailParameters);
                    var SendEmail = _emailHandler.SendEmail(user.Email, "YOUR TIMESHEET HAS BEEN APPROVED", EmailTemplate, "");
                }
                else
                {
                    await _notificationService.SendNotification(new NotificationModel { UserId = user.Id, Title = "Timesheet Rejected", Type = "Notification", Message = $"Your timesheet(s) was rejected" });

                    var timeSheetLink = $"{Globals.FrontEndBaseUrl}TeamMember/timesheets/{user.EmployeeInformationId}?date={startDate.ToString("yyyy-MM-dd")}";

                    List<KeyValuePair<string, string>> EmailParameters = new()
                {
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, user.FirstName),
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_COMMENT, reason),
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_URL, timeSheetLink)
                };

                    var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.TIMESHEET_DECLINED_EMAIL_FILENAME, EmailParameters);
                    var SendEmail = _emailHandler.SendEmail(user.Email, "YOUR TIMESHEET(S) HAS BEEN DECLINED", EmailTemplate, "");
                }


                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                return _logger.Error<bool>(_logger.GetMethodName(), ex);
            }
        }

        /// <summary>
        /// Get Approved Timesheet
        /// </summary>
        /// <returns></returns>
        public async Task<StandardResponse<PagedCollection<TimeSheetApprovedView>>> GetApprovedTimeSheet(PagingOptions pagingOptions, Guid superAdminId, string search = null)
        {
            try
            {
                var loggedInUserRole = _httpContextAccessor.HttpContext.User.GetLoggedInUserRole();

                var allUsers = _userRepository.Query().Include(u => u.EmployeeInformation).Where(user => (user.Role.ToLower() == "team member" && user.IsActive == true || user.Role.ToLower() == "internal admin" && user.IsActive == true || user.Role.ToLower() == "internal supervisor" && user.IsActive == true) && user.SuperAdminId == superAdminId).OrderByDescending(x => x.DateModified);

                if (!string.IsNullOrEmpty(search))
                {
                    allUsers = allUsers.Where(user => user.FirstName.ToLower().Contains(search.ToLower()) || user.LastName.ToLower().Contains(search.ToLower())
                    || (user.FirstName.ToLower() + " " + user.LastName.ToLower()).Contains(search.ToLower())).OrderByDescending(x => x.DateModified);
                }

                var pagedUsers = allUsers.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value).ToList().AsQueryable();

                var allApprovedTimeSheet = new List<TimeSheetApprovedView>();

                foreach (var user in pagedUsers)
                {
                    var approvedTimeSheets = GetRecentlyApprovedTimeSheet(user);
                    if (user == null) continue;
                    if (approvedTimeSheets == null) continue;
                    allApprovedTimeSheet.Add(approvedTimeSheets);
                }

                var pagedCollection = PagedCollection<TimeSheetApprovedView>.Create(Link.ToCollection(nameof(TimeSheetController.ListApprovedTimeSheet)), allApprovedTimeSheet.ToArray(), allUsers.Count(), pagingOptions);
                return StandardResponse<PagedCollection<TimeSheetApprovedView>>.Ok(pagedCollection);
            }
            catch (Exception ex)
            {
                return _logger.Error<PagedCollection<TimeSheetApprovedView>>(_logger.GetMethodName(), ex);
            }

        }

        public async Task<StandardResponse<PagedCollection<TimeSheetApprovedView>>> GetApprovedTeamMemberTimeSheet(PagingOptions pagingOptions, Guid employeeInformationId)
        {
            try
            {
                var approvedTimeSheet = _timeSheetRepository.Query().Include(timeSheet => timeSheet.EmployeeInformation).ThenInclude(timeSheet => timeSheet.User).
                    Where(timeSheet => timeSheet.EmployeeInformationId == employeeInformationId).ToList();

                var allApprovedTimeSheet = new List<TimeSheetApprovedView>();

                var groupByMonth = approvedTimeSheet.GroupBy(month => month.Date.Month);

                foreach (var timeSheet in groupByMonth)
                {
                    foreach (var record in timeSheet)
                    {
                        var totalHours = timeSheet.Sum(timeSheet => timeSheet.Hours);
                        var approvedHours = timeSheet.Where(timeSheet => timeSheet.IsApproved == true).Sum(timeSheet => timeSheet.Hours);
                        var noOfDays = timeSheet.AsQueryable().Count();
                        var startDate = new DateTime(record.Date.Year, record.Date.Month, 1);
                        var endDate = startDate.AddMonths(1).AddSeconds(-1);
                        var approvedTimeSheets = new TimeSheetApprovedView
                        {
                            Name = record.EmployeeInformation.User.FirstName + " " + record.EmployeeInformation.User.LastName,
                            Email = record.EmployeeInformation.User.Email,
                            EmployeeInformationId = record.EmployeeInformationId,
                            TotalHours = totalHours,
                            NumberOfDays = noOfDays,
                            ApprovedNumberOfHours = approvedHours,
                            Date = record.Date,
                            EmployeeInformation = _mapper.Map<EmployeeInformationView>(record.EmployeeInformation),
                            StartDate = startDate,
                            EndDate = endDate,
                            DateModified = timeSheet.Max(x => x.DateModified)
                        };
                        allApprovedTimeSheet.Add(approvedTimeSheets);
                    }
                }
                allApprovedTimeSheet = allApprovedTimeSheet.OrderByDescending(x => x.DateModified).GroupBy(x => new { x.EmployeeInformationId, x.Date.Month, x.Date.Year }).Select(y => y.First()).ToList();

                var allApprovedTimeSheetPaginated = allApprovedTimeSheet.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value).ToList();

                var pagedCollection = PagedCollection<TimeSheetApprovedView>.Create(Link.ToCollection(nameof(TimeSheetController.ListTeamMemberApprovedTimeSheet)), allApprovedTimeSheetPaginated.ToArray(), allApprovedTimeSheet.Count(), pagingOptions);

                return StandardResponse<PagedCollection<TimeSheetApprovedView>>.Ok(pagedCollection);
            }
            catch (Exception ex)
            {
                return _logger.Error<PagedCollection<TimeSheetApprovedView>>(_logger.GetMethodName(), ex);
            }

        }

        public async Task<StandardResponse<PagedCollection<TimeSheetHistoryView>>> GetSuperviseesTimeSheet(PagingOptions pagingOptions, string search = null, DateFilter dateFilter = null)
        {
            try
            {
                Guid UserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();
                var allSupervisees = _userRepository.Query().Include(u => u.EmployeeInformation).Where(x => x.EmployeeInformation.SupervisorId == UserId).ToList();

                

                if (!string.IsNullOrEmpty(search))
                {
                    allSupervisees = allSupervisees.Where(user => user.FirstName.ToLower().Contains(search.ToLower()) || user.LastName.ToLower().Contains(search.ToLower())
                    || (user.FirstName.ToLower() + " " + user.LastName.ToLower()).Contains(search.ToLower())).ToList();
                }

                var pageUsers = allSupervisees.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value).ToList().AsQueryable();

                var allTimeSheetHistory = new List<TimeSheetHistoryView>();

                foreach (var user in pageUsers)
                {
                    if (user.IsActive == false) continue;
                    var timeSheetHistory = GetTimeSheetHistory(user, dateFilter);

                    allTimeSheetHistory.Add(timeSheetHistory);
                }

                var timeSheetHistories = allTimeSheetHistory.OrderByDescending(x => x.DateModified);

                var pagedCollection = PagedCollection<TimeSheetHistoryView>.Create(Link.ToCollection(nameof(TimeSheetController.GetSuperviseesTimeSheet)), timeSheetHistories.ToArray(), allSupervisees.Count(), pagingOptions);
                return StandardResponse<PagedCollection<TimeSheetHistoryView>>.Ok(pagedCollection);
            }
            catch (Exception ex)
            {
                return _logger.Error<PagedCollection<TimeSheetHistoryView>>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<PagedCollection<TimeSheetApprovedView>>> GetSuperviseesApprovedTimeSheet(PagingOptions pagingOptions, string search = null, DateFilter dateFilter = null)
        {
            try
            {
                Guid UserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();
                var allSupervisees = _userRepository.Query().Include(x => x.EmployeeInformation).Where(x => x.EmployeeInformation.SupervisorId == UserId).ToList();

                if (!string.IsNullOrEmpty(search))
                {
                    allSupervisees = allSupervisees.Where(user => user.FirstName.ToLower().Contains(search.ToLower()) || user.LastName.ToLower().Contains(search.ToLower())
                    || (user.FirstName.ToLower() + " " + user.LastName.ToLower()).Contains(search.ToLower())).ToList();
                }

                var pageUsers = allSupervisees.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value).ToList().AsQueryable();

                var allTimeSheetHistory = new List<TimeSheetApprovedView>();

                foreach (var user in pageUsers)
                {
                    if (user.IsActive == false) continue;
                    var timeSheetHistory = GetTimeSheetApproved(user, dateFilter);
                    allTimeSheetHistory.Add(timeSheetHistory);
                }

                var approvedTimesheet = allTimeSheetHistory.OrderByDescending(x => x.DateModified);

                var pagedCollection = PagedCollection<TimeSheetApprovedView>.Create(Link.ToCollection(nameof(TimeSheetController.GetSuperviseesApprovedTimeSheet)), approvedTimesheet.ToArray(), allSupervisees.Count(), pagingOptions);
                return StandardResponse<PagedCollection<TimeSheetApprovedView>>.Ok(pagedCollection);
            }
            catch (Exception ex)
            {
                return _logger.Error<PagedCollection<TimeSheetApprovedView>>(_logger.GetMethodName(), ex);
            }
        }

        /// <summary>
        /// Add work hour for a day.
        /// </summary>
        /// <param name="employeeInformationId">The employee information id</param>
        /// <param name="date">The date with the month and year fro the record needed</param>
        /// <returns>bool</returns>

        private ExpectedEarnings GetExpectedWorkHoursAndPay(Guid? employeeInformationId, DateTime date)
        {
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var businessDays = GetBusinessDays(firstDayOfMonth, lastDayOfMonth);

            var employeeInformation = _employeeInformationRepository.Query().Include(u => u.PayrollType).FirstOrDefault(e => e.Id == employeeInformationId);

            double expectedWorkHours = 0;
            double? expectedPay = 0;

            if (employeeInformation.PayrollType.Name == PayrollTypes.ONSHORE.ToString())
            {
                expectedWorkHours = employeeInformation.HoursPerDay * businessDays;
                expectedPay = employeeInformation.RatePerHour * employeeInformation.HoursPerDay * businessDays;
            }

            if (employeeInformation.PayrollType.Name == PayrollTypes.OFFSHORE.ToString())
            {
                expectedWorkHours = employeeInformation.HoursPerDay * businessDays;
                expectedPay = employeeInformation.MonthlyPayoutRate;
            }

            var earnings = new ExpectedEarnings { ExpectedPay = expectedPay, ExpectedWorkHours = expectedWorkHours };
            return earnings;

        }

        private ExpectedEarnings GetExpectedWorkHoursAndPay(Guid? employeeInformationId, DateTime startDate, DateTime endDate)
        {
            var businessDays = GetBusinessDays(startDate, endDate);

            var employeeInformation = _employeeInformationRepository.Query().Include(u => u.PayrollType).FirstOrDefault(e => e.Id == employeeInformationId);

            double expectedWorkHours = 0;
            double? expectedPay = 0;

            if (employeeInformation.PayrollType.Name == PayrollTypes.ONSHORE.ToString())
            {
                expectedWorkHours = employeeInformation.HoursPerDay * businessDays;
                expectedPay = employeeInformation.RatePerHour * employeeInformation.HoursPerDay * businessDays;
            }

            if (employeeInformation.PayrollType.Name == PayrollTypes.OFFSHORE.ToString())
            {
                expectedWorkHours = employeeInformation.HoursPerDay * businessDays;
                expectedPay = employeeInformation.MonthlyPayoutRate;
            }

            var earnings = new ExpectedEarnings { ExpectedPay = expectedPay, ExpectedWorkHours = expectedWorkHours };
            return earnings;

        }

        public ExpectedEarnings GetExpectedWorkHoursAndPay2(Guid employeeInformationId, DateTime startDate, DateTime endDate)
        {
            var businessDays = GetBusinessDays(startDate, endDate);

            var employeeInformation = _employeeInformationRepository.Query().Include(u => u.PayrollType).FirstOrDefault(e => e.Id == employeeInformationId);

            double expectedWorkHours = 0;
            double? expectedPay = 0;

            if (employeeInformation.PayrollType.Name == PayrollTypes.ONSHORE.ToString())
            {
                expectedWorkHours = employeeInformation.HoursPerDay * businessDays;
                expectedPay = employeeInformation.RatePerHour * employeeInformation.HoursPerDay * businessDays;
            }

            if (employeeInformation.PayrollType.Name == PayrollTypes.OFFSHORE.ToString())
            {
                expectedWorkHours = employeeInformation.HoursPerDay * businessDays;
                expectedPay = employeeInformation.MonthlyPayoutRate;
            }

            var earnings = new ExpectedEarnings { ExpectedPay = expectedPay, ExpectedWorkHours = expectedWorkHours };
            return earnings;

        }

        public double GetBusinessDays(DateTime startD, DateTime endD)
        {
            double calcBusinessDays = 1 + ((endD - startD).TotalDays * 5 - (startD.DayOfWeek - endD.DayOfWeek) * 2) / 7;
            if (endD.DayOfWeek == DayOfWeek.Saturday) calcBusinessDays--;
            if (startD.DayOfWeek == DayOfWeek.Sunday) calcBusinessDays--;

            return calcBusinessDays;
        }
        
        /// <summary>
        /// Generate a payroll for all timesheets approved today or update an existing one
        /// </summary>
        /// param name="employeeInformationId">The id of the employee to generate timesheet for</param>
        /// <returns></returns>
        public async Task<StandardResponse<bool>> GeneratePayroll(Guid employeeInformationId)
        {
            try
            {
                var timeSheet = _timeSheetRepository.Query()
                .Where(timeSheet => timeSheet.IsApproved == true && timeSheet.EmployeeInformationId == employeeInformationId && timeSheet.DateModified.Day == DateTime.Now.Day).ToList();

                if (timeSheet.Count <= 0)
                    return StandardResponse<bool>.Ok(false);

                var employeeInformation = _employeeInformationRepository.Query().Include(u => u.PayrollType).Include(e => e.User).FirstOrDefault(e => e.Id == employeeInformationId);

                if (employeeInformation == null)
                    return StandardResponse<bool>.NotFound("Employee information not found");

                var totalHours = timeSheet.Sum(timeSheet => timeSheet.Hours);

                var existingPayroll = _payrollRepository.Query().FirstOrDefault(p => p.StartDate == timeSheet.First().Date && p.EndDate == timeSheet.Last().Date);

                if (existingPayroll != null)
                    return StandardResponse<bool>.Ok(true).AddStatusMessage("This payroll has aready been generated");

                var payroll = new Payroll
                {
                    EmployeeInformationId = employeeInformationId,
                    TotalHours = totalHours,
                    DateCreated = DateTime.Now,
                    DateModified = DateTime.Now,
                    TotalAmount = totalHours * employeeInformation.RatePerHour,
                    Rate = employeeInformation.RatePerHour,
                    PayRollTypeId = employeeInformation.PayRollTypeId,
                    Name = employeeInformation.User.FullName,
                    StatusId = (int)Statuses.PENDING,
                    StartDate = timeSheet.First().Date,
                    EndDate = timeSheet.Last().Date,
                    PaymentPartnerId = employeeInformation.PaymentPartnerId ?? null
                };
                payroll = _payrollRepository.CreateAndReturn(payroll);

                return StandardResponse<bool>.Ok(true);

            }
            catch (Exception ex)
            {
                return _logger.Error<bool>(_logger.GetMethodName(), ex);
            }
        }

        /// <summary>
        // List team member timesheet history for the past five months
        /// </summary>
        /// <returns>bool</returns>
        public async Task<StandardResponse<PagedCollection<TimeSheetHistoryView>>> GetTeamMemberTimeSheetHistory(PagingOptions pagingOptions)
        {
            try
            {
                var loggedInUserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();
                var employeeInformation = _employeeInformationRepository.Query().Include(e => e.User).FirstOrDefault(e => e.UserId == loggedInUserId);
                var timeSheetHistory = new List<TimeSheetHistoryView>();
                var timeSheet = _timeSheetRepository.Query()
                    .Where(timeSheet => timeSheet.EmployeeInformationId == employeeInformation.Id && timeSheet.IsApproved == true && timeSheet.Date.Month >= DateTime.Now.AddMonths(-5).Month && timeSheet.Date.Year >= DateTime.Now.AddMonths(-5).Year).ToList();

                var groupByMonth = timeSheet.GroupBy(month => month.Date.Month);

                foreach (var timeSheetRecord in groupByMonth)
                {
                    foreach (var record in timeSheetRecord)
                    {
                        var approvedHours = timeSheetRecord.Where(x => x.IsApproved).Sum(timeSheet => timeSheet.Hours);
                        var totalHours = timeSheetRecord.Sum(timeSheet => timeSheet.Hours);
                        var noOfDays = timeSheetRecord.AsQueryable().Count();
                        var startDate = new DateTime(record.Date.Year, record.Date.Month, 1);
                        var endDate = startDate.AddMonths(1).AddSeconds(-1);
                        var timeSheetHistoryView = new TimeSheetHistoryView
                        {
                            Name = employeeInformation.User.FirstName + " " + employeeInformation.User.LastName,
                            Email = employeeInformation.User.Email,
                            EmployeeInformationId = employeeInformation.Id,
                            TotalHours = totalHours,
                            NumberOfDays = noOfDays,
                            Date = record.Date,
                            EmployeeInformation = _mapper.Map<EmployeeInformationView>(employeeInformation),
                            ApprovedNumberOfHours = approvedHours,
                            StartDate = startDate,
                            EndDate = endDate,
                            DateModified = timeSheetRecord.Max(x => x.DateModified)
                        };
                        timeSheetHistory.Add(timeSheetHistoryView);
                    }
                }
                timeSheetHistory = timeSheetHistory.OrderByDescending(x => x.DateModified).GroupBy(x => new { x.EmployeeInformationId, x.Date.Month, x.Date.Year }).Select(y => y.First()).ToList();

                var timeSheetHistoryPaginated = timeSheetHistory.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value).ToList();

                var pagedCollection = PagedCollection<TimeSheetHistoryView>.Create(Link.ToCollection(nameof(TimeSheetController.GetTeamMemberTimeSheetHistory)), timeSheetHistoryPaginated.ToArray(), timeSheetHistory.Count(), pagingOptions);
                return StandardResponse<PagedCollection<TimeSheetHistoryView>>.Ok(pagedCollection);
            }
            catch (Exception ex)
            {
                return _logger.Error<PagedCollection<TimeSheetHistoryView>>(_logger.GetMethodName(), ex);
            }
        }


        public async Task<StandardResponse<PagedCollection<TimeSheetApprovedView>>> GetApprovedClientTeamMemberTimeSheet(PagingOptions pagingOptions, string search = null)
        {
            try
            {
                Guid UserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();

                var allUsers = _userRepository.Query().Include(u => u.EmployeeInformation).Where(user => user.Role.ToLower() == "team member" && user.EmployeeInformation.ClientId == UserId);

                if (!string.IsNullOrEmpty(search))
                {
                    allUsers = allUsers.Where(user => user.FirstName.ToLower().Contains(search.ToLower()) || user.LastName.ToLower().Contains(search.ToLower())
                    || (user.FirstName.ToLower() + " " + user.LastName.ToLower()).Contains(search.ToLower()));
                }

                var pagedUsers = allUsers.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value).ToList().AsQueryable();

                var allApprovedTimeSheet = new List<TimeSheetApprovedView>();

                foreach (var user in pagedUsers)
                {
                    if (user.IsActive == false) continue;
                    var approvedTimeSheet = _timeSheetRepository.Query()
                    .Where(timeSheet => timeSheet.EmployeeInformationId == user.EmployeeInformationId && timeSheet.DateCreated.Month == DateTime.Now.Month &&
                    timeSheet.DateCreated.Year == DateTime.Now.Year);

                    var totalHours = approvedTimeSheet.Sum(timesheet => timesheet.Hours);
                    var approvedHours = approvedTimeSheet.Where(timesheet => timesheet.IsApproved == true).Sum(timesheet => timesheet.Hours);
                    var noOfDays = approvedTimeSheet.Count();

                    var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    var endDate = startDate.AddMonths(1).AddSeconds(-1);

                    var approvedTimeSheets = new TimeSheetApprovedView
                    {
                        Name = user.FirstName + " " + user.LastName,
                        Email = user.Email,
                        EmployeeInformationId = user.EmployeeInformationId,
                        TotalHours = totalHours,
                        NumberOfDays = noOfDays,
                        ApprovedNumberOfHours = approvedHours,
                        EmployeeInformation = _mapper.Map<EmployeeInformationView>(user.EmployeeInformation),
                        StartDate = startDate,
                        EndDate = endDate,
                        DateModified = approvedTimeSheet.Max(x => x.DateModified)
                    };
                    allApprovedTimeSheet.Add(approvedTimeSheets);
                }

                var approvedTimesheets = allApprovedTimeSheet.OrderByDescending(x => x.DateModified);

                var pagedCollection = PagedCollection<TimeSheetApprovedView>.Create(Link.ToCollection(nameof(TimeSheetController.GetApprovedClientTeamMemberSheet)), approvedTimesheets.ToArray(), allUsers.Count(), pagingOptions);
                return StandardResponse<PagedCollection<TimeSheetApprovedView>>.Ok(pagedCollection);
            }
            catch (Exception ex)
            {
                return _logger.Error<PagedCollection<TimeSheetApprovedView>>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<PagedCollection<TimeSheetHistoryView>>> GetClientTimeSheetHistory(PagingOptions pagingOptions, string search = null, DateFilter dateFilter = null)
        {
            try
            {
                Guid UserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();

                var timeSheet = _timeSheetRepository.Query().
                    Where(timeSheet => timeSheet.EmployeeInformation.ClientId == UserId && timeSheet.IsApproved == true);

                if (dateFilter.StartDate.HasValue)
                    timeSheet = timeSheet.Where(u => u.Date.Date >= dateFilter.StartDate).OrderByDescending(u => u.Date);

                if (dateFilter.EndDate.HasValue)
                    timeSheet = timeSheet.Where(u => u.Date.Date <= dateFilter.EndDate).OrderByDescending(u => u.Date);

                var allUsers = _userRepository.Query().Include(u => u.EmployeeInformation).Where(user => user.EmployeeInformation.ClientId == UserId && user.Role.ToLower() == "team member");


                if (!string.IsNullOrEmpty(search))
                {
                    allUsers = allUsers.Where(user => user.FirstName.ToLower().Contains(search.ToLower()) || user.LastName.ToLower().Contains(search.ToLower())
                    || (user.FirstName.ToLower() + " " + user.LastName.ToLower()).Contains(search.ToLower()));
                }

                allUsers = allUsers.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value).ToList().AsQueryable();

                var allTimeSheetHistory = new List<TimeSheetHistoryView>();

                foreach (var user in allUsers)
                {
                    if (user.IsActive == false) continue;
                    var approvedHours = timeSheet.Where(timeSheet => timeSheet.EmployeeInformationId == user.EmployeeInformationId && timeSheet.IsApproved == true).Sum(timeSheet => timeSheet.Hours);
                    var totalHours = _timeSheetRepository.Query().Where(timeSheet => timeSheet.EmployeeInformationId == user.EmployeeInformationId).AsQueryable().Sum(timeSheet => timeSheet.Hours);
                    var noOfDays = _timeSheetRepository.Query().Where(timeSheet => timeSheet.EmployeeInformationId == user.EmployeeInformationId).AsQueryable().Count();
                    var timeSheetHistory = new TimeSheetHistoryView
                    {
                        Name = user.FirstName + " " + user.LastName,
                        Email = user.Email,
                        EmployeeInformationId = user.EmployeeInformationId,
                        TotalHours = totalHours,
                        NumberOfDays = noOfDays,
                        ApprovedNumberOfHours = approvedHours,
                        EmployeeInformation = _mapper.Map<EmployeeInformationView>(user.EmployeeInformation),
                        StartDate = user.DateCreated,
                        EndDate = DateTime.Now,
                        DateModified = timeSheet.Max(x => x.DateModified)
                    };

                    allTimeSheetHistory.Add(timeSheetHistory);
                }

                var timesheetHistories = allTimeSheetHistory.OrderByDescending(x => x.DateModified);

                var pagedCollection = PagedCollection<TimeSheetHistoryView>.Create(Link.ToCollection(nameof(TimeSheetController.GetClientTimeSheetHistory)), timesheetHistories.ToArray(), allUsers.Count(), pagingOptions);
                return StandardResponse<PagedCollection<TimeSheetHistoryView>>.Ok(pagedCollection);

            }
            catch (Exception ex)
            {
                return _logger.Error<PagedCollection<TimeSheetHistoryView>>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<PagedCollection<RecentTimeSheetView>>> GetTeamMemberRecentTimeSheet(PagingOptions pagingOptions, Guid employeeInformationId, DateFilter dateFilter = null)
        {
            try
            {
                //var loggedInUserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();
                var employeeInformation = _employeeInformationRepository.Query().Include(e => e.User).FirstOrDefault(e => e.Id == employeeInformationId);
                var timeSheets = _timeSheetRepository.Query()
                        .Where(timeSheet => timeSheet.EmployeeInformationId == employeeInformation.Id).OrderByDescending(a => a.DateCreated).ToList();

                if (dateFilter.StartDate.HasValue)
                    timeSheets = timeSheets.Where(u => u.Date.Date >= dateFilter.StartDate).OrderByDescending(u => u.Date).ToList();

                if (dateFilter.EndDate.HasValue)
                    timeSheets = timeSheets.Where(u => u.Date.Date <= dateFilter.EndDate).OrderByDescending(u => u.Date).ToList();

                var recentTimeSheets = new List<RecentTimeSheetView>();

                var groupByMonth = timeSheets.GroupBy(month => new { month.Date.Year, month.Date.Month });
                var count = groupByMonth.Count();

                foreach (var timeSheet in groupByMonth)
                {
                    foreach (var record in timeSheet)
                    {
                        var totalHours = timeSheet.Sum(timeSheet => timeSheet.Hours);
                        var noOfDays = timeSheet.AsQueryable().Count();
                        var recentTimeSheet = new RecentTimeSheetView
                        {
                            Name = employeeInformation.User.FullName,
                            Year = record.Date.Year.ToString(),
                            Month = _utilityMethods.GetMonthName(record.Date.Month),
                            Hours = totalHours,
                            NumberOfDays = noOfDays,
                            EmployeeInformationId = record.EmployeeInformationId,
                            DateCreated = record.Date,
                        };
                        recentTimeSheets.Add(recentTimeSheet);
                    }
                }
                recentTimeSheets = recentTimeSheets.GroupBy(x => new { x.EmployeeInformationId, x.DateCreated.Month, x.DateCreated.Year }).Select(y => y.First()).ToList();
                var pagedTimeSheets = recentTimeSheets.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value).ToList().AsQueryable();

                var pagedCollection = PagedCollection<RecentTimeSheetView>.Create(Link.ToCollection(nameof(TimeSheetController.GetTeamMemberRecentTimeSheet)), pagedTimeSheets.ToArray(), recentTimeSheets.Count(), pagingOptions);
                return StandardResponse<PagedCollection<RecentTimeSheetView>>.Ok(pagedCollection);
            }
            catch(Exception ex)
            {
                return _logger.Error<PagedCollection<RecentTimeSheetView>>(_logger.GetMethodName(), ex);
            }
            
        }
        private TimeSheetHistoryView GetTimeSheetHistory(User user, DateFilter dateFilter = null)
        {
            var timesheets = _timeSheetRepository.Query().Where(timesheet => timesheet.EmployeeInformationId == user.EmployeeInformationId && 
            timesheet.Date.DayOfWeek != DayOfWeek.Saturday && timesheet.Date.DayOfWeek != DayOfWeek.Sunday).OrderByDescending(u => u.Date);

            if(!timesheets.Any()) return null;

            DateTime? startDate = null;

            if(timesheets.Where(x => x.DateModified.Date > x.Date.Date && (x.StatusId == (int)Statuses.APPROVED || x.StatusId == (int)Statuses.REJECTED))
                .OrderBy(u => u.Date).Any())
            {
                startDate = timesheets?.First()?.Date;
            }

            //DateTime? startDate = timesheets?.Where(x => x.DateModified.Date > x.Date.Date && (x.StatusId == (int)Statuses.APPROVED || x.StatusId == (int)Statuses.REJECTED))
            //    .OrderBy(u => u.Date)?.First()?.Date;
            if (startDate == null) return null;

            var lastTimesheet = timesheets.OrderBy(x => x.Date).LastOrDefault(x => x.EmployeeInformationId == user.EmployeeInformationId);

            var endDate = _paymentScheduleRepository.Query().FirstOrDefault(x => x.CycleType.ToLower() == user.EmployeeInformation.PaymentFrequency.ToLower() && lastTimesheet.Date.Date >= x.WeekDate.Date.Date && 
            lastTimesheet.Date.Date <= x.LastWorkDayOfCycle.Date && x.SuperAdminId == user.SuperAdminId).LastWorkDayOfCycle;




            if (dateFilter.StartDate.HasValue)
                timesheets = timesheets.Where(u => u.Date.Date >= dateFilter.StartDate).OrderByDescending(u => u.Date);

            if (dateFilter.EndDate.HasValue)
                timesheets = timesheets.Where(u => u.Date.Date <= dateFilter.EndDate).OrderByDescending(u => u.Date);

            var approvedHours = timesheets.Where(timeSheet => timeSheet.EmployeeInformationId == user.EmployeeInformationId && timeSheet.IsApproved == true).AsQueryable()
                .Sum(timeSheet => timeSheet.Hours);
            var totalHours = timesheets.Where(timeSheet => timeSheet.EmployeeInformationId == user.EmployeeInformationId).AsQueryable().Sum(timeSheet => timeSheet.Hours);
            var noOfDays = timesheets.Where(timeSheet => timeSheet.EmployeeInformationId == user.EmployeeInformationId).AsQueryable().Count();
            var timeSheetHistory = new TimeSheetHistoryView
            {
                Name = user.FirstName + " " + user.LastName,
                Email = user.Email,
                EmployeeInformationId = user.EmployeeInformationId,
                TotalHours = totalHours,
                NumberOfDays = noOfDays,
                ApprovedNumberOfHours = approvedHours,
                EmployeeInformation = _mapper.Map<EmployeeInformationView>(user.EmployeeInformation),
                StartDate = dateFilter.StartDate.HasValue ? dateFilter.StartDate.Value : startDate.Value,
                EndDate = dateFilter.EndDate.HasValue ? dateFilter.EndDate.Value : endDate,
                DateModified = timesheets.Max(x => x.DateModified)
            };

            return timeSheetHistory;
        }

        private TimeSheetApprovedView GetTimeSheetApproved(User user, DateFilter dateFilter = null)
        {
            var timesheets = _timeSheetRepository.Query().Where(timesheet => timesheet.EmployeeInformationId == user.EmployeeInformationId);

            if (dateFilter.StartDate.HasValue)
                timesheets = timesheets.Where(u => u.Date.Date >= dateFilter.StartDate).OrderByDescending(u => u.Date);

            if (dateFilter.EndDate.HasValue)
                timesheets = timesheets.Where(u => u.Date.Date <= dateFilter.EndDate).OrderByDescending(u => u.Date);

            var totalHours = _timeSheetRepository.Query().Where(timeSheet => timeSheet.EmployeeInformationId == user.EmployeeInformationId).AsQueryable().Sum(timeSheet => timeSheet.Hours);
            var approvedHours = _timeSheetRepository.Query().Where(timeSheet => timeSheet.EmployeeInformationId == user.EmployeeInformationId && timeSheet.IsApproved == true).AsQueryable().Sum(timeSheet => timeSheet.Hours);
            var noOfDays = _timeSheetRepository.Query().Where(timeSheet => timeSheet.EmployeeInformationId == user.EmployeeInformationId && timeSheet.IsApproved == true).AsQueryable().Count();
            var timeSheetHistory = new TimeSheetApprovedView
            {
                Name = user.FirstName + " " + user.LastName,
                Email = user.Email,
                EmployeeInformation = _mapper.Map<EmployeeInformationView>(user.EmployeeInformation),
                EmployeeInformationId = user.EmployeeInformationId,
                TotalHours = totalHours,
                NumberOfDays = noOfDays,
                ApprovedNumberOfHours = approvedHours,
                StartDate = dateFilter.StartDate.HasValue ? dateFilter.StartDate.Value : user.DateCreated,
                EndDate = dateFilter.EndDate.HasValue ? dateFilter.EndDate.Value : DateTime.Now
            };

            return timeSheetHistory;
        }

        //recently approved timesheet testing period
        public TimeSheetApprovedView GetRecentlyApprovedTimeSheet(User user)
        {
            try
            {
                var employee = _employeeInformationRepository.Query().FirstOrDefault(x => x.Id == user.EmployeeInformationId);

                var lastTimesheet = _timeSheetRepository.Query().OrderBy(x => x.Date).LastOrDefault(x => x.EmployeeInformationId == user.EmployeeInformationId);
                if (lastTimesheet == null) return null;
                PaymentSchedule period = null;

                //if(employee.PaymentFrequency.ToLower() == "monthly")
                //{
                //    period = _paymentScheduleRepository.Query().FirstOrDefault(x => x.CycleType.ToLower() == employee.PaymentFrequency.ToLower() && DateTime.Today.Date >= x.WeekDate.Date.Date && DateTime.Now.Date.AddDays(-10) <= x.LastWorkDayOfCycle.Date && lastTimesheet.Date.Date >= x.WeekDate.Date);
                //}
                //else
                //{
                //    period = _paymentScheduleRepository.Query().FirstOrDefault(x => x.CycleType.ToLower() == employee.PaymentFrequency.ToLower() && DateTime.Today.Date >= x.WeekDate.Date.Date && DateTime.Now.Date.AddDays(-2) <= x.LastWorkDayOfCycle.Date && lastTimesheet.Date.Date >= x.WeekDate.Date.Date);
                //}

                var currentDate = DateTime.Now.Date;

                if (currentDate.DayOfWeek == DayOfWeek.Saturday) currentDate = currentDate.AddDays(-1);

                if(currentDate.DayOfWeek == DayOfWeek.Sunday) currentDate = currentDate.AddDays(1);

                period = _paymentScheduleRepository.Query().FirstOrDefault(x => x.CycleType.ToLower() == employee.PaymentFrequency.ToLower() && currentDate >= x.WeekDate.Date.Date && currentDate <= x.LastWorkDayOfCycle.Date);

                var timeSheet = _timeSheetRepository.Query()
                    .Where(timeSheet => timeSheet.EmployeeInformationId == employee.Id && timeSheet.Date.Date >= period.WeekDate.Date && timeSheet.Date.Date <= period.LastWorkDayOfCycle.Date.Date 
                    && timeSheet.Date.DayOfWeek != DayOfWeek.Saturday && timeSheet.Date.DayOfWeek != DayOfWeek.Sunday);

                var expectedEarnings = GetExpectedWorkHoursAndPay2(employee.Id, period.WeekDate, period.LastWorkDayOfCycle);

                var totalHours = timeSheet.Sum(timesheet => timesheet.Hours);
                var approvedHours = timeSheet.Where(timesheet => timesheet.IsApproved == true).Sum(timesheet => timesheet.Hours);
                var noOfDays = timeSheet.Count();

                var actualPayout = (expectedEarnings.ExpectedPay * approvedHours) / expectedEarnings.ExpectedWorkHours;

                var approvedTimeSheets = new TimeSheetApprovedView
                {
                    Name = user.FirstName + " " + user.LastName,
                    Email = user.Email,
                    EmployeeInformationId = user.EmployeeInformationId,
                    TotalHours = totalHours,
                    NumberOfDays = noOfDays,
                    ApprovedNumberOfHours = approvedHours,
                    ExpectedHours = expectedEarnings.ExpectedWorkHours,
                    ExpectedPayout = expectedEarnings.ExpectedPay,
                    ActualPayout = actualPayout,
                    EmployeeInformation = _mapper.Map<EmployeeInformationView>(user.EmployeeInformation),
                    StartDate = period.WeekDate,
                    EndDate = period.LastWorkDayOfCycle
                    //DateModified = timeSheet.Max(x => x.DateModified)
                };

                return approvedTimeSheets;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }

        public double? GetOffshoreTeamMemberTotalPay(Guid? employeeInformationId, DateTime startDate, DateTime endDate, int totalHoursworked, int invoiceType)
        {
            var employeeInformation = _employeeInformationRepository.Query().Include(u => u.PayrollType).FirstOrDefault(e => e.Id == employeeInformationId);
            var businessDays = GetBusinessDays(startDate, endDate);
            var expectedWorkHours = employeeInformation.HoursPerDay * businessDays;
            var expectedPay = invoiceType == 1 ? employeeInformation?.MonthlyPayoutRate : employeeInformation?.ClientRate;
            var totalEarnings = (expectedPay * totalHoursworked) / expectedWorkHours;
            return totalEarnings;

        }

        public double? GetTeamMemberPayPerHour(Guid userId)
        {
            var user = _userRepository.Query().FirstOrDefault(x => x.Id == userId);
            var employeeInformation = _employeeInformationRepository.Query().Include(u => u.PayrollType).FirstOrDefault(e => e.Id == user.EmployeeInformationId);

            var firstDateOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).Date;
            var lastDayOfMonth = firstDateOfMonth.AddMonths(1).AddDays(-1).Date;
            var totalHourForTheMonth = (lastDayOfMonth - firstDateOfMonth).TotalHours / 3;
            var businessDays = GetBusinessDays(firstDateOfMonth, lastDayOfMonth);
            var earningsPerHour = employeeInformation.PayRollTypeId == 1 ? employeeInformation.RatePerHour : employeeInformation.MonthlyPayoutRate / totalHourForTheMonth;
            
            return earningsPerHour;

        }

        public async Task<StandardResponse<bool>> CreateTimeSheetForADay(DateTime date, Guid? employeeInformationId = null)
        {
            try
            {
                if (employeeInformationId.HasValue)
                {
                    var employeeInformaion = _employeeInformationRepository.Query().Include(x => x.User).FirstOrDefault(x => x.Id == employeeInformationId);
                    if (employeeInformaion == null) return StandardResponse<bool>.Error("No employee found");
                    var timeSheet = new TimeSheet
                    {
                        Date = date,
                        EmployeeInformationId = (Guid)employeeInformationId,
                        Hours = 0,
                        IsApproved = false,
                        StatusId = (int)Statuses.PENDING
                    };
                    _timeSheetRepository.CreateAndReturn(timeSheet);

                    var timesheet = _timeSheetRepository.Query().Include(x => x.EmployeeInformation).FirstOrDefault(timeSheet => timeSheet.EmployeeInformationId == employeeInformationId && timeSheet.Date.Day == date.Day && timeSheet.Date.Month == date.Month && timeSheet.Date.Year == date.Year);
                    var checkIfOnLeave = _leaveRepository.Query().FirstOrDefault(x => x.EmployeeInformationId == employeeInformationId && x.StartDate.Date <= date.Date && date.Date <= x.EndDate.Date && x.StatusId == (int)Statuses.APPROVED);
                    var employeeInformation = _employeeInformationRepository.Query().FirstOrDefault(x => x.Id == employeeInformationId);
                    if (checkIfOnLeave != null)
                    {
                        var noOfDaysEligible = _leaveService.GetEligibleLeaveDays(employeeInformationId);
                        noOfDaysEligible = noOfDaysEligible - employeeInformation.NumberOfEligibleLeaveDaysTaken;
                        if (noOfDaysEligible > 0)
                        {
                            timeSheet.OnLeave = true;
                            timeSheet.OnLeaveAndEligibleForLeave = true;
                            //timeSheet.Hours = employeeInformation.NumberOfHoursEligible ?? default(int);

                            employeeInformation.NumberOfEligibleLeaveDaysTaken += 1;
                            _employeeInformationRepository.Update(employeeInformation);
                        }
                        if (noOfDaysEligible <= 0)
                        {
                            timeSheet.OnLeave = true;
                            timeSheet.OnLeaveAndEligibleForLeave = false;
                        }
                    }
                    timesheet.EmployeeInformation.User.DateModified = DateTime.Now;
                    _timeSheetRepository.Update(timesheet);
                }
                else
                {
                    var allUsers = _userRepository.Query().Where(user => user.Role.ToLower() == "team member" || user.Role.ToLower() == "internal supervisor" || user.Role.ToLower() == "internal admin" || user.Role.ToLower() == "internal payroll manager").ToList();

                    foreach (var user in allUsers)
                    {
                        if (_timeSheetRepository.Query().Any(timeSheet => timeSheet.EmployeeInformationId == user.EmployeeInformationId && timeSheet.Date.Day == date.Day &&
                        timeSheet.Date.Month == date.Month && timeSheet.Date.Year == date.Year))
                            continue;
                        if (user.EmployeeInformationId == null) continue;
                        if (date.DayOfWeek == DayOfWeek.Saturday) continue;
                        if (date.DayOfWeek == DayOfWeek.Sunday) continue;
                        if (user.IsActive == false) continue;
                        if (user.EmailConfirmed == false) continue;
                        var timeSheet = new TimeSheet
                        {
                            Date = date,
                            EmployeeInformationId = (Guid)user.EmployeeInformationId,
                            Hours = 0,
                            IsApproved = false,
                            StatusId = (int)Statuses.PENDING
                        };


                        _timeSheetRepository.CreateAndReturn(timeSheet);
                        var timesheet = _timeSheetRepository.Query().Include(x => x.EmployeeInformation).FirstOrDefault(timeSheet => timeSheet.EmployeeInformationId == user.EmployeeInformationId && timeSheet.Date.Day == date.Day && timeSheet.Date.Month == date.Month && timeSheet.Date.Year == date.Year);
                        var checkIfOnLeave = _leaveRepository.Query().FirstOrDefault(x => x.EmployeeInformationId == user.EmployeeInformationId && x.StartDate.Date <= date.Date && date.Date <= x.EndDate.Date && x.StatusId == (int)Statuses.APPROVED);
                        var employeeInformation = _employeeInformationRepository.Query().FirstOrDefault(x => x.Id == user.EmployeeInformationId);
                        if (checkIfOnLeave != null)
                        {
                            if (date.DayOfWeek == DayOfWeek.Saturday) continue;
                            if (date.DayOfWeek == DayOfWeek.Sunday) continue;

                            var noOfDaysEligible = _leaveService.GetEligibleLeaveDays(user.EmployeeInformationId);
                            noOfDaysEligible = noOfDaysEligible - employeeInformation.NumberOfEligibleLeaveDaysTaken;
                            if (noOfDaysEligible > 0)
                            {
                                timeSheet.OnLeave = true;
                                timeSheet.OnLeaveAndEligibleForLeave = true;
                                //timeSheet.Hours = employeeInformation.NumberOfHoursEligible ?? default(int);

                                employeeInformation.NumberOfEligibleLeaveDaysTaken += 1;
                                _employeeInformationRepository.Update(employeeInformation);
                            }
                            if (noOfDaysEligible <= 0)
                            {
                                timeSheet.OnLeave = true;
                                timeSheet.OnLeaveAndEligibleForLeave = false;
                            }
                        }
                        timesheet.EmployeeInformation.User.DateModified = DateTime.Now;
                        _timeSheetRepository.Update(timesheet);
                    }
                }
                
                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                return _logger.Error<bool>(_logger.GetMethodName(), ex);
            }
        }

        public StandardResponse<byte[]> ExportTimesheetRecord(TimesheetRecordDownloadModel model, DateFilter dateFilter, Guid superAdminId)
        {
            try
            {
                switch (model.Record)
                {
                    case TimesheetRecordToDownload.TimesheetApproved:
                        var allUsers = _userRepository.Query().Include(u => u.EmployeeInformation).Where(user => (user.Role.ToLower() == "team member" && user.IsActive == true || user.Role.ToLower() == "internal admin" && user.IsActive == true || user.Role.ToLower() == "internal supervisor" && user.IsActive == true) && user.SuperAdminId == superAdminId).OrderByDescending(x => x.DateModified).ToList();

                        var allApprovedTimeSheet = new List<TimeSheetApprovedView>();

                        foreach (var user in allUsers)
                        {
                            var approvedTimeSheets = GetRecentlyApprovedTimeSheet(user);
                            if (user == null) continue;
                            if (approvedTimeSheets == null) continue;
                            allApprovedTimeSheet.Add(approvedTimeSheets);
                        }
                        if (dateFilter.StartDate.HasValue) allApprovedTimeSheet.Where(x => dateFilter.StartDate.Value.Date >= x.StartDate.Date);

                        if (dateFilter.EndDate.HasValue) allApprovedTimeSheet.Where(x => dateFilter.EndDate.Value.Date >= x.EndDate.Date);

                        var workbook = _dataExport.ExportTimesheetRecords(model.Record, allApprovedTimeSheet, model.rowHeaders);
                        return StandardResponse<byte[]>.Ok(workbook);
                        break;

                    case TimesheetRecordToDownload.TeamMemberApproved:

                        var employeeInformation = _employeeInformationRepository.Query().Include(e => e.User).FirstOrDefault(e => e.Id == model.EmployeeInformationId);
                        var timeSheets = _timeSheetRepository.Query()
                                .Where(timeSheet => timeSheet.EmployeeInformationId == employeeInformation.Id).OrderByDescending(a => a.DateCreated).ToList();

                        if (dateFilter.StartDate.HasValue)
                            timeSheets = timeSheets.Where(u => u.Date.Date >= dateFilter.StartDate).OrderByDescending(u => u.Date).ToList();

                        if (dateFilter.EndDate.HasValue)
                            timeSheets = timeSheets.Where(u => u.Date.Date <= dateFilter.EndDate).OrderByDescending(u => u.Date).ToList();

                        var recentTimeSheets = new List<RecentTimeSheetView>();

                        var groupByMonth = timeSheets.GroupBy(month => new { month.Date.Year, month.Date.Month });
                        var count = groupByMonth.Count();

                        foreach (var timeSheet in groupByMonth)
                        {
                            foreach (var record in timeSheet)
                            {
                                var totalHours = timeSheet.Sum(timeSheet => timeSheet.Hours);
                                var noOfDays = timeSheet.AsQueryable().Count();
                                var recentTimeSheet = new RecentTimeSheetView
                                {
                                    Name = employeeInformation.User.FullName,
                                    Year = record.Date.Year.ToString(),
                                    Month = _utilityMethods.GetMonthName(record.Date.Month),
                                    Hours = totalHours,
                                    NumberOfDays = noOfDays,
                                    EmployeeInformationId = record.EmployeeInformationId,
                                    DateCreated = record.Date,
                                };
                                recentTimeSheets.Add(recentTimeSheet);
                            }
                        }
                        recentTimeSheets = recentTimeSheets.GroupBy(x => new { x.EmployeeInformationId, x.DateCreated.Month, x.DateCreated.Year }).Select(y => y.First()).ToList();

                        if (dateFilter.StartDate.HasValue) recentTimeSheets.Where(x => dateFilter.StartDate.Value.Date >= x.DateCreated.Date);

                        if (dateFilter.EndDate.HasValue) recentTimeSheets.Where(x => dateFilter.EndDate.Value.Date >= x.DateCreated.Date);
                        workbook = _dataExport.ExportTeamMemberTimesheetRecords(model.Record, recentTimeSheets, model.rowHeaders);
                        return StandardResponse<byte[]>.Ok(workbook);
                        break;
                    case TimesheetRecordToDownload.TimesheetHistory:

                        var allUsersTimesheetHistory = _userRepository.Query().Include(u => u.EmployeeInformation).Where(user => (user.Role.ToLower() == "team member" || user.Role.ToLower() == "internal supervisor" || user.Role.ToLower() == "internal admin") && user.SuperAdminId == superAdminId);
                        var allTimeSheetHistory = new List<TimeSheetHistoryView>();

                        foreach (var user in allUsersTimesheetHistory)
                        {
                            if (user.IsActive == false) continue;
                            var timeSheetHistory = GetTimeSheetHistory(user, dateFilter);

                            if (timeSheetHistory == null) continue;

                            allTimeSheetHistory.Add(timeSheetHistory);
                        }

                        var timeSheetHistories = allTimeSheetHistory.OrderByDescending(x => x.DateModified).ToList();
                        workbook = _dataExport.ExportTimesheetHistoryRecords(model.Record, timeSheetHistories, model.rowHeaders);
                        return StandardResponse<byte[]>.Ok(workbook);
                        break;

                    default:
                        break;
                }
                
                return StandardResponse<byte[]>.Error("error downloading file, please try again");
            }
            catch (Exception e)
            {
                return StandardResponse<byte[]>.Error(e.Message);
            }
        }
    }
}