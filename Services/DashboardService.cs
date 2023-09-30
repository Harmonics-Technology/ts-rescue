using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Repositories.Interfaces;
using TimesheetBE.Services.Abstractions;
using TimesheetBE.Services.Interfaces;
using TimesheetBE.Utilities;
using TimesheetBE.Utilities.Abstrctions;
using TimesheetBE.Utilities.Extentions;
using Month = TimesheetBE.Models.ViewModels.Month;

namespace TimesheetBE.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICustomLogger<DashboardService> _logger;
        private readonly IConfigurationProvider _configuration;
        private readonly ITimeSheetRepository _timeSheetRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IPayrollRepository _payrollRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUtilityMethods _utilityMethods;
        private readonly IPaySlipRepository _paySlipRepository;
        private readonly IEmployeeInformationRepository _employeeInformationRepository;
        private readonly IPaymentScheduleRepository _paymentScheduleRepository;
        private readonly ITimeSheetService _timeSheetService;
        private readonly IProjectManagementService _projectManagementService;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectTaskRepository _projectTaskRepository;
        private readonly IProjectTimesheetRepository _projectTimesheetRepository;
        public DashboardService(IUserRepository userRepository, ICustomLogger<DashboardService> logger, IConfigurationProvider configuration, ITimeSheetRepository timeSheetRepository,
            IPayrollRepository payrollRepository, IHttpContextAccessor httpContextAccessor, IUtilityMethods utilityMethods, IInvoiceRepository invoiceRepository,
            IPaySlipRepository paySlipRepository, IEmployeeInformationRepository employeeInformationRepository, IPaymentScheduleRepository paymentScheduleRepository, ITimeSheetService timeSheetService, 
            IProjectManagementService projectManagementService, IProjectRepository projectRepository, IProjectTaskRepository projectTaskRepository, IProjectTimesheetRepository projectTimesheetRepository)
        {
            _userRepository = userRepository;
            _logger = logger;
            _configuration = configuration;
            _timeSheetRepository = timeSheetRepository;
            _invoiceRepository = invoiceRepository;
            _payrollRepository = payrollRepository;
            _httpContextAccessor = httpContextAccessor;
            _utilityMethods = utilityMethods;
            _paySlipRepository = paySlipRepository;
            _employeeInformationRepository = employeeInformationRepository;
            _paymentScheduleRepository = paymentScheduleRepository;
            _timeSheetService = timeSheetService;
            _projectManagementService = projectManagementService;
            _projectRepository = projectRepository;
            _projectTaskRepository = projectTaskRepository;
            _projectTimesheetRepository = projectTimesheetRepository;
        }

        public async Task<StandardResponse<DashboardView>> GetDashBoardMetrics(Guid superAminId)
        {
            try
            {
                //var loggedInUserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();
                var allClient = _userRepository.ListUsers().Result.Users.Count(user => user.Role.ToLower() == "client" && user.SuperAdminId == superAminId);
                var allTeamMember = _userRepository.ListUsers().Result.Users.Count(user => user.Role.ToLower() == "team member" && user.SuperAdminId == superAminId);
                var allTeamMembers = _userRepository.Query().Include(x => x.EmployeeInformation).ThenInclude(x => x.Supervisor).Where(x => (x.Role.ToLower() == "team member" && x.IsActive == true && x.EmailConfirmed == true || x.Role.ToLower() == "internal admin" && x.IsActive == true && x.EmailConfirmed == true || x.Role.ToLower() == "internal supervisor" && x.IsActive == true && x.EmailConfirmed == true) && x.SuperAdminId == superAminId).OrderByDescending(x => x.DateModified).Take(5).ToList();
                var allAdmins = _userRepository.ListUsers().Result.Users.Count(user => (user.Role.ToLower() == "super admin" || user.Role.ToLower() == "admin" || user.Role.ToLower() == "internal payroll manager" || user.Role.ToLower() == "business manager" || user.Role.ToLower() == "payroll manager") && user.SuperAdminId == superAminId);
                var recentClients = _userRepository.ListUsers().Result.Users.Where(user => user.DateCreated <= DateTime.Now.AddMonths(1) && user.Role.ToLower() == "client" && user.SuperAdminId == superAminId).OrderByDescending(user => user.DateCreated).Take(5);
                var recentPayrolls = _invoiceRepository.Query().Include(x => x.EmployeeInformation).ThenInclude(x => x.User).Where(payroll => payroll.StatusId != (int)Statuses.PENDING && payroll.StatusId != (int)Statuses.INVOICED && payroll.EmployeeInformation.PayRollTypeId == 2 && payroll.EmployeeInformation.User.SuperAdminId == superAminId).ProjectTo<InvoiceView>(_configuration).OrderByDescending(payroll => payroll.DateCreated).Take(5);
                var recentInvoiced = _invoiceRepository.Query().Include(x => x.EmployeeInformation).ThenInclude(x => x.User).Where(invoiced => invoiced.StatusId == (int)Statuses.INVOICED && invoiced.EmployeeInformation.User.SuperAdminId == superAminId).ProjectTo<InvoiceView>(_configuration).OrderByDescending(invoice => invoice.DateCreated).Take(5);
                var recentPayslips = _paySlipRepository.Query().Include(x => x.EmployeeInformation).ThenInclude(x => x.User).Where(payslip => payslip.EmployeeInformation.User.SuperAdminId == superAminId).ProjectTo<PaySlipView>(_configuration).OrderByDescending(payslip => payslip.DateCreated).Take(5);
                var recentTimesheets = _timeSheetRepository.Query().Include(user => user.EmployeeInformation).ThenInclude(user => user.User).Where(timesheet => timesheet.Date < DateTime.Now && timesheet.EmployeeInformation.User.SuperAdminId == superAminId).OrderByDescending(timesheet => timesheet.Date).ProjectTo<TimeSheetView>(_configuration).Take(5);

                var recentTimesheetView = new List<TimeSheetApprovedView>();
                foreach (var user in allTeamMembers)
                {
                    //if (user.IsActive == false) continue;
                    var approvedTimesheet = _timeSheetService.GetRecentlyApprovedTimeSheet(user);
                    if (approvedTimesheet == null) continue;
                    recentTimesheetView.Add(approvedTimesheet);

                }


                var metrics = new DashboardView
                {
                    TotalClients = allClient,
                    TotalTeamMembers = allTeamMember,
                    TotalDownLines = allAdmins,
                    RecentCLients = recentClients.ProjectTo<UserView>(_configuration).ToList(),
                    RecentPayrolls = recentPayrolls.ToList(),
                    RecentInvoiced = recentInvoiced.ToList(),
                    RecentPayslips = recentPayslips.ToList(),
                    RecentTimeSheet = recentTimesheetView.OrderByDescending(x => x.DateModified).ToList()
                };

                return StandardResponse<DashboardView>.Ok(metrics);
                //return StandardResponse<DashboardView>.Ok();
            }
            catch (Exception ex)
            {
                return _logger.Error<DashboardView>(MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        public async Task<StandardResponse<DashboardTeamMemberView>> GetTeamMemberDashBoard(Guid employeeInformationId)
        {
            try
            {
                var currentYear = DateTime.Now.Year;
                var timeSheet = _timeSheetRepository.Query()
                .Where(timeSheet => timeSheet.EmployeeInformationId == employeeInformationId && timeSheet.Date.Year == currentYear).ToList();

                var monthlyGroupedTimeSheet = timeSheet.GroupBy(x => x.Date.Month).ToList();





                var allApprovedTimeSheet = 0;
                var allAwaitingTimeSheet = 0;
                var allRejectedTimeSheet = 0;

                monthlyGroupedTimeSheet.ForEach(x =>
                {
                    if (x.All(y => y.StatusId == (int)Statuses.APPROVED && y.Date.DayOfWeek != DayOfWeek.Saturday && y.Date.DayOfWeek != DayOfWeek.Sunday))
                        allApprovedTimeSheet++;

                    if (x.Any(y => y.StatusId == (int)Statuses.PENDING && y.Date.DayOfWeek != DayOfWeek.Saturday && y.Date.DayOfWeek != DayOfWeek.Sunday))
                        allAwaitingTimeSheet++;

                    if (x.Any(y => y.StatusId == (int)Statuses.REJECTED && y.Date.DayOfWeek != DayOfWeek.Saturday && y.Date.DayOfWeek != DayOfWeek.Sunday))
                        allRejectedTimeSheet++;
                });

                var recentTimeSheet = GetTeamMemberRecentTimeSheet(employeeInformationId, null, null);

                var metrics = new DashboardTeamMemberView
                {
                    ApprovedTimeSheet = allApprovedTimeSheet,
                    AwaitingTimeSheet = allAwaitingTimeSheet,
                    RejectedTimeSheet = allRejectedTimeSheet,
                    RecentTimeSheet = recentTimeSheet.Take(3).ToList()
                };

                return StandardResponse<DashboardTeamMemberView>.Ok(metrics);
            }
            catch (Exception ex)
            {
                return _logger.Error<DashboardTeamMemberView>(MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        public async Task<StandardResponse<DashboardPaymentPartnerView>> GetPaymentPartnerDashboard()
        {
            try
            {
                var loggedInUserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();
 
                var recentPayrolls = _invoiceRepository.Query().Include(x => x.Expenses).Include(invoice => invoice.EmployeeInformation).ThenInclude(invoice => invoice.User)
                    .Where(invoice => invoice.EmployeeInformation.PaymentPartnerId == loggedInUserId && invoice.StatusId != (int)Statuses.INVOICED).ProjectTo<InvoiceView>(_configuration).OrderByDescending(x => x.DateCreated).Take(5);

                var recentPayslips = _invoiceRepository.Query().Where(invoice => invoice.PaymentPartnerId == loggedInUserId && invoice.StatusId == (int)Statuses.APPROVED).ProjectTo<InvoiceView>(_configuration).OrderByDescending(invoice => invoice.DateCreated).Take(5);

                var recentInvoicedInvoice = _invoiceRepository.Query().Where(invoice => invoice.PaymentPartnerId == loggedInUserId && invoice.StatusId == (int)Statuses.INVOICED).ProjectTo<InvoiceView>(_configuration).OrderByDescending(invoice => invoice.DateCreated).Take(5);
                
                var metrics = new DashboardPaymentPartnerView { RecentPayroll = recentPayrolls.ToList(), RecentApprovedInvoice = recentPayslips.ToList(), RecentInvoicedInvoice = recentInvoicedInvoice.ToList() };
                return StandardResponse<DashboardPaymentPartnerView>.Ok(metrics);
            }
            catch (Exception ex)
            {
                return _logger.Error<DashboardPaymentPartnerView>(MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        public async Task<StandardResponse<DashboardClientView>> GetClientDashBoard()
        {
            try
            {
                var loggedInUserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();

                var timeSheet = _timeSheetRepository.Query()
                .Where(timeSheet => timeSheet.EmployeeInformation.Supervisor.ClientId == loggedInUserId).ToList();

                var monthlyGroupedTimeSheet = timeSheet.GroupBy(x => new { x.EmployeeInformationId }).ToList();

                var allApprovedTimeSheet = 0;
                var allAwaitingTimeSheet = 0;
                var allRejectedTimeSheet = 0;

                monthlyGroupedTimeSheet.ForEach(x =>
                {
                    if (x.All(y => y.StatusId == (int)Statuses.APPROVED && y.Date.DayOfWeek != DayOfWeek.Saturday && y.Date.DayOfWeek != DayOfWeek.Sunday))
                        allApprovedTimeSheet++;

                    if (x.Any(y => y.StatusId == (int)Statuses.PENDING && y.Date.DayOfWeek != DayOfWeek.Saturday && y.Date.DayOfWeek != DayOfWeek.Sunday))
                        allAwaitingTimeSheet++;

                    if (x.Any(y => y.StatusId == (int)Statuses.REJECTED && y.Date.DayOfWeek != DayOfWeek.Saturday && y.Date.DayOfWeek != DayOfWeek.Sunday))
                        allRejectedTimeSheet++;
                });
                
                var recentTimeSheet = GetTeamMemberRecentTimeSheet(null, loggedInUserId, null);

                var metrics = new DashboardClientView
                {
                    ApprovedTimeSheet = allApprovedTimeSheet,
                    AwaitingTimeSheet = allAwaitingTimeSheet,
                    RejectedTimeSheet = allRejectedTimeSheet,
                    RecentTimeSheet = recentTimeSheet.Take(3).ToList(),
                    RecentInvoice = null
                };

                return StandardResponse<DashboardClientView>.Ok(metrics);
            }
            catch (Exception ex)
            {
                return _logger.Error<DashboardClientView>(MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        public async Task<StandardResponse<DashboardClientView>> GetSupervisorDashBoard()
        {
            try
            {
                var loggedInUserId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();
                var timeSheet = _timeSheetRepository.Query()
                .Where(timeSheet => timeSheet.EmployeeInformation.SupervisorId == loggedInUserId).ToList();

                var monthlyGroupedTimeSheet = timeSheet.GroupBy(x => new { x.EmployeeInformationId }).ToList();

                var allApprovedTimeSheet = 0;
                var allAwaitingTimeSheet = 0;
                var allRejectedTimeSheet = 0;

                monthlyGroupedTimeSheet.ForEach(x =>
                {
                    if (x.All(y => y.StatusId == (int)Statuses.APPROVED && y.Date.DayOfWeek != DayOfWeek.Saturday && y.Date.DayOfWeek != DayOfWeek.Sunday))
                        allApprovedTimeSheet++;

                    if (x.Any(y => y.StatusId == (int)Statuses.PENDING && y.Date.DayOfWeek != DayOfWeek.Saturday && y.Date.DayOfWeek != DayOfWeek.Sunday))
                        allAwaitingTimeSheet++;

                    if (x.Any(y => y.StatusId == (int)Statuses.REJECTED && y.Date.DayOfWeek != DayOfWeek.Saturday && y.Date.DayOfWeek != DayOfWeek.Sunday))
                        allRejectedTimeSheet++;
                });
                var recentTimeSheet = GetTeamMemberRecentTimeSheet(null, null, loggedInUserId);

                var metrics = new DashboardClientView
                {
                    ApprovedTimeSheet = allApprovedTimeSheet,
                    AwaitingTimeSheet = allAwaitingTimeSheet,
                    RejectedTimeSheet = allRejectedTimeSheet,
                    RecentTimeSheet = recentTimeSheet.Take(3).ToList(),
                    RecentInvoice = null
                };

                return StandardResponse<DashboardClientView>.Ok(metrics);
            }
            catch (Exception ex)
            {
                return _logger.Error<DashboardClientView>(MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private List<RecentTimeSheetView> GetTeamMemberRecentTimeSheet(Guid? employeeInformationId = null, Guid? clientId = null, Guid? supervisorId = null)
        {
            List<TimeSheet> timeSheets = null;
            if (employeeInformationId.HasValue && employeeInformationId.Value != Guid.Empty)
                timeSheets = _timeSheetRepository.Query().Include(x => x.EmployeeInformation)
                    .Where(timeSheet => timeSheet.EmployeeInformationId == employeeInformationId && timeSheet.Date.DayOfWeek != DayOfWeek.Saturday
                    && timeSheet.Date.DayOfWeek != DayOfWeek.Sunday).OrderByDescending(a => a.Date).ToList();
            if (clientId.HasValue && clientId.Value != Guid.Empty)
                timeSheets = _timeSheetRepository.Query().Include(timeSheet => timeSheet.EmployeeInformation).ThenInclude(timeSheet => timeSheet.Supervisor)
                    .Where(timeSheet => timeSheet.EmployeeInformation.Supervisor.ClientId == clientId && timeSheet.Date.DayOfWeek != DayOfWeek.Saturday
                    && timeSheet.Date.DayOfWeek != DayOfWeek.Sunday).OrderByDescending(a => a.Date).ToList();
            if (supervisorId.HasValue && supervisorId.Value != Guid.Empty)
                timeSheets = _timeSheetRepository.Query().Include(timeSheet => timeSheet.EmployeeInformation).ThenInclude(timeSheet => timeSheet.Supervisor)
                   .Where(timeSheet => timeSheet.EmployeeInformation.SupervisorId == supervisorId && timeSheet.Date.DayOfWeek != DayOfWeek.Saturday
                    && timeSheet.Date.DayOfWeek != DayOfWeek.Sunday).OrderByDescending(a => a.Date).ToList();

            var recentTimeSheets = new List<RecentTimeSheetView>();

            var groupByMonth = timeSheets.GroupBy(month => new { month.Date.Year, month.Date.Month, month.EmployeeInformationId });
            var count = groupByMonth.Count();

            foreach (var timeSheet in groupByMonth)
            {
                foreach (var record in timeSheet)
                {
                    var employee = _employeeInformationRepository.Query().Include(x => x.User).FirstOrDefault(x => x.Id == record.EmployeeInformationId);
                    var totalHours = timeSheet.Sum(timeSheet => timeSheet.Hours);
                    var noOfDays = timeSheet.AsQueryable().Count();
                    var recentTimeSheet = new RecentTimeSheetView
                    {
                        Name = employee.User.FirstName + " " + employee.User.LastName,
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

            return recentTimeSheets;
        }

        private List<RecentInvoiceView> GetRecentInvoices(Guid clientId)
        {
            var invoices = _invoiceRepository.Query().
                Where(invoice => invoice.EmployeeInformation.Supervisor.ClientId == clientId).OrderByDescending(a => a.DateCreated).ToList();
            var clientName = _userRepository.Query().FirstOrDefault(user => user.Id == clientId).OrganizationName;
            var recentInvoices = new List<RecentInvoiceView>();
            foreach (var invoice in invoices)
            {
                var invoiceStatus = (Statuses)invoice.StatusId;
                var sglInvoice = new RecentInvoiceView
                {
                    Client = clientName,
                    InvoiceReference = invoice.InvoiceReference,
                    Amount = invoice.TotalAmount,
                    GeneratedOn = invoice.DateCreated,
                    Status = invoiceStatus.ToString(),
                };
                recentInvoices.Add(sglInvoice);
            }

            return recentInvoices;
        }

        private List<RecentInvoiceView> GetSuperviseesRecentInvoices(Guid supervisorId)
        {
            var invoices = _invoiceRepository.Query().Include(x => x.EmployeeInformation).ThenInclude(y => y.User).
                Where(invoice => invoice.EmployeeInformation.SupervisorId == supervisorId).OrderByDescending(a => a.DateCreated).ToList();
            var recentInvoices = new List<RecentInvoiceView>();
            foreach (var invoice in invoices)
            {
                var invoiceStatus = (Statuses)invoice.StatusId;
                var sglInvoice = new RecentInvoiceView
                {
                    Client = null,
                    TeamMemberName = invoice.EmployeeInformation.User.FullName,
                    InvoiceReference = invoice.InvoiceReference,
                    Amount = invoice.TotalAmount,
                    GeneratedOn = invoice.DateCreated,
                    Status = invoiceStatus.ToString(),
                };
                recentInvoices.Add(sglInvoice);
            }

            return recentInvoices;
        }

        public async Task<StandardResponse<DashboardProjectManagementView>> GetProjectManagementDashboard(Guid superAdminId)
        {
            try
            {
                var noOfProject = _projectRepository.Query().Where(x => x.SuperAdminId == superAdminId).Count();
                var noOfTasks = _projectTaskRepository.Query().Where(x => x.SuperAdminId == superAdminId).Count();
                var totalNumberOfHours = _projectTimesheetRepository.Query().Include(x => x.ProjectTask).Where(x => x.ProjectTask.SuperAdminId == superAdminId).Sum(x => x.TotalHours);
                var totalBudgetSpent = _projectTimesheetRepository.Query().Include(x => x.ProjectTask).Where(x => x.ProjectTask.SuperAdminId == superAdminId).Sum(x => x.AmountEarned);
                var projectSummary = _projectRepository.Query().Where(x => x.SuperAdminId == superAdminId).Take(5).ProjectTo<ProjectView>(_configuration).ToList();
                var overdueProject = _projectRepository.Query().Where(x => x.SuperAdminId == superAdminId && DateTime.Now.Date > x.EndDate).Take(5).ProjectTo<ProjectView>(_configuration).ToList();

                var metrics = new DashboardProjectManagementView
                {
                    NoOfProject = noOfProject,
                    NoOfTask = noOfTasks,
                    TotalHours = totalNumberOfHours,
                    TotalBudgetSpent = totalBudgetSpent,
                    ProjectSummary = projectSummary,
                    OverdueProjects = overdueProject,
                    OprationalAndProjectTasksStats = GetMonthlyOperationalAndProjectTasks(superAdminId),
                    BudgetBurnOutRates = GetBudgetBurnOutRate(superAdminId)
                };

                return StandardResponse<DashboardProjectManagementView>.Ok(metrics);



            }
            catch(Exception ex)
            {
                return StandardResponse<DashboardProjectManagementView>.Failed("An error occured");
            }
        }

        private List<OprationTasksVsProjectTask> GetMonthlyOperationalAndProjectTasks(Guid superAdminId)
        {
            int[] months = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            var groupRecordsByYear = new List<OprationTasksVsProjectTask>();

            foreach (var month in months)
            {
                var yearProjectTasks = _projectTaskRepository.Query().Where(x => x.DateCreated.Year == DateTime.Now.Year).ToList();
                var operationalTaskForMonth = yearProjectTasks.Where(x => x.ProjectId == null && x.DateCreated.Month == month).Count();
                var projectTaskForMonth = yearProjectTasks.Where(x => x.ProjectId != null && x.DateCreated.Month == month).Count();
                var record = new OprationTasksVsProjectTask
                {
                    Month = ((Month)month).ToString(),
                    OperationalTask = operationalTaskForMonth,
                    ProjectTask = projectTaskForMonth
                };
                groupRecordsByYear.Add(record);
            }
            return groupRecordsByYear;
        }

        private List<BudgetBurnOutRate> GetBudgetBurnOutRate(Guid superAdminId)
        {
            int[] months = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            var groupRecordsByYear = new List<BudgetBurnOutRate>();

            foreach (var month in months)
            {
                var projectTimesheets = _projectTimesheetRepository.Query().Where(x => x.DateCreated.Year == DateTime.Now.Year).ToList();
                var budgetBurnOutPerMonth = projectTimesheets.Where(x => x.DateCreated.Month == month).Sum(x => x.AmountEarned);
                var record = new BudgetBurnOutRate
                {
                    Month = ((Month)month).ToString(),
                    Rate = budgetBurnOutPerMonth
                };
                groupRecordsByYear.Add(record);
            }
            return groupRecordsByYear;
        }
    }
}