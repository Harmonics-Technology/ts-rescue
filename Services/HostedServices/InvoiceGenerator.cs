using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Models.IdentityModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Repositories.Interfaces;
using TimesheetBE.Services.Interfaces;
using TimesheetBE.Utilities.Abstrctions;

namespace TimesheetBE.Services.HostedServices
{
    public class InvoiceGenerator : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private Timer _timer;
        DateTime aliveSince;
        public IServiceProvider Services { get; }
        private ILogger<InvoiceGenerator> _logger;
        public InvoiceGenerator(IServiceProvider services, ILogger<InvoiceGenerator> logger)
        {
            Services = services;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            aliveSince = DateTime.Now;
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            try
            {
                var runJobs = new ThreadStart(() =>
                {
                    try
                    {
                        var count = Interlocked.Increment(ref executionCount);
                        using (var scope = Services.CreateScope())
                        {
                            var _webHostEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
                            var _userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                            var _timeSheetRepository = scope.ServiceProvider.GetRequiredService<ITimeSheetRepository>();
                            var _timeSheetService = scope.ServiceProvider.GetRequiredService<ITimeSheetService>();
                            var _paymentScheduleRepository = scope.ServiceProvider.GetRequiredService<IPaymentScheduleRepository>();
                            var _codeProvider = scope.ServiceProvider.GetRequiredService<ICodeProvider>();
                            var _invoiceRepository = scope.ServiceProvider.GetRequiredService<IInvoiceRepository>();
                            var _expenseRepository = scope.ServiceProvider.GetRequiredService<IExpenseRepository>();
                            var _notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                            var allUsers = _userRepository.Query().Include(user => user.EmployeeInformation).Where(user => user.Role.ToLower() == "team member" || user.Role.ToLower() == "internal supervisor" || user.Role.ToLower() == "internal admin").ToList();

                            //var allUsers = _userRepository.Query().Include(user => user.EmployeeInformation).Where(user => user.Email == "dy.oungdavids@gmail.com").ToList();

                            foreach (var user in allUsers)
                            {
                                //Generate invoices for users base on their payment frequency
                                GenerateIvoiceForWeeklyScheduleUser(_invoiceRepository, _timeSheetRepository, user, _paymentScheduleRepository, _codeProvider, _expenseRepository, _timeSheetService, _userRepository, _notificationService);
                            }


                        }
                    }
                    catch (System.Exception)
                    {

                        throw;
                    }
                });
                new Thread(runJobs).Start();

            }
            catch (Exception ex)
            {

            }

        }

        private void GenerateIvoiceForWeeklyScheduleUser(IInvoiceRepository _invoiceRepository, ITimeSheetRepository _timeSheetRepository, User user, 
            IPaymentScheduleRepository _paymentScheduleRepository, ICodeProvider _codeProvider, IExpenseRepository _expenseRepository, ITimeSheetService _timeSheetService, IUserRepository _userRepository, INotificationService _notificationService)
        {

            var getAdmins = _userRepository.Query().Where(x => x.Role.ToLower() == "payroll manager").ToList();

            int[] allMonth = new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 9, 10, 11, 12};

            var currentPaySchedule = _paymentScheduleRepository.Query().FirstOrDefault(x => x.CycleType.ToLower() == user.EmployeeInformation.PaymentFrequency.ToLower() && x.WeekDate.Date.Date <= DateTime.Now.Date && DateTime.Now.Date <= x.LastWorkDayOfCycle.Date && x.SuperAdminId == user.SuperAdminId);

