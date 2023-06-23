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
using TimesheetBE.Services.Interfaces;

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
                            var _employeeInformationRepository = scope.ServiceProvider.GetRequiredService<IEmployeeInformationRepository>();
                            var _leaveService = scope.ServiceProvider.GetRequiredService<ILeaveService>();
                            var _leaveRepository = scope.ServiceProvider.GetRequiredService<ILeaveRepository>();

                            var allUsers = _userRepository.Query().Where(user => user.Role.ToLower() == "team member" || user.Role.ToLower() == "internal supervisor" || user.Role.ToLower() == "internal admin" || user.Role.ToLower() == "internal payroll manager" && user.SuperAdmin.ClientSubscriptionStatus == true).ToList();
                            //var allUsers = _userRepository.Query().Where(user =>user.EmployeeInformationId == Guid.Parse("08db5a6a-5eb9-427e-8394-8345267122ea")).ToList();

                            var nextDay = DateTime.Now.AddDays(1);

                            foreach (var user in allUsers)
                            {
                                var timesheetGenerationDate = _employeeInformationRepository.Query().FirstOrDefault(x => x.Id == user.EmployeeInformationId);
                                var lastTimesheet = _timeSheetRepository.Query().Where(x => x.EmployeeInformationId == user.EmployeeInformationId).OrderBy(x => x.Date).LastOrDefault();

                                //check if lastimesheet is null
                                if (lastTimesheet == null && timesheetGenerationDate.TimeSheetGenerationStartDate != DateTime.Parse("01/01/0001 00:00:00"))
                                {
                                    nextDay = timesheetGenerationDate.TimeSheetGenerationStartDate;
                                }

                                if(lastTimesheet != null)
                                {
                                    nextDay = lastTimesheet.Date.AddDays(1);
                                }

                                if (nextDay > timesheetGenerationDate.TimeSheetGenerationStartDate && timesheetGenerationDate.TimeSheetGenerationStartDate != DateTime.Parse("01/01/0001 00:00:00")
                                && nextDay.Date != DateTime.Now.AddDays(1).Date && lastTimesheet != null)
                                //if (nextDay > timesheetGenerationDate.TimeSheetGenerationStartDate && timesheetGenerationDate.TimeSheetGenerationStartDate != DateTime.Parse("01/01/0001 00:00:00") && lastTimesheet != null)
                                {
                                    if (lastTimesheet != null && lastTimesheet.Date.Date.AddDays(1) < DateTime.Now.Date)
                                    {
                                        nextDay = lastTimesheet.Date.AddDays(1);
                                    }

                                    if (nextDay.DayOfWeek == DayOfWeek.Saturday && lastTimesheet != null && lastTimesheet.Date.Date.AddDays(1) < DateTime.Now)
                                    {
                                        nextDay = nextDay.AddDays(2);
                                    }

                                    if (nextDay.DayOfWeek == DayOfWeek.Sunday && lastTimesheet != null && lastTimesheet.Date.Date.AddDays(1) < DateTime.Now)
                                    {
                                        nextDay = nextDay.AddDays(1);
                                    }
                                }

                                if (_timeSheetRepository.Query().Any(timeSheet => timeSheet.EmployeeInformationId == user.EmployeeInformationId && timeSheet.Date.Day == nextDay.Day &&
                                timeSheet.Date.Month == nextDay.Month && timeSheet.Date.Year == nextDay.Year))
                                    continue;
                                if (nextDay.DayOfWeek == DayOfWeek.Saturday) continue;
                                if (nextDay.DayOfWeek == DayOfWeek.Sunday) continue;
                                if (user.EmployeeInformationId == null) continue;
                                if (user.IsActive == false) continue;
                                if (user.EmailConfirmed == false) continue;
                                //if(user.SuperAdmin.ClientSubscriptionStatus == false || user.SuperAdmin.ClientSubscriptionStatus == null) continue;

                                // create timesheet for the next day of the current week and month for all users
                                var timeSheet = new TimeSheet
                                {
                                    Date = nextDay,
                                    EmployeeInformationId = (Guid)user.EmployeeInformationId,
                                    Hours = 0,
                                    IsApproved = false,
                                    StatusId = (int)Statuses.PENDING
                                };

                                if (_timeSheetRepository.Query().Any(timeSheet => timeSheet.EmployeeInformationId == user.EmployeeInformationId && timeSheet.Date.Day == nextDay.Day &&
                                timeSheet.Date.Month == nextDay.Month && timeSheet.Date.Year == nextDay.Year))
                                    continue;
                           
                                _timeSheetRepository.CreateAndReturn(timeSheet);
                                var timesheet = _timeSheetRepository.Query().Include(x => x.EmployeeInformation).FirstOrDefault(timeSheet => timeSheet.EmployeeInformationId == user.EmployeeInformationId && timeSheet.Date.Day == nextDay.Day && timeSheet.Date.Month == nextDay.Month && timeSheet.Date.Year == nextDay.Year);
                                var checkIfOnLeave = _leaveRepository.Query().FirstOrDefault(x => x.EmployeeInformationId == user.EmployeeInformationId && x.StartDate.Date <= nextDay.Date && nextDay.Date <= x.EndDate.Date && x.StatusId == (int)Statuses.APPROVED);
                                var employeeInformation = _employeeInformationRepository.Query().FirstOrDefault(x => x.Id == user.EmployeeInformationId);
                                if(checkIfOnLeave != null)
                                {
                                    if (nextDay.DayOfWeek == DayOfWeek.Saturday) continue;
                                    if (nextDay.DayOfWeek == DayOfWeek.Sunday) continue;

                                    var noOfDaysEligible = _leaveService.GetEligibleLeaveDays(user.EmployeeInformationId);
                                    noOfDaysEligible = noOfDaysEligible - employeeInformation.NumberOfEligibleLeaveDaysTaken;
                                    if(noOfDaysEligible > 0)
                                    {
                                        timeSheet.OnLeave = true;
                                        timeSheet.OnLeaveAndEligibleForLeave = true;
                                        //timeSheet.Hours = employeeInformation.NumberOfHoursEligible ?? default(int);

                                        employeeInformation.NumberOfEligibleLeaveDaysTaken += 1;
                                        _employeeInformationRepository.Update(employeeInformation);
                                    }
                                    if(noOfDaysEligible <= 0)
                                    {
                                        timeSheet.OnLeave = true;
                                        timeSheet.OnLeaveAndEligibleForLeave = false;
                                    } 
                                } 
                                timesheet.EmployeeInformation.User.DateModified = DateTime.Now;
                                _timeSheetRepository.Update(timesheet);
                                
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