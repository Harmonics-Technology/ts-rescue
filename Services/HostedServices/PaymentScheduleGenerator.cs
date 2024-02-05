using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TimesheetBE.Repositories.Interfaces;
using TimesheetBE.Utilities.Abstrctions;
using TimesheetBE.Utilities;
using System.Linq;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Repositories;
using TimesheetBE.Services.Interfaces;

namespace TimesheetBE.Services.HostedServices
{
    public class PaymentScheduleGenerator : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private Timer _timer;
        DateTime aliveSince;
        public IServiceProvider Services { get; }
        private ILogger<PaymentScheduleGenerator> _logger;
        public PaymentScheduleGenerator(IServiceProvider services, ILogger<PaymentScheduleGenerator> logger)
        {
            Services = services;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            aliveSince = DateTime.Now;
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromDays(1));
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
                            var _payrollService = scope.ServiceProvider.GetRequiredService<IPayrollService>();

                            var users = _userRepository.Query().Where(x => x.Role.ToLower() == "super admin" && x.Id == Guid.Parse("08dbf4d5-88f2-4e59-8432-91354aee981c") && x.IsActive == true && x.ClientSubscriptionStatus == true).ToList();

                            foreach (var user in users)
                            {
                                _payrollService.AutoGenerateWeeklyPaySchedule(user.Id);
                                _payrollService.AutoGenerateBiWeeklyPayschedule(user.Id);
                                _payrollService.AutoGenerateMonthlyPayScheduleWeeklyPeriod(user.Id);
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
            catch (System.Exception)
            {
                throw;
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