            foreach (var month in allMonth)
            {
                //var thmo = DateTime.Now.ToString("MMMM");
                var monthlyPaySchedule = _paymentScheduleRepository.Query().Where(schedule => schedule.LastWorkDayOfCycle.Month == month && schedule.WeekDate < schedule.LastWorkDayOfCycle).ToList();

                if (user?.EmployeeInformation?.PaymentFrequency == null) continue;

                if(user?.EmployeeInformation?.EnableFinancials == false) continue;

                switch (user?.EmployeeInformation?.PaymentFrequency.ToLower())
                {
                    case "weekly":

                        var weeklyPaymentSchedule = monthlyPaySchedule.Where(schedule => schedule.CycleType == "Weekly" && schedule.SuperAdminId == user.SuperAdminId);

                        //var monthy = weeklyPaymentSchedule.ToList();
                        foreach (var schedule in weeklyPaymentSchedule)
                        {
                            if (DateTime.Now <= schedule.LastWorkDayOfCycle)
                                break;
                            var timesheets = _timeSheetRepository.Query().Where(timesheet => schedule.WeekDate.Date <= timesheet.Date.Date && timesheet.Date.Date <= schedule.LastWorkDayOfCycle.Date && timesheet.Date.DayOfWeek != DayOfWeek.Saturday && timesheet.Date.DayOfWeek != DayOfWeek.Sunday && timesheet.EmployeeInformationId == user.EmployeeInformationId).ToList();

                            var expenses = _expenseRepository.Query().Where(expense => expense.TeamMemberId == user.Id && expense.StatusId == (int)Statuses.APPROVED && expense.IsInvoiced == false).ToList();

                            if (timesheets.Count() > 0) {
                                if (!timesheets.Any(x => x.StatusId == (int)Statuses.REJECTED || x.StatusId == (int)Statuses.PENDING))
                                {
                                    var invoice = _invoiceRepository.Query().FirstOrDefault(invoice => invoice.StartDate.Date == schedule.WeekDate.Date && invoice.EndDate.Date == schedule.LastWorkDayOfCycle.Date && invoice.EmployeeInformationId == user.EmployeeInformationId);
                                    
                                    if (invoice == null)
                                    {
                                        var totalHourss = timesheets.Sum(timesheet => timesheet.Hours);
                                        var invoiceCount = _invoiceRepository.Query().Include(x => x.CreatedByUser).Where(x => x.CreatedByUser.SuperAdminId == user.SuperAdminId).Count(); 
                                        invoice = new Invoice
                                        {
                                            EmployeeInformationId = (Guid)user.EmployeeInformationId,
                                            StartDate = schedule.WeekDate,
                                            EndDate = schedule.LastWorkDayOfCycle,
                                            PaymentDate = currentPaySchedule != null ? currentPaySchedule.PaymentDate : DateTime.Now.Date,
                                            InvoiceReference = invoiceCount == 0 ? $"INV{1:0000}" : $"INV{invoiceCount + 1:0000}",
                                            TotalHours = totalHourss,
                                            TotalAmount = user.EmployeeInformation.PayRollTypeId == 1 ? totalHourss * user.EmployeeInformation.RatePerHour : Convert.ToDouble(_timeSheetService.GetOffshoreTeamMemberTotalPay(user.EmployeeInformationId, schedule.WeekDate, schedule.LastWorkDayOfCycle, totalHourss, 1)),
                                            StatusId = user.EmployeeInformation.InvoiceGenerationType.ToLower() == "invoice" ? (int)Statuses.PENDING : (int)Statuses.SUBMITTED,
                                            CreatedByUserId = user.Id,
                                            InvoiceTypeId = (int)InvoiceTypes.PAYROLL
                                        };
                                        _invoiceRepository.CreateAndReturn(invoice);

                                        if (expenses.Count() > 0)
                                        {
                                            var newInvoice = _invoiceRepository.Query().FirstOrDefault(invoice => invoice.StartDate.Date == schedule.WeekDate.Date && invoice.EndDate.Date == schedule.LastWorkDayOfCycle.Date && invoice.EmployeeInformationId == user.EmployeeInformationId);
                                            foreach (var expense in expenses)
                                            {
                                                expense.InvoiceId = newInvoice.Id;
                                                expense.IsInvoiced = true;
                                                _expenseRepository.Update(expense);
                                            }

                                            var totalExpenseAmount = _expenseRepository.Query().Where(expense => expense.InvoiceId == newInvoice.Id).Sum(expense => expense.Amount);
                                            newInvoice.TotalAmount = newInvoice.TotalAmount + Convert.ToDouble(totalExpenseAmount);
                                            _invoiceRepository.Update(newInvoice);

                                        }

                                        if(user.EmployeeInformation.PayRollTypeId == 2)
                                        {
                                            foreach (var admin in getAdmins)
                                            {
                                                _notificationService.SendNotification(new NotificationModel { UserId = admin.Id, Title = "Pending Invoice", Type = "Notification", Message = "A new item has been added to an invoice awaiting your approval." });
                                            }
                                        }
                                    }
                                }
                            }
                            
                        }
                            
                        break;

                    case "bi-weekly":
                        var biWeeklyPaymentSchedule = monthlyPaySchedule.Where(schedule => schedule.CycleType == "Bi-Weekly" && schedule.SuperAdminId == user.SuperAdminId);
                        foreach (var schedule in biWeeklyPaymentSchedule)
                        {
                            if (DateTime.Now <= schedule.LastWorkDayOfCycle)
                                break;
                            var timesheets = _timeSheetRepository.Query().Where(timesheet =>  schedule.WeekDate.Date <= timesheet.Date.Date && timesheet.Date.Date <= schedule.LastWorkDayOfCycle.Date && timesheet.Date.DayOfWeek != DayOfWeek.Saturday && timesheet.Date.DayOfWeek != DayOfWeek.Sunday && timesheet.EmployeeInformationId == user.EmployeeInformationId).ToList();

                            var expenses = _expenseRepository.Query().Where(expense => expense.TeamMemberId == user.Id && expense.StatusId == (int)Statuses.APPROVED && expense.IsInvoiced == false).ToList();

                            if(timesheets.Count() > 0)
                            {
                                if (!timesheets.Any(x => x.StatusId == (int)Statuses.REJECTED || x.StatusId == (int)Statuses.PENDING))
                                {
                                    var invoice = _invoiceRepository.Query().FirstOrDefault(invoice => invoice.StartDate.Date == schedule.WeekDate.Date && invoice.EndDate.Date == schedule.LastWorkDayOfCycle.Date && invoice.EmployeeInformationId == user.EmployeeInformationId);
                                    var invoiceCount = _invoiceRepository.Query().Include(x => x.CreatedByUser).Where(x => x.CreatedByUser.SuperAdminId == user.SuperAdminId).Count();
                                    if (invoice == null)
                                    {
                                        var totalHourss = timesheets.Sum(timesheet => timesheet.Hours);
                                        invoice = new Invoice
                                        {
                                            EmployeeInformationId = (Guid)user.EmployeeInformationId,
                                            StartDate = schedule.WeekDate,
                                            EndDate = schedule.LastWorkDayOfCycle,
                                            PaymentDate = currentPaySchedule != null ? currentPaySchedule.PaymentDate : DateTime.Now.Date,
                                            InvoiceReference = invoiceCount == 0 ? $"INV{1:0000}" : $"INV{invoiceCount + 1:0000}",
                                            TotalHours = totalHourss,
                                            TotalAmount = user.EmployeeInformation.PayRollTypeId == 1 ? totalHourss * user.EmployeeInformation.RatePerHour : Convert.ToDouble(_timeSheetService.GetOffshoreTeamMemberTotalPay(user.EmployeeInformationId, schedule.WeekDate, schedule.LastWorkDayOfCycle, totalHourss, 1)),
                                            StatusId = user.EmployeeInformation.InvoiceGenerationType.ToLower() == "invoice" ? (int)Statuses.PENDING : (int)Statuses.SUBMITTED,
                                            CreatedByUserId = user.Id,
                                            InvoiceTypeId = (int)InvoiceTypes.PAYROLL,
                                        };
                                        _invoiceRepository.CreateAndReturn(invoice);

                                        if (expenses.Count() > 0)
                                        {
                                            var newInvoice = _invoiceRepository.Query().FirstOrDefault(invoice => invoice.StartDate.Date == schedule.WeekDate.Date && invoice.EndDate.Date == schedule.LastWorkDayOfCycle.Date && invoice.EmployeeInformationId == user.EmployeeInformationId);
                                            foreach (var expense in expenses)
                                            {
                                                expense.InvoiceId = newInvoice.Id;
                                                expense.IsInvoiced = true;
                                                _expenseRepository.Update(expense);
                                            }

                                            var totalExpenseAmount = _expenseRepository.Query().Where(expense => expense.InvoiceId == newInvoice.Id).Sum(expense => expense.Amount);
                                            newInvoice.TotalAmount = newInvoice.TotalAmount + Convert.ToDouble(totalExpenseAmount);
                                            _invoiceRepository.Update(newInvoice);

                                        }

                                        if (user.EmployeeInformation.PayRollTypeId == 2)
                                        {
                                            foreach (var admin in getAdmins)
                                            {
                                                _notificationService.SendNotification(new NotificationModel { UserId = admin.Id, Title = "Pending Invoice", Type = "Notification", Message = "A new item has been added to an invoice awaiting your approval." });
                                            }
                                        }
                                    }
                                }
                            }
                            
                        }

                        break;

                    case "monthly":
                        var monthlyPaymentSchedule = monthlyPaySchedule.Where(schedule => schedule.CycleType == "Monthly" && schedule.SuperAdminId == user.SuperAdminId);
                        foreach (var schedule in monthlyPaymentSchedule)
                        {
                            if (DateTime.Now <= schedule.LastWorkDayOfCycle)
                                break;
                            var timesheets = _timeSheetRepository.Query().Where(timesheet => schedule.WeekDate.Date <= timesheet.Date.Date && timesheet.Date.Date <= schedule.LastWorkDayOfCycle.Date && timesheet.Date.DayOfWeek != DayOfWeek.Saturday && timesheet.Date.DayOfWeek != DayOfWeek.Sunday && timesheet.EmployeeInformationId == user.EmployeeInformationId).ToList();

                            var expenses = _expenseRepository.Query().Where(expense => expense.TeamMemberId == user.Id && expense.StatusId == (int)Statuses.APPROVED && expense.IsInvoiced == false).ToList(); // && schedule.WeekDate.Date <= expense.DateCreated.Date && expense.DateCreated.Date <= schedule.LastWorkDayOfCycle.Date).ToList();
                            if (timesheets.Count() > 0)
                            {
                                if (!timesheets.Any(x => x.StatusId == (int)Statuses.REJECTED || x.StatusId == (int)Statuses.PENDING))
                                {
                                    var invoice = _invoiceRepository.Query().FirstOrDefault(invoice => invoice.StartDate.Date == schedule.WeekDate.Date && invoice.EndDate.Date == schedule.LastWorkDayOfCycle.Date && invoice.EmployeeInformationId == user.EmployeeInformationId);
                                    var invoiceCount = _invoiceRepository.Query().Include(x => x.CreatedByUser).Where(x => x.CreatedByUser.SuperAdminId == user.SuperAdminId).Count();
                                    if (invoice == null)
                                    {
                                        var totalHourss = timesheets.Sum(timesheet => timesheet.Hours);
                                        invoice = new Invoice
                                        {
                                            EmployeeInformationId = (Guid)user.EmployeeInformationId,
                                            StartDate = schedule.WeekDate,
                                            EndDate = schedule.LastWorkDayOfCycle,
                                            PaymentDate = currentPaySchedule != null ? currentPaySchedule.PaymentDate : DateTime.Now.Date,
                                            InvoiceReference = invoiceCount == 0 ? $"INV{1:0000}" : $"INV{invoiceCount + 1:0000}",
                                            TotalHours = totalHourss,
                                            TotalAmount = user.EmployeeInformation.PayRollTypeId == 1 ? totalHourss * user.EmployeeInformation.RatePerHour : Convert.ToDouble(_timeSheetService.GetOffshoreTeamMemberTotalPay(user.EmployeeInformationId, schedule.WeekDate, schedule.LastWorkDayOfCycle, totalHourss, 1)),
                                            StatusId = user.EmployeeInformation.InvoiceGenerationType.ToLower() == "invoice" ? (int)Statuses.PENDING : (int)Statuses.SUBMITTED,
                                            CreatedByUserId = user.Id,
                                            InvoiceTypeId = (int)InvoiceTypes.PAYROLL,
                                        };
                                        _invoiceRepository.CreateAndReturn(invoice);

                                        if (expenses.Count() > 0)
                                        {
                                            var newInvoice = _invoiceRepository.Query().FirstOrDefault(invoice => invoice.StartDate.Date == schedule.WeekDate.Date && invoice.EndDate.Date == schedule.LastWorkDayOfCycle.Date && invoice.EmployeeInformationId == user.EmployeeInformationId);
                                            foreach (var expense in expenses)
                                            {
                                                expense.InvoiceId = newInvoice.Id;
                                                expense.IsInvoiced = true;
                                                _expenseRepository.Update(expense);
                                            }

                                            var totalExpenseAmount = _expenseRepository.Query().Where(expense => expense.InvoiceId == newInvoice.Id).Sum(expense => expense.Amount);
                                            newInvoice.TotalAmount = newInvoice.TotalAmount + Convert.ToDouble(totalExpenseAmount);
                                            _invoiceRepository.Update(newInvoice);

                                        }

                                        if (user.EmployeeInformation.PayRollTypeId == 2)
                                        {
                                            foreach (var admin in getAdmins)
                                            {
                                                _notificationService.SendNotification(new NotificationModel { UserId = admin.Id, Title = "Pending Invoice", Type = "Notification", Message = "A new item has been added to an invoice awaiting your approval." });
                                            }
                                        }
                                    }
                                }
                            }
                            
                        }

                        break;
                }
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _logger.LogDebug($"Timed host service disposed at {DateTime.Now.ToLongDateString()}. Running since {aliveSince}");
        }
    }
}
