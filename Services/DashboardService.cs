using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
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
        public DashboardService(IUserRepository userRepository, ICustomLogger<DashboardService> logger, IConfigurationProvider configuration, ITimeSheetRepository timeSheetRepository,
            IPayrollRepository payrollRepository, IHttpContextAccessor httpContextAccessor, IUtilityMethods utilityMethods, IInvoiceRepository invoiceRepository,
            IPaySlipRepository paySlipRepository, IEmployeeInformationRepository employeeInformationRepository, IPaymentScheduleRepository paymentScheduleRepository, ITimeSheetService timeSheetService)
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
        }

        public async Task<StandardResponse<DashboardView>> GetDashBoardMetrics()
        {
            try
            {
                var allClient = _userRepository.ListUsers().Result.Users.Count(user => user.Role.ToLower() == "client");
                var allTeamMember = _userRepository.ListUsers().Result.Users.Count(user => user.Role.ToLower() == "team member");
                var allTeamMembers = _userRepository.Query().Include(x => x.EmployeeInformation).ThenInclude(x => x.Supervisor).Where(x => x.Role.ToLower() == "team member" && x.IsActive == true || x.Role.ToLower() == "internal admin" && x.IsActive == true || x.Role.ToLower() == "internal supervisor" && x.IsActive == true).Take(17).ToList();
                var allAdmins = _userRepository.ListUsers().Result.Users.Count(user => user.Role.ToLower() == "super admin" || user.Role.ToLower() == "admin" || user.Role.ToLower() == "internal payroll manager" || user.Role.ToLower() == "business manager" || user.Role.ToLower() == "payroll manager");
                var recentClients = _userRepository.ListUsers().Result.Users.Where(user => user.DateCreated <= DateTime.Now.AddMonths(1) && user.Role.ToLower() == "client").OrderByDescending(user => user.DateCreated).Take(10);
                var recentPayrolls = _invoiceRepository.Query().Where(payroll => payroll.StatusId != (int)Statuses.PENDING && payroll.StatusId != (int)Statuses.INVOICED).ProjectTo<InvoiceView>(_configuration).OrderByDescending(payroll => payroll.DateCreated).Take(5);
                var recentInvoiced = _invoiceRepository.Query().Where(invoiced => invoiced.StatusId == (int)Statuses.INVOICED).ProjectTo<InvoiceView>(_configuration).OrderByDescending(invoice => invoice.DateCreated).Take(5);
                var recentPayslips = _paySlipRepository.Query().ProjectTo<PaySlipView>(_configuration).OrderByDescending(payslip => payslip.DateCreated).Take(5);
                var recentTimesheets = _timeSheetRepository.Query().Where(timesheet => timesheet.Date < DateTime.Now).Include(user => user.EmployeeInformation).ThenInclude(user => user.User).OrderByDescending(timesheet => timesheet.Date).ProjectTo<TimeSheetView>(_configuration).Take(5);

                var recentTimesheetView = new List<TimeSheetApprovedView>();
                foreach (var user in allTeamMembers)
                {
                    //if (user.IsActive == false) continue;
                    var approvedTimesheet = _timeSheetService.GetRecentlyApprovedTimeSheet(user);
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
                    if (x.All(y => y.StatusId == (int)Statuses.APPROVED))
                        allApprovedTimeSheet++;

                    if (x.Any(y => y.StatusId == (int)Statuses.PENDING))
                        allAwaitingTimeSheet++;

                    if (x.Any(y => y.StatusId == (int)Statuses.REJECTED))
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
                //var recentPayrolls = new List<RecentPayrollView>();
                //foreach (var payroll in recentPayroll)
                //{
                //    var payrollStatus = (Statuses)payroll.StatusId;
                //    var recentPayrollView = new RecentPayrollView
                //    {
                //        Client = payroll.EmployeeInformation.User.OrganizationName,
                //        StartDate = payroll.StartDate,
                //        EndDate = payroll.EndDate,
                //        Rate = Convert.ToDouble(payroll.Rate),
                //        TotalAmount = payroll.TotalAmount,
                //        Status = payrollStatus.ToString(),
                //    };
                //    recentPayrolls.Add(recentPayrollView);
                //}

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
                //var sheet = timeSheet.ToList();

                var monthlyGroupedTimeSheet = timeSheet.GroupBy(x => new { x.Date.Month, x.Date.Year, x.EmployeeInformationId }).ToList();

                var allApprovedTimeSheet = 0;
                var allAwaitingTimeSheet = 0;
                var allRejectedTimeSheet = 0;

                monthlyGroupedTimeSheet.ForEach(x =>
                {
                    if (x.All(y => y.StatusId == (int)Statuses.APPROVED))
                        allApprovedTimeSheet++;

                    if (x.Any(y => y.StatusId == (int)Statuses.PENDING))
                        allAwaitingTimeSheet++;

                    if (x.Any(y => y.StatusId == (int)Statuses.REJECTED))
                        allRejectedTimeSheet++;
                });
                //var allApprovedTimeSheet = timeSheet.Where(i => i.StatusId == (int)Statuses.APPROVED).ToList().Count();
                //var allAwaitingTimeSheet = timeSheet.Where(i => i.StatusId == (int)Statuses.PENDING).ToList().Count();
                //var allRejectedTimeSheet = timeSheet.Where(i => i.StatusId == (int)Statuses.REJECTED).ToList().Count();

                var recentTimeSheet = GetTeamMemberRecentTimeSheet(null, loggedInUserId, null);
                //var recentInvoice = GetRecentInvoices(loggedInUserId);

                var metrics = new DashboardClientView
                {
                    ApprovedTimeSheet = allApprovedTimeSheet,
                    AwaitingTimeSheet = allAwaitingTimeSheet,
                    RejectedTimeSheet = allRejectedTimeSheet,
                    RecentTimeSheet = recentTimeSheet.Take(3).ToList(),
                    //RecentInvoice = recentInvoice.Take(3).ToList()
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

                var monthlyGroupedTimeSheet = timeSheet.GroupBy(x => new { x.Date.Month, x.Date.Year, x.EmployeeInformationId }).ToList();

                var allApprovedTimeSheet = 0;
                var allAwaitingTimeSheet = 0;
                var allRejectedTimeSheet = 0;

                monthlyGroupedTimeSheet.ForEach(x =>
                {
                    if (x.All(y => y.StatusId == (int)Statuses.APPROVED))
                        allApprovedTimeSheet++;

                    if (x.Any(y => y.StatusId == (int)Statuses.PENDING))
                        allAwaitingTimeSheet++;

                    if (x.Any(y => y.StatusId == (int)Statuses.REJECTED))
                        allRejectedTimeSheet++;
                });
                //var allApprovedTimeSheet = timeSheet.Where(i => i.StatusId == (int)Statuses.APPROVED).ToList().Count();
                //var allAwaitingTimeSheet = timeSheet.Where(i => i.StatusId == (int)Statuses.PENDING).ToList().Count();
                //var allRejectedTimeSheet = timeSheet.Where(i => i.StatusId == (int)Statuses.REJECTED).ToList().Count();
                var recentTimeSheet = GetTeamMemberRecentTimeSheet(null, null, loggedInUserId);
                // var recentInvoice = GetSuperviseesRecentInvoices(loggedInUserId);

                var metrics = new DashboardClientView
                {
                    ApprovedTimeSheet = allApprovedTimeSheet,
                    AwaitingTimeSheet = allAwaitingTimeSheet,
                    RejectedTimeSheet = allRejectedTimeSheet,
                    RecentTimeSheet = recentTimeSheet.Take(3).ToList(),
                    //RecentInvoice = recentInvoice.Take(3).ToList()
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
                    .Where(timeSheet => timeSheet.EmployeeInformationId == employeeInformationId).OrderByDescending(a => a.DateCreated).ToList();
            if (clientId.HasValue && clientId.Value != Guid.Empty)
                timeSheets = _timeSheetRepository.Query().Include(timeSheet => timeSheet.EmployeeInformation).ThenInclude(timeSheet => timeSheet.Supervisor)
                    .Where(timeSheet => timeSheet.EmployeeInformation.Supervisor.ClientId == clientId).OrderByDescending(a => a.DateCreated).ToList();
            if (supervisorId.HasValue && supervisorId.Value != Guid.Empty)
                timeSheets = _timeSheetRepository.Query().Include(timeSheet => timeSheet.EmployeeInformation).ThenInclude(timeSheet => timeSheet.Supervisor)
                   .Where(timeSheet => timeSheet.EmployeeInformation.SupervisorId == supervisorId).OrderByDescending(a => a.DateCreated).ToList();

            var recentTimeSheets = new List<RecentTimeSheetView>();

            var groupByMonth = timeSheets.GroupBy(month => new { month.DateCreated.Year, month.DateCreated.Month });
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
                        Year = record.DateCreated.Year.ToString(),
                        Month = _utilityMethods.GetMonthName(record.DateCreated.Month),
                        Hours = totalHours,
                        NumberOfDays = noOfDays,
                        EmployeeInformationId = record.EmployeeInformationId,
                        DateCreated = record.DateCreated,
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

    }
}