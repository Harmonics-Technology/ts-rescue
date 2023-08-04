using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TimesheetBE.Controllers;
using TimesheetBE.Models;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Repositories.Interfaces;
using TimesheetBE.Services.Interfaces;
using TimesheetBE.Utilities;
using TimesheetBE.Utilities.Abstrctions;
using TimesheetBE.Utilities.Extentions;

namespace TimesheetBE.Services
{
    public class PayrollService : IPayrollService
    {
        private readonly IPayrollRepository _payrollRepository;
        private readonly IPaySlipRepository _paySlipRepository;
        private readonly IMapper _mapper;
        private readonly IConfigurationProvider _configuration;
        private readonly ICustomLogger<PayrollService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmployeeInformationRepository _employeeInformationRepository;
        private readonly IPaymentScheduleRepository _paymentScheduleRepository;
        public PayrollService(IPayrollRepository payrollRepository, IPaySlipRepository paySlipRepository, IMapper mapper, IConfigurationProvider configuration, ICustomLogger<PayrollService> logger, IHttpContextAccessor httpContextAccessor, IEmployeeInformationRepository employeeInformationRepository, IPaymentScheduleRepository paymentScheduleRepository)
        {
            _payrollRepository = payrollRepository;
            _paySlipRepository = paySlipRepository;
            _mapper = mapper;
            _configuration = configuration;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _employeeInformationRepository = employeeInformationRepository;
            _paymentScheduleRepository = paymentScheduleRepository;
        }

