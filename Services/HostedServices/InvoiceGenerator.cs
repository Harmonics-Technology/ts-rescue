using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
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
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
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

            //var AllMonths = Enumerable.Range(1, 12).Select(a => new
            //{
            //    Name = System.Globalization.DateTimeFormatInfo.CurrentInfo.GetMonthName(a).ToUpper(),
            //    //Code = a.ToString()
            //});
            var getAdmins = _userRepository.Query().Where(x => x.Role.ToLower() == "payroll manager").ToList();

            int[] allMonth = new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 9, 10, 11, 12};

            foreach (var month in allMonth)
            {
                //var thmo = DateTime.Now.ToString("MMMM");
                var monthlyPaySchedule = _paymentScheduleRepository.Query().Where(schedule => schedule.LastWorkDayOfCycle.Month == month && schedule.WeekDate < schedule.LastWorkDayOfCycle).ToList();
                if (user?.EmployeeInformation?.PaymentFrequency == null) continue;
                switch (user?.EmployeeInformation?.PaymentFrequency.ToLower())
                {
                    case "weekly":
                        var weeklyPaymentSchedule = monthlyPaySchedule.Where(schedule => schedule.CycleType == "Weekly");
                        var monthy = weeklyPaymentSchedule.ToList();
                        foreach(var schedule in weeklyPaymentSchedule)
                        {
                            if (DateTime.Now <= schedule.LastWorkDayOfCycle)
                                break;
                            var timesheets = _timeSheetRepository.Query().Where(timesheet => timesheet.Date >= schedule.WeekDate && timesheet.Date <= schedule.LastWorkDayOfCycle && timesheet.Date.DayOfWeek != DayOfWeek.Saturday && timesheet.Date.DayOfWeek != DayOfWeek.Sunday && timesheet.EmployeeInformationId == user.EmployeeInformationId).ToList();
                            var expenses = _expenseRepository.Query().Where(expense => expense.TeamMemberId == user.Id && expense.StatusId == (int)Statuses.APPROVED && expense.DateCreated >= schedule.WeekDate && expense.DateCreated <= schedule.LastWorkDayOfCycle).ToList();
                            if(timesheets.Count() > 0) {
                                if (!timesheets.Any(x => x.StatusId == (int)Statuses.REJECTED || x.StatusId == (int)Statuses.PENDING))
                                {
                                    var invoice = _invoiceRepository.Query().FirstOrDefault(invoice => invoice.StartDate == schedule.WeekDate && invoice.EndDate == schedule.LastWorkDayOfCycle && invoice.EmployeeInformationId == user.EmployeeInformationId);
                                    
                                    if (invoice == null)
                                    {
                                        var totalHourss = timesheets.Sum(timesheet => timesheet.Hours);
                                        invoice = new Invoice
                                        {
                                            EmployeeInformationId = (Guid)user.EmployeeInformationId,
                                            StartDate = schedule.WeekDate,
                                            EndDate = schedule.LastWorkDayOfCycle,
                                            InvoiceReference = _codeProvider.New(Guid.Empty, "Invoice Reference", 0, 6, "INV-").CodeString,
                                            TotalHours = totalHourss,
                                            TotalAmount = user.EmployeeInformation.PayRollTypeId == 1 ? totalHourss * user.EmployeeInformation.RatePerHour : Convert.ToDouble(_timeSheetService.GetOffshoreTeamMemberTotalPay(user.EmployeeInformationId, schedule.WeekDate, schedule.LastWorkDayOfCycle, totalHourss)),
                                            StatusId = user.EmployeeInformation.PayRollTypeId == 1 ? (int)Statuses.PENDING : (int)Statuses.SUBMITTED,
                                            CreatedByUserId = user.Id,
                                            InvoiceTypeId = (int)InvoiceTypes.PAYROLL
                                        };
                                        _invoiceRepository.CreateAndReturn(invoice);

                                        if (expenses.Count() > 0)
                                        {
                                            var newInvoice = _invoiceRepository.Query().FirstOrDefault(invoice => invoice.StartDate == schedule.WeekDate && invoice.EndDate == schedule.LastWorkDayOfCycle && invoice.EmployeeInformationId == user.EmployeeInformationId);
                                            foreach (var expense in expenses)
                                            {
                                                //var invoiceId = _invoiceRepository.Query().FirstOrDefault(invoice => invoice.StartDate == schedule.WeekDate && invoice.EndDate == schedule.LastWorkDayOfCycle && invoice.EmployeeInformationId == user.EmployeeInformationId).Id;
                                                expense.InvoiceId = newInvoice.Id;
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
                        var biWeeklyPaymentSchedule = monthlyPaySchedule.Where(schedule => schedule.CycleType == "Bi-Weekly");
                        foreach (var schedule in biWeeklyPaymentSchedule)
                        {
                            if (DateTime.Now <= schedule.LastWorkDayOfCycle)
                                break;
                            var timesheets = _timeSheetRepository.Query().Where(timesheet => timesheet.Date >= schedule.WeekDate && timesheet.Date <= schedule.LastWorkDayOfCycle && timesheet.Date.DayOfWeek != DayOfWeek.Saturday && timesheet.Date.DayOfWeek != DayOfWeek.Sunday && timesheet.EmployeeInformationId == user.EmployeeInformationId).ToList();
                            var expenses = _expenseRepository.Query().Where(expense => expense.TeamMemberId == user.Id && expense.StatusId == (int)Statuses.APPROVED && expense.DateCreated >= schedule.WeekDate && expense.DateCreated <= schedule.LastWorkDayOfCycle).ToList();
                            if(timesheets.Count() > 0)
                            {
                                if (!timesheets.Any(x => x.StatusId == (int)Statuses.REJECTED || x.StatusId == (int)Statuses.PENDING))
                                {
                                    var invoice = _invoiceRepository.Query().FirstOrDefault(invoice => invoice.StartDate == schedule.WeekDate && invoice.EndDate == schedule.LastWorkDayOfCycle && invoice.EmployeeInformationId == user.EmployeeInformationId);
                                    if (invoice == null)
                                    {
                                        var totalHourss = timesheets.Sum(timesheet => timesheet.Hours);
                                        invoice = new Invoice
                                        {
                                            EmployeeInformationId = (Guid)user.EmployeeInformationId,
                                            StartDate = schedule.WeekDate,
                                            EndDate = schedule.LastWorkDayOfCycle,
                                            InvoiceReference = _codeProvider.New(Guid.Empty, "Invoice Reference", 0, 6, "INV-").CodeString,
                                            TotalHours = totalHourss,
                                            TotalAmount = user.EmployeeInformation.PayRollTypeId == 1 ? totalHourss * user.EmployeeInformation.RatePerHour : Convert.ToDouble(_timeSheetService.GetOffshoreTeamMemberTotalPay(user.EmployeeInformationId, schedule.WeekDate, schedule.LastWorkDayOfCycle, totalHourss)),
                                            StatusId = user.EmployeeInformation.PayRollTypeId == 1 ? (int)Statuses.PENDING : (int)Statuses.SUBMITTED,
                                            CreatedByUserId = user.Id,
                                            InvoiceTypeId = (int)InvoiceTypes.PAYROLL,
                                        };
                                        _invoiceRepository.CreateAndReturn(invoice);

                                        if (expenses.Count() > 0)
                                        {
                                            var newInvoice = _invoiceRepository.Query().FirstOrDefault(invoice => invoice.StartDate == schedule.WeekDate && invoice.EndDate == schedule.LastWorkDayOfCycle && invoice.EmployeeInformationId == user.EmployeeInformationId);
                                            foreach (var expense in expenses)
                                            {
                                                //var invoiceId = _invoiceRepository.Query().FirstOrDefault(invoice => invoice.StartDate == schedule.WeekDate && invoice.EndDate == schedule.LastWorkDayOfCycle && invoice.EmployeeInformationId == user.EmployeeInformationId).Id;
                                                expense.InvoiceId = newInvoice.Id;
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
                        var monthlyPaymentSchedule = monthlyPaySchedule.Where(schedule => schedule.CycleType == "Monthly");
                        foreach (var schedule in monthlyPaymentSchedule)
                        {
                            if (DateTime.Now <= schedule.LastWorkDayOfCycle)
                                break;
                            var timesheets = _timeSheetRepository.Query().Where(timesheet => timesheet.Date >= schedule.WeekDate && timesheet.Date <= schedule.LastWorkDayOfCycle && timesheet.Date.DayOfWeek != DayOfWeek.Saturday && timesheet.Date.DayOfWeek != DayOfWeek.Sunday && timesheet.EmployeeInformationId == user.EmployeeInformationId).ToList();
                            var expenses = _expenseRepository.Query().Where(expense => expense.TeamMemberId == user.Id && expense.StatusId == (int)Statuses.APPROVED && expense.DateCreated >= schedule.WeekDate && expense.DateCreated <= schedule.LastWorkDayOfCycle).ToList();
                            if(timesheets.Count() > 0)
                            {
                                if (!timesheets.Any(x => x.StatusId == (int)Statuses.REJECTED || x.StatusId == (int)Statuses.PENDING))
                                {
                                    var invoice = _invoiceRepository.Query().FirstOrDefault(invoice => invoice.StartDate == schedule.WeekDate && invoice.EndDate == schedule.LastWorkDayOfCycle && invoice.EmployeeInformationId == user.EmployeeInformationId);
                                    if (invoice == null)
                                    {
                                        var totalHourss = timesheets.Sum(timesheet => timesheet.Hours);
                                        invoice = new Invoice
                                        {
                                            EmployeeInformationId = (Guid)user.EmployeeInformationId,
                                            StartDate = schedule.WeekDate,
                                            EndDate = schedule.LastWorkDayOfCycle,
                                            InvoiceReference = _codeProvider.New(Guid.Empty, "Invoice Reference", 0, 6, "INV-").CodeString,
                                            TotalHours = totalHourss,
                                            TotalAmount = user.EmployeeInformation.PayRollTypeId == 1 ? totalHourss * user.EmployeeInformation.RatePerHour : Convert.ToDouble(_timeSheetService.GetOffshoreTeamMemberTotalPay(user.EmployeeInformationId, schedule.WeekDate, schedule.LastWorkDayOfCycle, totalHourss)),
                                            StatusId = user.EmployeeInformation.PayRollTypeId == 1 ? (int)Statuses.PENDING : (int)Statuses.SUBMITTED,
                                            CreatedByUserId = user.Id,
                                            InvoiceTypeId = (int)InvoiceTypes.PAYROLL,
                                        };
                                        _invoiceRepository.CreateAndReturn(invoice);

                                        if (expenses.Count() > 0)
                                        {
                                            var newInvoice = _invoiceRepository.Query().FirstOrDefault(invoice => invoice.StartDate == schedule.WeekDate && invoice.EndDate == schedule.LastWorkDayOfCycle && invoice.EmployeeInformationId == user.EmployeeInformationId);
                                            foreach (var expense in expenses)
                                            {
                                                //var invoiceId = _invoiceRepository.Query().FirstOrDefault(invoice => invoice.StartDate == schedule.WeekDate && invoice.EndDate == schedule.LastWorkDayOfCycle && invoice.EmployeeInformationId == user.EmployeeInformationId).Id;
                                                expense.InvoiceId = newInvoice.Id;
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
                //foreach (var schedule in monthlyPaySchedule)
                //{
                //    switch (user?.EmployeeInformation?.PaymentFrequency)
                //    {
                //        case "weekly":
                //            var weeklyPaymentSchedule = schedule.
                //            var timesheets = _timeSheetRepository.Query().Where(timesheet => timesheet.Date >= schedule.WeekDate && timesheet.Date <= schedule.LastWorkDayOfCycle && schedule.);
                //            break;
                //    }
                    
                //}
            }


            //var lastInvoice = _invoiceRepository.Query().Where(invoice => invoice.EmployeeInformationId == user.EmployeeInformationId).OrderBy(invoice => invoice.DateCreated).LastOrDefault();

            //PaymentSchedule currentCycle = null;
            //switch (user?.EmployeeInformation?.PaymentFrequency)
            //{
            //    case "weekly":
            //        if (lastInvoice != null)
            //            currentCycle = _paymentScheduleRepository.Query().Where(x => lastInvoice.EndDate.AddDays(2) == x.WeekDate && user.EmployeeInformation.PaymentFrequency == "Weekly").FirstOrDefault();
            //        if (lastInvoice == null)
            //            currentCycle = _paymentScheduleRepository.Query().Where(x => x.CycleType == "Weekly").FirstOrDefault();
            //        break;

            //    case "bi-weekly":
            //        if (lastInvoice != null)
            //            currentCycle = _paymentScheduleRepository.Query().Where(x => lastInvoice.EndDate.AddDays(2) == x.WeekDate && user.EmployeeInformation.PaymentFrequency == "Bi-Weekly").FirstOrDefault();
            //        if (lastInvoice == null)
            //            currentCycle = _paymentScheduleRepository.Query().Where(x => x.CycleType == "Bi-Weekly").FirstOrDefault();
            //        break;

            //    case "Monthly":
            //        if (lastInvoice != null)
            //            currentCycle = _paymentScheduleRepository.Query().Where(x => lastInvoice.EndDate.AddDays(2) == x.WeekDate && user.EmployeeInformation.PaymentFrequency == "Monthly").FirstOrDefault();
            //        if (lastInvoice == null)
            //            currentCycle = _paymentScheduleRepository.Query().Where(x => x.CycleType == "Monthly").FirstOrDefault();
            //        break;
            //};
                


            //var totalHours = _timeSheetRepository.Query().Where(timesheet => timesheet.Date >= currentCycle.WeekDate && timesheet.Date <= currentCycle.LastWorkDayOfCycle && timesheet.IsApproved == true).Sum(timesheet => timesheet.Hours);
            //var totalexpense = _expenseRepository.Query().Where(expense => expense.TeamMemberId == user.Id && expense.DateCreated >= currentCycle.WeekDate && expense.DateCreated <= currentCycle.LastWorkDayOfCycle && expense.StatusId == (int)Statuses.APPROVED).Sum(expense => expense.Amount);

            //var totalPayOut = totalHours * Convert.ToDouble(user.EmployeeInformation.RatePerHour) + Convert.ToDouble(totalexpense);
            //if(currentCycle.LastWorkDayOfCycle.AddDays(5).DayOfWeek == DayOfWeek.Wednesday && DateTime.Now.DayOfWeek == DayOfWeek.Wednesday)
            //{
            //    var invoice = new Invoice
            //    {
            //        EmployeeInformationId = (Guid)user.EmployeeInformationId,
            //        StartDate = currentCycle.WeekDate,
            //        EndDate = currentCycle.LastWorkDayOfCycle,
            //        InvoiceReference = _codeProvider.New(Guid.Empty, "Invoice Reference", 0, 6, "INV-").CodeString,
            //        TotalHours = totalHours,
            //        TotalAmount = totalPayOut,
            //        StatusId = (int)Statuses.PENDING,
            //        CreatedByUserId = user.Id,
            //        InvoiceTypeId = (int)InvoiceTypes.PAYROLL,
            //    };

            //    _invoiceRepository.CreateAndReturn(invoice);
            //}
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
