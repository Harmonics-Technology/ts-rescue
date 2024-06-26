﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Models.IdentityModels;
using TimesheetBE.Repositories.Interfaces;
using TimesheetBE.Services.Interfaces;
using TimesheetBE.Utilities;
using TimesheetBE.Utilities.Abstrctions;
using TimesheetBE.Utilities.Constants;

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
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(30));
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
                            var _timeSheetService = scope.ServiceProvider.GetRequiredService<ITimeSheetService>();
                            var _onboardingFeeService = scope.ServiceProvider.GetRequiredService<IOnboardingFeeService>();
                            var _onboradingFeeRepository = scope.ServiceProvider.GetRequiredService<IOnboardingFeeRepository>();
                            var _emailHandler = scope.ServiceProvider.GetRequiredService<IEmailHandler>();
                            var _appSettings = scope.ServiceProvider.GetRequiredService<IOptions<Globals>>();

                            var allUsers = _userRepository.Query().Include(user => user.EmployeeInformation).Where(user => user.Role.ToLower() == "client").ToList();

                            foreach (var user in allUsers)
                            {
                                //Generate invoices for users base on their payment frequency
                                GenerateInvoiceForWeeklyScheduleUser(_invoiceRepository, user, _paymentScheduleRepository, _codeProvider, _expenseRepository, _timeSheetService, _onboradingFeeRepository, _emailHandler, _appSettings);
                            }


                        }
                    }
                    catch (System.Exception ex)
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

        private void GenerateInvoiceForWeeklyScheduleUser(IInvoiceRepository _invoiceRepository, User user, IPaymentScheduleRepository _paymentScheduleRepository, ICodeProvider _codeProvider, 
            IExpenseRepository _expenseRepository, ITimeSheetService _timeSheetService, IOnboardingFeeRepository _onboradingFeeRepository, IEmailHandler _emailHandler, IOptions<Globals> _appSettings)
        {
            int[] allMonth = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 9, 10, 11, 12 };

            foreach (var month in allMonth)
            {
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
                            var invoices = _invoiceRepository.Query().Include(x => x.EmployeeInformation).Where(invoice => invoice.StatusId == (int)Statuses.INVOICED && invoice.EmployeeInformation.Supervisor.ClientId == user.Id && invoice.InvoiceTypeId == (int)InvoiceTypes.PAYROLL && schedule.WeekDate <= invoice.StartDate && invoice.EndDate <= schedule.LastWorkDayOfCycle
                            || invoice.StatusId == (int)Statuses.INVOICED && invoice.EmployeeInformation.Supervisor.EmployeeInformation.Supervisor.ClientId == user.Id && invoice.InvoiceTypeId == (int)InvoiceTypes.PAYROLL
                            && schedule.WeekDate <= invoice.StartDate && invoice.EndDate <= schedule.LastWorkDayOfCycle).ToList();
                            if (invoices.Count() > 0)
                            {
                                
                                var invoice = _invoiceRepository.Query().FirstOrDefault(invoice => invoice.StartDate == schedule.WeekDate && invoice.EndDate == schedule.LastWorkDayOfCycle);
                                if (invoice == null && schedule.LastWorkDayOfCycle.Date.AddDays(Convert.ToDouble(user?.Term)) >= DateTime.Now.Date)
                                {
                                    double? totalClientBill = 0;
                                    foreach(var inv in invoices)
                                    {
                                        var clientTotalPay = inv.EmployeeInformation.PayRollTypeId == 1 ? inv.TotalHours * inv.EmployeeInformation?.ClientRate : Convert.ToDouble(_timeSheetService.GetOffshoreTeamMemberTotalPay(inv.EmployeeInformationId, inv.StartDate, inv.EndDate, inv.TotalHours, 2) / Convert.ToDouble(inv.Rate));
                                        inv.ClientTotalAmount = clientTotalPay;
                                        _invoiceRepository.Update(inv);
                                        totalClientBill += clientTotalPay;

                                    }

                                    var currentHST = _onboradingFeeRepository.Query().FirstOrDefault(x => x.OnboardingFeeType.ToLower() == "hst");
                                    invoice = new Invoice
                                    {
                                        StartDate = schedule.WeekDate,
                                        EndDate = schedule.LastWorkDayOfCycle,
                                        InvoiceReference = _codeProvider.New(Guid.Empty, "Invoice Reference", 0, 6, "INV-").CodeString,
                                        TotalAmount = Convert.ToDouble(totalClientBill),
                                        StatusId = (int)Statuses.INVOICED,
                                        CreatedByUserId = user.Id,
                                        InvoiceTypeId = (int)InvoiceTypes.CLIENT,
                                        HST = currentHST?.Fee
                                    };
                                    _invoiceRepository.CreateAndReturn(invoice);

                                    var newInvoice = _invoiceRepository.Query().FirstOrDefault(invoice => invoice.StartDate == schedule.WeekDate && invoice.EndDate == schedule.LastWorkDayOfCycle && invoice.CreatedByUserId == user.Id);
                                    foreach (var inv in invoices)
                                    {
                                        inv.ClientInvoiceId = newInvoice.Id;
                                        newInvoice.TotalAmount += inv.EmployeeInformation.FixedAmount == true ? inv.EmployeeInformation.OnBoradingFee : Convert.ToDouble(inv.ClientTotalAmount) * (inv.EmployeeInformation.OnBoradingFee / 100);
                                        _invoiceRepository.Update(newInvoice);
                                        _invoiceRepository.Update(inv);
                                    }

                                    List<KeyValuePair<string, string>> EmailParameters = new()
                                    {
                                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_LOGO_URL, _appSettings.Value.LOGO),
                                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, user.FirstName),
                                    };

                                    var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.CLIENT_INVOICE_FILENAME, EmailParameters);
                                    var SendEmail = _emailHandler.SendEmail(user.Email, "INVOICE TO CLIENT", EmailTemplate, "");
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
                            var invoices = _invoiceRepository.Query().Include(x => x.EmployeeInformation).Where(invoice => invoice.StatusId == (int)Statuses.INVOICED && invoice.EmployeeInformation.Supervisor.ClientId == user.Id && invoice.InvoiceTypeId == (int)InvoiceTypes.PAYROLL && schedule.WeekDate <= invoice.StartDate && invoice.EndDate <= schedule.LastWorkDayOfCycle
                            || invoice.StatusId == (int)Statuses.INVOICED && invoice.EmployeeInformation.Supervisor.EmployeeInformation.Supervisor.ClientId == user.Id && invoice.InvoiceTypeId == (int)InvoiceTypes.PAYROLL
                            && schedule.WeekDate <= invoice.StartDate && invoice.EndDate <= schedule.LastWorkDayOfCycle).ToList();
                            if (invoices.Count() > 0)
                            {

                                var invoice = _invoiceRepository.Query().FirstOrDefault(invoice => invoice.StartDate == schedule.WeekDate && invoice.EndDate == schedule.LastWorkDayOfCycle && invoice.InvoiceTypeId == 4);
                                if (invoice == null && schedule.LastWorkDayOfCycle.Date.AddDays(Convert.ToDouble(user?.Term)) <= DateTime.Now.Date)
                                {

                                    double? totalClientBill = 0;
                                    foreach (var inv in invoices)
                                    {
                                        var clientTotalPay = inv.EmployeeInformation.PayRollTypeId == 1 ? inv.TotalHours * inv.EmployeeInformation?.ClientRate : Convert.ToDouble(_timeSheetService.GetOffshoreTeamMemberTotalPay(inv.EmployeeInformationId, inv.StartDate, inv.EndDate, inv.TotalHours, 2)) / Convert.ToDouble(inv.Rate);
                                        inv.ClientTotalAmount = clientTotalPay;
                                        _invoiceRepository.Update(inv);
                                        totalClientBill += clientTotalPay;
                                    }

                                    var currentHST = _onboradingFeeRepository.Query().FirstOrDefault(x => x.OnboardingFeeType.ToLower() == "hst");
                                    invoice = new Invoice
                                    {
                                        StartDate = schedule.WeekDate,
                                        EndDate = schedule.LastWorkDayOfCycle,
                                        InvoiceReference = _codeProvider.New(Guid.Empty, "Invoice Reference", 0, 6, "INV-").CodeString,
                                        TotalAmount = Convert.ToDouble(totalClientBill),
                                        StatusId = (int)Statuses.INVOICED,
                                        CreatedByUserId = user.Id,
                                        InvoiceTypeId = (int)InvoiceTypes.CLIENT,
                                        HST = currentHST?.Fee
                                    };
                                    _invoiceRepository.CreateAndReturn(invoice);

                                    var newInvoice = _invoiceRepository.Query().FirstOrDefault(invoice => invoice.StartDate == schedule.WeekDate && invoice.EndDate == schedule.LastWorkDayOfCycle && invoice.CreatedByUserId == user.Id);
                                    foreach (var inv in invoices)
                                    {
                                        inv.ClientInvoiceId = newInvoice.Id;
                                        newInvoice.TotalAmount += inv.EmployeeInformation.FixedAmount == true ? inv.EmployeeInformation.OnBoradingFee : Convert.ToDouble(inv.ClientTotalAmount) * (inv.EmployeeInformation.OnBoradingFee / 100);
                                        _invoiceRepository.Update(newInvoice);
                                        _invoiceRepository.Update(inv);
                                    }

                                    List<KeyValuePair<string, string>> EmailParameters = new()
                                    {
                                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_LOGO_URL, _appSettings.Value.LOGO),
                                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, user.FirstName),
                                    };

                                    var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.CLIENT_INVOICE_FILENAME, EmailParameters);
                                    var SendEmail = _emailHandler.SendEmail(user.Email, "INVOICE TO CLIENT", EmailTemplate, "");
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
