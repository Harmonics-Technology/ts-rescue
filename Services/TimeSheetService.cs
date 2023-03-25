using System;
using System.Collections.Generic;
using System.Linq;
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

        public TimeSheetService(IUserRepository userRepository, ITimeSheetRepository timeSheetRepository, IMapper mapper, IConfigurationProvider configurationProvider, IEmployeeInformationRepository employeeInformationRepository, ICustomLogger<TimeSheetService> logger, 
            IPayrollRepository payrollRepository, IHttpContextAccessor httpContextAccessor, 
            IPaymentScheduleRepository paymentScheduleRepository, IInvoiceRepository invoiceRepository, IUtilityMethods utilityMethods, IEmailHandler emailHandler, INotificationService notificationService)
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
        }


        /// <summary>
        /// Get a paginated collection of timesheet history for all user 
        /// </summary>
        /// <param name="pagingOptions">The page number</param>
        /// <returns></returns>
        public async Task<StandardResponse<PagedCollection<TimeSheetHistoryView>>> ListTimeSheetHistories(PagingOptions pagingOptions, string search = null, DateFilter dateFilter = null)
        {
            try
            {
                var loggedInUserRole = _httpContextAccessor.HttpContext.User.GetLoggedInUserRole();

                var allUsers = _userRepository.Query().Include(u => u.EmployeeInformation).Where(user => user.Role.ToLower() == "team member" || user.Role.ToLower() == "internal supervisor" || user.Role.ToLower() == "internal admin");

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
        public async Task<StandardResponse<TimeSheetMonthlyView>> GetTimeSheet(Guid employeeInformationId, DateTime date)
        {
            try
            {
                var timeSheet = _timeSheetRepository.Query()
                .Where(timeSheet => timeSheet.EmployeeInformationId == employeeInformationId && timeSheet.Date.Month == date.Month && timeSheet.Date.Year == date.Year);

                var totalHoursWorked = _timeSheetRepository.Query()
                .Where(timeSheet => timeSheet.EmployeeInformationId == employeeInformationId && timeSheet.Date.Month == date.Month && timeSheet.Date.Year == date.Year)
                .AsQueryable().Sum(timeSheet => timeSheet.Hours);

                var totalApprovedHours = _timeSheetRepository.Query()
                .Where(timeSheet => timeSheet.EmployeeInformationId == employeeInformationId && timeSheet.IsApproved == true && timeSheet.Date.Month == date.Month && timeSheet.Date.Year == date.Year)
                .AsQueryable().Sum(timeSheet => timeSheet.Hours);

                if (timeSheet.Count() == 0)
                    return StandardResponse<TimeSheetMonthlyView>.NotFound("No time sheet found for this user for the date requested");

                var expectedEarnings = GetExpectedWorkHoursAndPay(employeeInformationId, date);

                var employeeInformation = _userRepository.Query().Include(u => u.EmployeeInformation).FirstOrDefault(user => user.EmployeeInformationId == employeeInformationId);

                var timeSheetView = timeSheet.ProjectTo<TimeSheetView>(_configurationProvider).ToList();
                var startDate = new DateTime(date.Year, date.Month, 1);
                var endDate = startDate.AddMonths(1).AddSeconds(-1);
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
            catch (Exception ex)
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
        public async Task<StandardResponse<bool>> ApproveTimeSheetForADay(Guid employeeInformationId, DateTime date)
        {
            try
            {
                var timeSheet = _timeSheetRepository.Query().Include(x => x.EmployeeInformation).ThenInclude(x => x.User)
                .FirstOrDefault(timeSheet => timeSheet.EmployeeInformationId == employeeInformationId && timeSheet.Date.Day == date.Day && timeSheet.Date.Month == date.Month && timeSheet.Date.Year == date.Year);

                if (timeSheet == null)
                    return StandardResponse<bool>.NotFound("No time sheet found for this user for the date requested");

                timeSheet.IsApproved = true;
                timeSheet.StatusId = (int)Statuses.APPROVED;
                timeSheet.DateModified = DateTime.Now;
                timeSheet.EmployeeInformation.User.DateModified = DateTime.Now;
                _timeSheetRepository.Update(timeSheet);

                await _notificationService.SendNotification(new NotificationModel { UserId = timeSheet.EmployeeInformation.UserId, Title = "Timesheet Approved", Type = "Notification", Message = $"Your timesheet for {date.Date} has been approved" });

                List<KeyValuePair<string, string>> EmailParameters = new()
                {
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, timeSheet.EmployeeInformation.User.FirstName),
                };

                var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.TIMESHEET_APPROVAL_EMAIL_FILENAME, EmailParameters);
                var SendEmail = _emailHandler.SendEmail(timeSheet.EmployeeInformation.User.Email, "YOUR TIMESHEET HAS BEEN APPROVED", EmailTemplate, "");


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
        public async Task<StandardResponse<bool>> RejectTimeSheetForADay(RejectTimeSheetModel model)
        {
            try
            {
                var timeSheet = _timeSheetRepository.Query().Include(x => x.EmployeeInformation).ThenInclude(x => x.User)
                .FirstOrDefault(timeSheet => timeSheet.EmployeeInformationId == model.EmployeeInformationId && timeSheet.Date.Day == model.Date.Day && timeSheet.Date.Month == model.Date.Month && timeSheet.Date.Year == model.Date.Year);

                if (timeSheet == null)
                    return StandardResponse<bool>.NotFound("No time sheet found for this user for the date requested");

                

                timeSheet.IsApproved = false;
                timeSheet.StatusId = (int)Statuses.REJECTED;
                timeSheet.RejectionReason = model.Reason;
                timeSheet.DateModified = DateTime.Now;
                timeSheet.EmployeeInformation.User.DateModified = DateTime.Now;
                _timeSheetRepository.Update(timeSheet);

                await _notificationService.SendNotification(new NotificationModel { UserId = timeSheet.EmployeeInformation.UserId, Title = "Timesheet Rejected", Type = "Notification", Message = $"Your timesheet for {model.Date.Date} was rejected" });

                List<KeyValuePair<string, string>> EmailParameters = new()
                {
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, timeSheet.EmployeeInformation.User.FirstName),
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_COMMENT, model.Reason),
                };

                var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.TIMESHEET_DECLINED_EMAIL_FILENAME, EmailParameters);
                var SendEmail = _emailHandler.SendEmail(timeSheet.EmployeeInformation.User.Email, "YOUR TIMESHEET HAS BEEN DECLINED", EmailTemplate, "");


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
        public async Task<StandardResponse<bool>> AddWorkHoursForADay(Guid employeeInformationId, DateTime date, int hours)
        {
            try
            {
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    return StandardResponse<bool>.NotFound("You cannot add time sheet for weekends");

                var timeSheet = _timeSheetRepository.Query().Include(x => x.EmployeeInformation).ThenInclude(x => x.User)
                .FirstOrDefault(timeSheet => timeSheet.EmployeeInformationId == employeeInformationId && timeSheet.Date.Day == date.Day && timeSheet.Date.Month == date.Month && timeSheet.Date.Year == date.Year);

                if (timeSheet == null)
                    return StandardResponse<bool>.NotFound("No time sheet found for this user for the date requested");

                timeSheet.Hours = hours;
                timeSheet.IsApproved = false;
                timeSheet.StatusId = (int)Statuses.PENDING;
                timeSheet.DateModified = DateTime.Now;
                timeSheet.EmployeeInformation.User.DateModified = DateTime.Now;
                _timeSheetRepository.Update(timeSheet);

                var supervisor = _userRepository.Query().FirstOrDefault(x => x.Id == timeSheet.EmployeeInformation.SupervisorId);

                await _notificationService.SendNotification(new NotificationModel { UserId = supervisor.Id, Title = "Timesheet", Type = "Notification", Message = "Your have pending timesheet that needs your approval" });

                List<KeyValuePair<string, string>> EmailParameters = new()
                {
                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, supervisor.FirstName),
                };

                var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.TIMESHEET_PENDING_APPROVAL_EMAIL_FILENAME, EmailParameters);
                var SendEmail = _emailHandler.SendEmail(timeSheet.EmployeeInformation.Supervisor.Email, "YOU HAVE PENDING TIMESHEET THAT NEEDS YOUR APPROVAL", EmailTemplate, "");

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

        public async Task<StandardResponse<PagedCollection<TimeSheetApprovedView>>> GetApprovedTimeSheet(PagingOptions pagingOptions, string search = null)
        {
            try
            {
                var loggedInUserRole = _httpContextAccessor.HttpContext.User.GetLoggedInUserRole();

                var allUsers = _userRepository.Query().Include(u => u.EmployeeInformation).Where(user => user.Role.ToLower() == "team member" && user.IsActive == true || user.Role.ToLower() == "internal admin" && user.IsActive == true || user.Role.ToLower() == "internal supervisor" && user.IsActive == true).OrderByDescending(x => x.DateModified);

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

                var pagedCollection = PagedCollection<TimeSheetApprovedView>.Create(Link.ToCollection(nameof(TimeSheetController.GetApprovedClientTeamMemberSheet)), allApprovedTimeSheetPaginated.ToArray(), allApprovedTimeSheet.Count(), pagingOptions);

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

                var pagedCollection = PagedCollection<TimeSheetHistoryView>.Create(Link.ToCollection(nameof(TimeSheetController.GetApprovedClientTeamMemberSheet)), timeSheetHistoryPaginated.ToArray(), timeSheetHistory.Count(), pagingOptions);
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

                var allUsers = _userRepository.Query().Include(u => u.EmployeeInformation).Where(user => user.Role.ToLower() == "team member" && user.EmployeeInformation.Supervisor.ClientId == UserId);

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

                var pagedCollection = PagedCollection<TimeSheetApprovedView>.Create(Link.ToCollection(nameof(TimeSheetController.ListApprovedTimeSheet)), approvedTimesheets.ToArray(), allUsers.Count(), pagingOptions);
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
                    Where(timeSheet => timeSheet.EmployeeInformation.Supervisor.ClientId == UserId && timeSheet.IsApproved == true);

                if (dateFilter.StartDate.HasValue)
                    timeSheet = timeSheet.Where(u => u.Date.Date >= dateFilter.StartDate).OrderByDescending(u => u.Date);

                if (dateFilter.EndDate.HasValue)
                    timeSheet = timeSheet.Where(u => u.Date.Date <= dateFilter.EndDate).OrderByDescending(u => u.Date);

                var allUsers = _userRepository.Query().Include(u => u.EmployeeInformation).Where(user => user.EmployeeInformation.Supervisor.ClientId == UserId && user.Role.ToLower() == "team member");


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

        public async Task<StandardResponse<PagedCollection<RecentTimeSheetView>>> GetTeamMemberRecentTimeSheet(PagingOptions pagingOptions, DateFilter dateFilter = null)
        {
            try
            {
                var loggedInUserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();
                var employeeInformation = _employeeInformationRepository.Query().Include(e => e.User).FirstOrDefault(e => e.UserId == loggedInUserId);
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
            var timesheets = _timeSheetRepository.Query().Where(timesheet => timesheet.EmployeeInformationId == user.EmployeeInformationId).OrderByDescending(u => u.Date);
            if(dateFilter.StartDate.HasValue)
                timesheets = timesheets.Where(u => u.Date.Date >= dateFilter.StartDate).OrderByDescending(u => u.Date);

            if (dateFilter.EndDate.HasValue)
                timesheets = timesheets.Where(u => u.Date.Date <= dateFilter.EndDate).OrderByDescending(u => u.Date);

            var approvedHours = timesheets.Where(timeSheet => timeSheet.EmployeeInformationId == user.EmployeeInformationId && timeSheet.IsApproved == true).AsQueryable().Sum(timeSheet => timeSheet.Hours);
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
                StartDate = dateFilter.StartDate.HasValue ? dateFilter.StartDate.Value : user.DateCreated,
                EndDate = dateFilter.EndDate.HasValue ? dateFilter.EndDate.Value : DateTime.Now,
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

        public TimeSheetApprovedView GetRecentlyApprovedTimeSheet(User user)
        {
            try
            {
                var employee = _employeeInformationRepository.Query().FirstOrDefault(x => x.Id == user.EmployeeInformationId);

                var lastTimesheet = _timeSheetRepository.Query().OrderBy(x => x.Date).LastOrDefault(x => x.EmployeeInformationId == user.EmployeeInformationId);

                var period = _paymentScheduleRepository.Query().FirstOrDefault(x => x.CycleType.ToLower() == employee.PaymentFrequency.ToLower() && DateTime.Today.Date >= x.WeekDate.Date.Date && DateTime.Now.Date.AddDays(-2) <= x.LastWorkDayOfCycle.Date.Date && lastTimesheet.Date.Date >= x.WeekDate.Date.Date);

                var timeSheet = _timeSheetRepository.Query()
                    .Where(timeSheet => timeSheet.EmployeeInformationId == employee.Id && timeSheet.Date.Date >= period.WeekDate.Date && timeSheet.Date.Date <= period.LastWorkDayOfCycle.Date.Date && timeSheet.Date.DayOfWeek != DayOfWeek.Saturday && timeSheet.Date.DayOfWeek != DayOfWeek.Saturday);

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
    }
}