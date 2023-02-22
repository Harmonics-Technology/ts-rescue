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
using TimesheetBE.Repositories.Interfaces;
using TimesheetBE.Utilities.Abstrctions;

namespace TimesheetBE.Services.HostedServices
{
    public class ClientInvoiceGenerator : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private Timer _timer;
        DateTime aliveSince;
        public IServiceProvider Services { get; }
        private ILogger<ClientInvoiceGenerator> _logger;

        public ClientInvoiceGenerator(IServiceProvider services, ILogger<ClientInvoiceGenerator> logger)
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
                            var _paymentScheduleRepository = scope.ServiceProvider.GetRequiredService<IPaymentScheduleRepository>();
                            var _codeProvider = scope.ServiceProvider.GetRequiredService<ICodeProvider>();
                            var _invoiceRepository = scope.ServiceProvider.GetRequiredService<IInvoiceRepository>();
                            var _expenseRepository = scope.ServiceProvider.GetRequiredService<IExpenseRepository>();

                            var allUsers = _userRepository.Query().Include(user => user.EmployeeInformation).Where(user => user.Role.ToLower() == "client").ToList();

                            foreach (var user in allUsers)
                            {
                                //Generate invoices for users base on their payment frequency
                                GenerateIvoiceForWeeklyScheduleUser(_invoiceRepository, user, _paymentScheduleRepository, _codeProvider, _expenseRepository);
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

        private void GenerateIvoiceForWeeklyScheduleUser(IInvoiceRepository _invoiceRepository, User user, IPaymentScheduleRepository _paymentScheduleRepository, ICodeProvider _codeProvider, IExpenseRepository _expenseRepository)
        {

            //var AllMonths = Enumerable.Range(1, 12).Select(a => new
            //{
            //    Name = System.Globalization.DateTimeFormatInfo.CurrentInfo.GetMonthName(a).ToUpper(),
            //    //Code = a.ToString()
            //});

            int[] allMonth = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 9, 10, 11, 12 };

            foreach (var month in allMonth)
            {
                //var thmo = DateTime.Now.ToString("MMMM");
                var monthlyPaySchedule = _paymentScheduleRepository.Query().Where(schedule => schedule.LastWorkDayOfCycle.Month == month && schedule.WeekDate < schedule.LastWorkDayOfCycle).ToList();
                if (user?.InvoiceGenerationFrequency == null) continue;
                switch (user?.InvoiceGenerationFrequency.ToLower())
                {
                   
                    case "bi-weekly":
                        var biWeeklyPaymentSchedule = monthlyPaySchedule.Where(schedule => schedule.CycleType == "Bi-Weekly");
                        foreach (var schedule in biWeeklyPaymentSchedule)
                        {
                            if (DateTime.Now <= schedule.LastWorkDayOfCycle)
                                break;
                            var invoices = _invoiceRepository.Query().Where(invoice => invoice.StatusId == (int)Statuses.INVOICED && invoice.EmployeeInformation.Supervisor.ClientId == user.Id && invoice.PaymentDate >= schedule.WeekDate && invoice.PaymentDate <= schedule.LastWorkDayOfCycle).ToList();
                            if (invoices.Count() > 0)
                            {
                                
                                var invoice = _invoiceRepository.Query().FirstOrDefault(invoice => invoice.StartDate == schedule.WeekDate && invoice.EndDate == schedule.LastWorkDayOfCycle);
                                if (invoice == null && schedule.LastWorkDayOfCycle.Date.AddDays(Convert.ToDouble(user?.Term)) == DateTime.Now.Date)
                                {

                                    
                                    invoice = new Invoice
                                    {
                                        StartDate = schedule.WeekDate,
                                        EndDate = schedule.LastWorkDayOfCycle,
                                        InvoiceReference = _codeProvider.New(Guid.Empty, "Invoice Reference", 0, 6, "INV-").CodeString,
                                        TotalAmount = invoices.Sum(invoice => invoice.TotalAmount),
                                        StatusId = (int)Statuses.INVOICED,
                                        CreatedByUserId = user.Id,
                                        InvoiceTypeId = (int)InvoiceTypes.CLIENT
                                    };
                                    _invoiceRepository.CreateAndReturn(invoice);
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
                            var invoices = _invoiceRepository.Query().Where(invoice => invoice.StatusId == (int)Statuses.INVOICED && invoice.EmployeeInformation.Supervisor.ClientId == user.Id &&  invoice.PaymentDate >= schedule.WeekDate && invoice.PaymentDate <= schedule.LastWorkDayOfCycle).ToList();
                            if (invoices.Count() > 0)
                            {

                                var invoice = _invoiceRepository.Query().FirstOrDefault(invoice => invoice.StartDate == schedule.WeekDate && invoice.EndDate == schedule.LastWorkDayOfCycle);
                                if (invoice == null && schedule.LastWorkDayOfCycle.Date.AddDays(Convert.ToDouble(user?.Term)) == DateTime.Now.Date)
                                {


                                    invoice = new Invoice
                                    {
                                        StartDate = schedule.WeekDate,
                                        EndDate = schedule.LastWorkDayOfCycle,
                                        InvoiceReference = _codeProvider.New(Guid.Empty, "Invoice Reference", 0, 6, "INV-").CodeString,
                                        TotalAmount = invoices.Sum(invoice => invoice.TotalAmount),
                                        StatusId = (int)Statuses.INVOICED,
                                        CreatedByUserId = user.Id,
                                        InvoiceTypeId = (int)InvoiceTypes.CLIENT
                                    };
                                    _invoiceRepository.CreateAndReturn(invoice);
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