        public async Task<StandardResponse<PagedCollection<PayrollView>>> ListPayrolls(PagingOptions pagingOptions, Guid? employeeInformationId = null)
        {
            try
            {
                var payrolls = _payrollRepository.Query().Include(u => u.EmployeeInformation).ThenInclude(user => user.User).AsQueryable();

                if (employeeInformationId.HasValue)
                    payrolls = payrolls.Where(x => x.EmployeeInformationId == employeeInformationId);

                payrolls = payrolls.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var allPayrolls = new List<PayrollView>();

                foreach (var payroll in payrolls)
                {
                    var payrollView = new PayrollView
                    {
                        PayrollId = payroll.Id,
                        EmployeeInformationId = payroll.EmployeeInformationId,
                        Name = payroll.EmployeeInformation.User.FullName,
                        StartDate = payroll.StartDate,
                        EndDate = payroll.EndDate,
                        PaymentDate = payroll.PaymentDate,
                        TotalHours = payroll.TotalHours,
                        Rate = payroll.Rate,
                        TotalAmount = payroll.TotalAmount
                    };

                    allPayrolls.Add(payrollView);
                }

                var pagedCollection = PagedCollection<PayrollView>.Create(Link.ToCollection(nameof(FinancialController.ListPayrolls)), allPayrolls.ToArray(), allPayrolls.Count(), pagingOptions);
                return StandardResponse<PagedCollection<PayrollView>>.Ok(pagedCollection);

            }
            catch (Exception ex)
            {
                return _logger.Error<PagedCollection<PayrollView>>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<PagedCollection<PayrollView>>> ListPendingPayrolls(PagingOptions pagingOptions, Guid? employeeInformationId = null)
        {
            try
            {
                var payrolls = _payrollRepository.Query().Where(p => p.StatusId == (int)Statuses.PENDING).Include(u => u.EmployeeInformation).ThenInclude(user => user.User).AsQueryable();

                if (employeeInformationId.HasValue)
                    payrolls = payrolls.Where(x => x.EmployeeInformationId == employeeInformationId);

                payrolls = payrolls.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var allPayrolls = new List<PayrollView>();

                foreach (var payroll in payrolls)
                {
                    var payrollView = new PayrollView
                    {
                        PayrollId = payroll.Id,
                        EmployeeInformationId = payroll.EmployeeInformationId,
                        Name = payroll.EmployeeInformation.User.FullName,
                        StartDate = payroll.StartDate,
                        EndDate = payroll.EndDate,
                        PaymentDate = payroll.PaymentDate,
                        TotalHours = payroll.TotalHours,
                        Rate = payroll.Rate,
                        TotalAmount = payroll.TotalAmount
                    };

                    allPayrolls.Add(payrollView);
                }

                var pagedCollection = PagedCollection<PayrollView>.Create(Link.ToCollection(nameof(FinancialController.ListPayrolls)), allPayrolls.ToArray(), allPayrolls.Count(), pagingOptions);
                return StandardResponse<PagedCollection<PayrollView>>.Ok(pagedCollection);

            }
            catch (Exception ex)
            {
                return _logger.Error<PagedCollection<PayrollView>>(_logger.GetMethodName(), ex);
            }
        }
        public async Task<StandardResponse<PagedCollection<PayrollView>>> ListApprovedPayrolls(PagingOptions pagingOptions, Guid? employeeInformationId = null)
        {
            try
            {
                var payrolls = _payrollRepository.Query().Where(p => p.StatusId != (int)Statuses.PENDING).Include(u => u.EmployeeInformation).ThenInclude(user => user.User).AsQueryable();

                if (employeeInformationId.HasValue)
                    payrolls = payrolls.Where(x => x.EmployeeInformationId == employeeInformationId);

                payrolls = payrolls.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var allPayrolls = new List<PayrollView>();

                foreach (var payroll in payrolls)
                {
                    var payrollView = new PayrollView
                    {
                        PayrollId = payroll.Id,
                        EmployeeInformationId = payroll.EmployeeInformationId,
                        Name = payroll.EmployeeInformation.User.FullName,
                        StartDate = payroll.StartDate,
                        EndDate = payroll.EndDate,
                        PaymentDate = payroll.PaymentDate,
                        TotalHours = payroll.TotalHours,
                        Rate = payroll.Rate,
                        TotalAmount = payroll.TotalAmount
                    };

                    allPayrolls.Add(payrollView);
                }

                var pagedCollection = PagedCollection<PayrollView>.Create(Link.ToCollection(nameof(FinancialController.ListPayrolls)), allPayrolls.ToArray(), allPayrolls.Count(), pagingOptions);
                return StandardResponse<PagedCollection<PayrollView>>.Ok(pagedCollection);

            }
            catch (Exception ex)
            {
                return _logger.Error<PagedCollection<PayrollView>>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<bool>> ApprovePayroll(Guid payrollId)
        {
            try
            {
                var payroll = _payrollRepository.Query()
                .FirstOrDefault(payroll => payroll.Id == payrollId);

                if (payroll == null)
                    return StandardResponse<bool>.NotFound("No time sheet found for this user for the date requested");

                payroll.StatusId = (int)Statuses.APPROVED;
                _payrollRepository.Update(payroll);


                var response = GeneratePaySlip(payrollId).Result;

                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                return _logger.Error<bool>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<bool>> GeneratePaySlip(Guid payrollId)
        {
            try
            {
                var payroll = _payrollRepository.Query().Include(u => u.EmployeeInformation).ThenInclude(v => v.User)
                .FirstOrDefault(payroll => payroll.Id == payrollId && payroll.StatusId == (int)Statuses.APPROVED);

                if (payroll.EmployeeInformation == null)
                    return StandardResponse<bool>.NotFound("Employee information not found");

                var paySlip = new PaySlip
                {
                    EmployeeInformationId = payroll.EmployeeInformationId,
                    StartDate = payroll.StartDate,
                    EndDate = payroll.EndDate,
                    TotalHours = payroll.TotalHours,
                    TotalAmount = payroll.TotalAmount,
                    Rate = payroll.Rate.ToString(),
                    PaymentDate = payroll.PaymentDate,
                };
                paySlip = _paySlipRepository.CreateAndReturn(paySlip);

                return StandardResponse<bool>.Ok(true);

            }
            catch (Exception ex)
            {
                return _logger.Error<bool>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<PagedCollection<PaySlipView>>> ListPaySlips(PagingOptions pagingOptions, Guid? employeeInformationId = null, DateFilter dateFilter = null)
        {
            try
            {
                var paySlips = _paySlipRepository.Query().Include(user => user.EmployeeInformation).ThenInclude(user => user.User).AsQueryable();
                if (employeeInformationId.HasValue)
                    paySlips = paySlips.Where(x => x.EmployeeInformationId == employeeInformationId);

                if (dateFilter.StartDate.HasValue)
                    paySlips = paySlips.Where(u => u.DateCreated >= dateFilter.StartDate).OrderByDescending(u => u.DateCreated);

                if (dateFilter.EndDate.HasValue)
                    paySlips = paySlips.Where(u => u.DateCreated <= dateFilter.EndDate).OrderByDescending(u => u.DateCreated);

                var payslipo = paySlips.ToList();

                paySlips = paySlips.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);
                var allPaySlips = paySlips.ProjectTo<PaySlipView>(_configuration).ToArray();

                var pagedCollection = PagedCollection<PaySlipView>.Create(Link.ToCollection(nameof(FinancialController.ListPaySlips)), allPaySlips.ToArray(), allPaySlips.Count(), pagingOptions);
                return StandardResponse<PagedCollection<PaySlipView>>.Ok(pagedCollection);

            }
            catch (Exception ex)
            {
                return _logger.Error<PagedCollection<PaySlipView>>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<PagedCollection<PayrollView>>> ListPayrollsByPaymentPartner(PagingOptions pagingOptions)
        {
            try
            {
                var loggedInUser = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();
                var payrolls = _payrollRepository.Query().Include(u => u.EmployeeInformation).ThenInclude(user => user.User).AsQueryable();

                payrolls = payrolls.Where(x => x.EmployeeInformation.PaymentPartnerId == loggedInUser);

                var pagedPayrolls = payrolls.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var allPayrolls = pagedPayrolls.ProjectTo<PayrollView>(_configuration).ToArray();

                var pagedCollection = PagedCollection<PayrollView>.Create(Link.ToCollection(nameof(FinancialController.ListPayrollsByPaymentPartner)), allPayrolls.ToArray(), allPayrolls.Count(), pagingOptions);
                return StandardResponse<PagedCollection<PayrollView>>.Ok(pagedCollection);

            }
            catch (Exception ex)
            {
                return _logger.Error<PagedCollection<PayrollView>>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<PagedCollection<PayrollView>>> ListClientTeamMembersPayroll(PagingOptions pagingOptions)
        {
            try
            {
                var loggedInUser = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();
                var payrolls = _payrollRepository.Query().Include(u => u.EmployeeInformation).ThenInclude(user => user.User).
                    Where(payroll => payroll.EmployeeInformation.Supervisor.ClientId == loggedInUser).AsQueryable();

                var pagedPayrolls = payrolls.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var allPayrolls = pagedPayrolls.ProjectTo<PayrollView>(_configuration).ToArray();

                var pagedCollection = PagedCollection<PayrollView>.Create(Link.ToCollection(nameof(FinancialController.ListClientTeamMembersPayroll)), allPayrolls.ToArray(), allPayrolls.Count(), pagingOptions);
                return StandardResponse<PagedCollection<PayrollView>>.Ok(pagedCollection);

            }
            catch (Exception ex)
            {
                return _logger.Error<PagedCollection<PayrollView>>(_logger.GetMethodName(), ex);
            }
        }


        /// <summary>
        /// List all payslips for a team member with pagination
        /// </summary>
        /// <param name="pagingOptions"></param>
        /// <param name="search"></param>
        /// <returns>PagedCollection<PaySlipView></returns>
        public async Task<StandardResponse<PagedCollection<PaySlipView>>> ListPaySlipsByTeamMember(PagingOptions pagingOptions, string search = null)
        {
            try
            {
                var loggedInUser = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<Guid>();
                var employeeInformationId = _employeeInformationRepository.Query().FirstOrDefault(e => e.UserId == loggedInUser).Id;
                var paySlips = _paySlipRepository.Query().Where(u => u.EmployeeInformationId == employeeInformationId);


                var pagedPaySlips = paySlips.Skip(pagingOptions.Offset.Value).Take(pagingOptions.Limit.Value);

                var allPaySlips = pagedPaySlips.ProjectTo<PaySlipView>(_configuration).ToArray();

                var pagedCollection = PagedCollection<PaySlipView>.Create(Link.ToCollection(nameof(FinancialController.ListPaySlips)), allPaySlips.ToArray(), allPaySlips.Count(), pagingOptions);
                return StandardResponse<PagedCollection<PaySlipView>>.Ok(pagedCollection);

            }
            catch (Exception ex)
            {
                return _logger.Error<PagedCollection<PaySlipView>>(_logger.GetMethodName(), ex);
            }
        }

        /// <summary>
        /// Generate a payment schedule for the year where the the last day of the cycle if  the 4th friday of every cycle
        /// </summary>
        /// <param name="year"></param>
        /// <returns>object</returns>
        //public async Task<StandardResponse<object>> GenerateMonthlyPaymentSchedule(int year)
        //{
        //    try
        //    {
        //        var paymentSchedule = new List<PaymentSchedule>();
        //        var startDate = new DateTime(year, 1, 1);
        //        var endDate = new DateTime(year, 12, 31);
        //        var cycle = 1;
        //        var paymentDate = startDate;
        //        while (paymentDate <= endDate)
        //        {
        //            var paymentScheduleItem = new PaymentSchedule
        //            {
        //                Cycle = cycle,
        //                WeekDate = paymentDate,
        //                LastWorkDayOfCycle = GetFridayOfWeek(paymentDate.AddDays(28 - 5)),
        //                ApprovalDate = GetMondayOfWeek(paymentDate.AddDays(28)),
        //                PaymentDate = GetFridayOfWeek(paymentDate.AddDays(28)),
        //                DateCreated = DateTime.Now,
        //                CycleType = "Monthly"
        //            };
        //            paymentSchedule.Add(paymentScheduleItem);
        //            cycle++;
        //            paymentDate = paymentDate.AddDays(28);
        //        }

        //        paymentSchedule.ForEach(x =>
        //        {
        //            _paymentScheduleRepository.CreateAndReturn(x);
        //        });

        //        return StandardResponse<object>.Ok(paymentSchedule);
        //    }
        //    catch (Exception ex)
        //    {
        //        return _logger.Error<object>(_logger.GetMethodName(), ex);
        //    }
        //}

        /// <summary>
        /// Generate biweekly payment schedule where the every 2 fridays is a cycle
        /// </summary>
        /// <param name="year"></param>
        /// <returns>object</returns>
        public async Task<StandardResponse<object>> GenerateBiWeeklyPaymentSchedule(int year)
        {
            try
            {
                var paymentSchedule = new List<PaymentSchedule>();
                var startDate = new DateTime(year, 1, 1);
                var endDate = new DateTime(year, 12, 31);
                var cycle = 1;
                var paymentDate = startDate;
                while (paymentDate <= endDate)
                {
                    var paymentScheduleItem = new PaymentSchedule
                    {
                        Cycle = cycle,
                        WeekDate = paymentDate,
                        LastWorkDayOfCycle = GetFridayOfWeek(paymentDate.AddDays(14 - 5)),
                        ApprovalDate = GetMondayOfWeek(paymentDate.AddDays(14)),
                        PaymentDate = GetFridayOfWeek(paymentDate.AddDays(14)),
                        DateCreated = DateTime.Now,
                        CycleType = "Bi-Weekly"
                    };
                    paymentSchedule.Add(paymentScheduleItem);
                    cycle++;
                    paymentDate = paymentDate.AddDays(14);
                }

                paymentSchedule.ForEach(x =>
                {
                    _paymentScheduleRepository.CreateAndReturn(x);
                });

                return StandardResponse<object>.Ok(paymentSchedule);
            }
            catch (Exception ex)
            {
                return _logger.Error<object>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<object>> GenerateCustomBiWeeklyPaymentSchedule(PayScheduleGenerationModel model)
        {
            try
            {
                var biWeeklyPaySchedule = _paymentScheduleRepository.Query().Where(x => x.CycleType.ToLower() == "bi-weekly" && x.SuperAdminId == model.SuperAdminId).ToList();

                if (biWeeklyPaySchedule.Count > 0)
                {
                    biWeeklyPaySchedule.ForEach(x =>
                    {
                        _paymentScheduleRepository.Delete(x);
                    });
                }

                if(model.StartDate.Year != DateTime.Now.Year) return StandardResponse<object>.Failed("You need to enter a date in the current year");
                var paymentSchedule = new List<PaymentSchedule>();
                var endDate = new DateTime(model.StartDate.Year, 12, 31);
                var cycle = 1;
                var paymentDate = model.StartDate;
                while (paymentDate <= endDate)
                {
                    var paymentScheduleItem = new PaymentSchedule
                    {
                        Cycle = cycle,
                        WeekDate = paymentDate,
                        LastWorkDayOfCycle = GetFridayOfWeek(paymentDate.AddDays(14 - 5)),
                        ApprovalDate = GetMondayOfWeek(paymentDate.AddDays(14)),
                        PaymentDate = GetFridayOfWeek(paymentDate.AddDays(14 - 5)).AddDays(model.PaymentDateDays),
                        DateCreated = DateTime.Now,
                        CycleType = "Bi-Weekly",
                        SuperAdminId = model.SuperAdminId
                    };
                    paymentSchedule.Add(paymentScheduleItem);
                    cycle++;
                    paymentDate = paymentDate.AddDays(14);
                }

                paymentSchedule.ForEach(x =>
                {
                    _paymentScheduleRepository.CreateAndReturn(x);
                });

                return StandardResponse<object>.Ok(paymentSchedule);
            }
            catch (Exception ex)
            {
                return _logger.Error<object>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<object>> GenerateCustomWeeklyPaymentSchedule(PayScheduleGenerationModel model)
        {
            try
            {
                var weeklyPaySchedule = _paymentScheduleRepository.Query().Where(x => x.CycleType.ToLower() == "weekly" && x.SuperAdminId == model.SuperAdminId).ToList();

                if (weeklyPaySchedule.Count > 0)
                {
                    weeklyPaySchedule.ForEach(x =>
                    {
                        _paymentScheduleRepository.Delete(x);
                    });
                }

                if (model.StartDate.Year != DateTime.Now.Year) return StandardResponse<object>.Failed("You need to enter a date in the current year");
                var paymentSchedule = new List<PaymentSchedule>();
                var endDate = new DateTime(model.StartDate.Year, 12, 31);
                var cycle = 1;
                var paymentDate = model.StartDate;
                while (paymentDate <= endDate)
                {
                    var paymentScheduleItem = new PaymentSchedule
                    {
                        Cycle = cycle,
                        WeekDate = paymentDate,
                        LastWorkDayOfCycle = GetFridayOfWeek(paymentDate.AddDays(7 - 5)),
                        ApprovalDate = GetMondayOfWeek(paymentDate.AddDays(7)),
                        PaymentDate = GetFridayOfWeek(paymentDate.AddDays(7)).AddDays(model.PaymentDateDays),
                        DateCreated = DateTime.Now,
                        CycleType = "Weekly",
                        SuperAdminId = model.SuperAdminId
                    };
                    paymentSchedule.Add(paymentScheduleItem);
                    cycle++;
                    paymentDate = paymentDate.AddDays(7);
                }

                paymentSchedule.ForEach(x =>
                {
                    _paymentScheduleRepository.CreateAndReturn(x);
                });

                return StandardResponse<object>.Ok(paymentSchedule);
            }
            catch (Exception ex)
            {
                return _logger.Error<object>(_logger.GetMethodName(), ex);
            }
        }

        /// <summary>
        /// Generate weekly payment schedule where the every fridays is a cycle
        /// </summary>
        /// <param name="year"></param>
        /// <returns>object</returns>
        public async Task<StandardResponse<object>> GenerateWeeklyPaymentSchedule(int year)
        {
            try
            {
                var paymentSchedule = new List<PaymentSchedule>();
                var startDate = new DateTime(year, 1, 1);
                var endDate = new DateTime(year, 12, 31);
                var cycle = 1;
                var paymentDate = startDate;
                while (paymentDate <= endDate)
                {
                    var paymentScheduleItem = new PaymentSchedule
                    {
                        Cycle = cycle,
                        WeekDate = paymentDate,
                        LastWorkDayOfCycle = GetFridayOfWeek(paymentDate.AddDays(7 - 5)),
                        ApprovalDate = GetMondayOfWeek(paymentDate.AddDays(7)),
                        PaymentDate = GetFridayOfWeek(paymentDate.AddDays(7)),
                        DateCreated = DateTime.Now,
                        CycleType = "Weekly",
                    };
                    paymentSchedule.Add(paymentScheduleItem);
                    cycle++;
                    paymentDate = paymentDate.AddDays(7);
                }

                paymentSchedule.ForEach(x =>
                {
                    _paymentScheduleRepository.CreateAndReturn(x);
                });

                return StandardResponse<object>.Ok(paymentSchedule);
            }
            catch (Exception ex)
            {
                return _logger.Error<object>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<List<EmployeePayScheduleView>>> GetEmployeePaySchedule(Guid employeeInformationId)
        {
            try
            {
                var employee = _employeeInformationRepository.Query().Include(x => x.User).FirstOrDefault(x => x.Id == employeeInformationId);
                if (employee == null) return StandardResponse<List<EmployeePayScheduleView>>.NotFound("Employee information not found");

                var schedules = _paymentScheduleRepository.Query().Where(x => x.SuperAdminId == employee.User.SuperAdminId && x.CycleType.ToLower() == employee.PaymentFrequency.ToLower()).ToList();

                var userSchedules = new List<EmployeePayScheduleView>();

                schedules.ForEach(x =>
                {
                    var schedule = new EmployeePayScheduleView { CycleId = x.Id, DateRange = $"{x.WeekDate.ToString()}-{x.LastWorkDayOfCycle.ToString()}" };
                    userSchedules.Add(schedule);
                });

                return StandardResponse<List<EmployeePayScheduleView>>.Ok(userSchedules);
            }
            catch(Exception ex)
            {
                return _logger.Error<List<EmployeePayScheduleView>>(_logger.GetMethodName(), ex);
            }
        }

        // Get the friday of a week date
        public static DateTime GetFridayOfWeek(DateTime date)
        {
            var dayOfWeek = (int)date.DayOfWeek;
            var daysToAdd = (int)DayOfWeek.Friday - dayOfWeek;
            return date.AddDays(daysToAdd);
        }

        // Get monday of a week date
        public static DateTime GetMondayOfWeek(DateTime date)
        {
            var dayOfWeek = (int)date.DayOfWeek;
            var daysToAdd = (int)DayOfWeek.Monday - dayOfWeek;
            return date.AddDays(daysToAdd);
        }

        //Get firstday of a month
        public static DateTime FirstDayOfMonth(DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        public static DateTime LastDayOfMonth(DateTime date)
        {
            return FirstDayOfMonth(date)
                .AddMonths(1)
                .AddMinutes(-1);
        }


        public async Task<StandardResponse<object>> GenerateMonthlyPaymentSchedule(int year)
        {
            try
            {
                var paymentSchedule = new List<PaymentSchedule>();
                var startDate = new DateTime(year, 1, 1);
                var endDate = new DateTime(year, 12, 31);
                var cycle = 1;
                var paymentDate = startDate;
                while (paymentDate <= endDate)
                {
                    var paymentScheduleItem = new PaymentSchedule
                    {
                        Cycle = cycle,
                        WeekDate = paymentDate,
                        LastWorkDayOfCycle = GetFridayOfWeek(paymentDate.AddDays(24)),
                        ApprovalDate = GetMondayOfWeek(paymentDate.AddDays(24 + 5)),
                        PaymentDate = GetFridayOfWeek(paymentDate.AddDays(24 + 5)),
                        DateCreated = DateTime.Now,
                        CycleType = "Monthly"
                    };
                    paymentSchedule.Add(paymentScheduleItem);
                    cycle++;
                    paymentDate = paymentDate.AddDays(30);
                }

                paymentSchedule.ForEach(x =>
                {
                    _paymentScheduleRepository.CreateAndReturn(x);
                });

                return StandardResponse<object>.Ok(paymentSchedule);
            }
            catch (Exception ex)
            {
                return _logger.Error<object>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<object>> GenerateCustomMonthlyPaymentScheduleWeekPeriod(PayScheduleGenerationModel model)
        {
            try
            {
                var monthlyPaySchedule = _paymentScheduleRepository.Query().Where(x => x.CycleType.ToLower() == "monthly" && x.SuperAdminId == model.SuperAdminId).ToList();

                if (monthlyPaySchedule.Count > 0)
                {
                    monthlyPaySchedule.ForEach(x =>
                    {
                        _paymentScheduleRepository.Delete(x);
                    });
                }

                var paymentSchedule = new List<PaymentSchedule>();

                var currentDate = DateTime.Now;

                var endDate = new DateTime(currentDate.Year, 12, 31);

                if (model.StartDate.Year != DateTime.Now.Year) return StandardResponse<object>.Failed("You need to enter a date in the current year");
                var cycle = 1;
                var paymentDate = model.StartDate;
                while (paymentDate <= endDate)
                {
                    var paymentScheduleItem = new PaymentSchedule
                    {
                        Cycle = cycle,
                        WeekDate = paymentDate,
                        LastWorkDayOfCycle = GetFridayOfWeek(paymentDate.AddDays(28-5)),
                        ApprovalDate = GetMondayOfWeek(paymentDate.AddDays(28)),
                        PaymentDate = GetFridayOfWeek(paymentDate.AddDays(28 - 5)).AddDays(model.PaymentDateDays),
                        DateCreated = DateTime.Now,
                        CycleType = "Monthly",
                        SuperAdminId = model.SuperAdminId
                    };
                    paymentSchedule.Add(paymentScheduleItem);
                    cycle++;
                    paymentDate = paymentDate.AddDays(28);
                }

                paymentSchedule.ForEach(x =>
                {
                    _paymentScheduleRepository.CreateAndReturn(x);
                });

                return StandardResponse<object>.Ok(paymentSchedule);
            }
            catch (Exception ex)
            {
                return _logger.Error<object>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<object>> GenerateCustomFullMonthPaymentSchedule(int paymentDay, Guid superAdminId)
        {
            try
            {
                var monthlyPaySchedule = _paymentScheduleRepository.Query().Where(x => x.CycleType.ToLower() == "monthly" && x.SuperAdminId == superAdminId).ToList();

                if (monthlyPaySchedule.Count > 0)
                {
                    monthlyPaySchedule.ForEach(x =>
                    {
                        _paymentScheduleRepository.Delete(x);
                    });
                }

                var paymentSchedule = new List<PaymentSchedule>();

                var currentDate = DateTime.Now;

                var startDate = new DateTime(currentDate.Year, 1, 1);

                var endDate = new DateTime(currentDate.Year, 12, 31);

                var cycle = 1;

                var paymentDate = startDate;

                while (paymentDate <= endDate)
                {
                    var paymentScheduleItem = new PaymentSchedule
                    {
                        Cycle = cycle,
                        WeekDate = paymentDate,
                        LastWorkDayOfCycle = LastDayOfMonth(paymentDate),
                        ApprovalDate = new DateTime(paymentDate.Year, paymentDate.Month, paymentDay).AddDays(-3),
                        PaymentDate = new DateTime(paymentDate.Year, paymentDate.Month, paymentDay),
                        DateCreated = DateTime.Now,
                        CycleType = "Monthly",
                        SuperAdminId = superAdminId
                    };
                    paymentSchedule.Add(paymentScheduleItem);
                    cycle++;
                    paymentDate = paymentDate.AddMonths(1);
                }

                paymentSchedule.ForEach(x =>
                {
                    _paymentScheduleRepository.CreateAndReturn(x);
                });

                return StandardResponse<object>.Ok(paymentSchedule);
            }
            catch (Exception ex)
            {
                return _logger.Error<object>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<object>> GetMonthlyPaySchedule(Guid superAdminId)
        {
            try
            {
                var monthlyPaySchedule = _paymentScheduleRepository.Query().Where(x => x.CycleType.ToLower() == "monthly" && x.SuperAdminId == superAdminId).ToList();
                return StandardResponse<object>.Ok(monthlyPaySchedule);
            }
            catch(Exception ex)
            {
                return _logger.Error<object>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<object>> GetBiWeeklyPaySchedule(Guid superAdminId)
        {
            try
            {
                var monthlyPaySchedule = _paymentScheduleRepository.Query().Where(x => x.CycleType.ToLower() == "bi-weekly" && x.SuperAdminId == superAdminId).ToList();
                return StandardResponse<object>.Ok(monthlyPaySchedule);
            }
            catch (Exception ex)
            {
                return _logger.Error<object>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<object>> GetWeeklyPaySchedule(Guid superAdminId)
        {
            try
            {
                var monthlyPaySchedule = _paymentScheduleRepository.Query().Where(x => x.CycleType.ToLower() == "weekly" && x.SuperAdminId == superAdminId).ToList();
                return StandardResponse<object>.Ok(monthlyPaySchedule);
            }
            catch (Exception ex)
            {
                return _logger.Error<object>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<object>> GetPayScheduleInAMonth(Guid employeeInformationId, DateTime date)
        {
            try
            {
                var employee = _employeeInformationRepository.Query().Include(x => x.User).FirstOrDefault(x => x.Id == employeeInformationId);
                var schedules = _paymentScheduleRepository.Query().Where(x => x.WeekDate.Month == date.Month && x.LastWorkDayOfCycle.Month == date.Month &&
                x.SuperAdminId == employee.User.SuperAdminId && x.CycleType.ToLower() == employee.PaymentFrequency.ToLower()).ToList();

                //if(employee.PaymentFrequency.ToLower() == "weekly")
                //{
                //    schedules = _paymentScheduleRepository.Query().Where(x => x.WeekDate.Date.Month == DateTime.Now.Date.Month && x.LastWorkDayOfCycle.Month == DateTime.Now.Month
                //     && x.LastWorkDayOfCycle.Month == DateTime.Now.Month && x.CycleType.ToLower() == employee.PaymentFrequency.ToLower()).ToList();
                //}

                return StandardResponse<object>.Ok(schedules);

            }
            catch (Exception ex)
            {
                return _logger.Error<object>(_logger.GetMethodName(), ex);
            }
        }


        // This presumes that weeks start with Monday.
        // Week 1 is the 1st week of the year with a Thursday in it.
        public static int GetIso8601WeekOfYear(DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        public async Task<StandardResponse<List<PaymentSchedule>>> GetPaymentSchedule(Guid employeeInformationId)
        {
            try
            {

                var employee = _employeeInformationRepository.Query().Include(x => x.User).FirstOrDefault(x => x.Id == employeeInformationId);
                if (employee == null) return StandardResponse<List<PaymentSchedule>>.NotFound("Employee information not found");

                var schedules = _paymentScheduleRepository.Query().Where(x => x.SuperAdminId == employee.User.SuperAdminId && x.CycleType.ToLower() == employee.PaymentFrequency.ToLower()).ToList();

                return StandardResponse<List<PaymentSchedule>>.Ok(schedules);
            }
            catch (Exception ex)
            {
                return _logger.Error<List<PaymentSchedule>>(_logger.GetMethodName(), ex);
            }
        }

        public async Task<StandardResponse<List<AdminPaymentScheduleView>>> GetPaymentSchedules(Guid superAdminId)
        {
            try
            {
                //var paymentSchedules = _paymentScheduleRepository.Query().Where(x => x.PaymentDate.Year == DateTime.Now.Year).ToList();
                var paymentSchedules = _paymentScheduleRepository.Query().Where(x => x.SuperAdminId == superAdminId).ToList();
                var grouped = paymentSchedules.GroupBy(x => x.CycleType).ToList();

                var adminPaymentScheduleViews = new List<AdminPaymentScheduleView>();

                grouped.ForEach(x =>
                {
                    var adminPaymentScheduleView = new AdminPaymentScheduleView
                    {
                        Schedules = x.ToList(),
                        ScheduleType = x.FirstOrDefault().CycleType
                    };
                    adminPaymentScheduleViews.Add(adminPaymentScheduleView);
                });
                
                return StandardResponse<List<AdminPaymentScheduleView>>.Ok(adminPaymentScheduleViews);
            }
            catch (Exception ex)
            {
                return _logger.Error<List<AdminPaymentScheduleView>>(_logger.GetMethodName(), ex);
            }
        }

        /// <summary>
        /// Get current cycle of year from current date and cycle type
        /// </summary>
        /// <param name="cycleType"></param>
        /// <param name="date"></param>
        /// <returns>int</returns> 
        public PaymentSchedule GetCurrentCyclePaymentSchedule(string cycleType, DateTime date)
        {
            var currentCycle = _paymentScheduleRepository.Query().Where(x => x.CycleType.ToLower() == cycleType.ToLower() && date >= x.WeekDate && date <= x.LastWorkDayOfCycle).FirstOrDefault();
            return currentCycle;
        }



    }
}
