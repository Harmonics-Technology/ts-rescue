using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Models.IdentityModels;
using TimesheetBE.Repositories.Interfaces;

namespace TimesheetBE.Services.HostedServices
{
    public class TimeSheetGenerator : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private Timer _timer;
        DateTime aliveSince;
        public IServiceProvider Services { get; }
        private ILogger<TimeSheetGenerator> _logger;
        public TimeSheetGenerator(IServiceProvider services, ILogger<TimeSheetGenerator> logger)
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
                            var _userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

                            var allUsers = _userRepository.Query().Where(user => user.Role.ToLower() == "team member" || user.Role.ToLower() == "internal supervisor" || user.Role.ToLower() == "internal admin" || user.Role.ToLower() == "internal payroll manager").ToList();
                            var nextDay = DateTime.Now.AddDays(1);

                            foreach (var user in allUsers)
                            {
                                if (_timeSheetRepository.Query().Any(timeSheet => timeSheet.EmployeeInformationId == user.EmployeeInformationId && timeSheet.Date.Day == nextDay.Day && timeSheet.Date.Month == nextDay.Month && timeSheet.Date.Year == nextDay.Year))
                                    continue;
                                if (nextDay.DayOfWeek == DayOfWeek.Saturday) continue;
                                if (nextDay.DayOfWeek == DayOfWeek.Sunday) continue;
                                if (user.EmployeeInformationId == null) continue;
                                if (user.IsActive == false) continue;
                                var timeSheet = new TimeSheet
                                {
                                    Date = nextDay,
                                    EmployeeInformationId = (Guid)user.EmployeeInformationId,
                                    Hours = 0,
                                    IsApproved = false,
                                    StatusId = (int)Statuses.PENDING
                                };
                                _timeSheetRepository.CreateAndReturn(timeSheet);
                                var timesheet = _timeSheetRepository.Query().Include(x => x.EmployeeInformation).FirstOrDefault(timeSheet => timeSheet.EmployeeInformationId == user.EmployeeInformationId && timeSheet.Date.Day == nextDay.Day && timeSheet.Date.Month == nextDay.Month && timeSheet.Date.Year == nextDay.Year);
                                timesheet.EmployeeInformation.User.DateModified = DateTime.Now;
                                _timeSheetRepository.Update(timesheet);
                                // create timesheet for the next day of the current week and month for all users
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