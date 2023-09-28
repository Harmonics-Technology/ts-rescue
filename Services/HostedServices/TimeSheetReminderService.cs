using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TimesheetBE.Services.Interfaces;

namespace TimesheetBE.Services.HostedServices
{
    public class TimeSheetReminderService : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private Timer _timer;
        DateTime aliveSince;
        public IServiceProvider Services { get; }
        private ILogger<TimeSheetReminderService> _logger;

        public TimeSheetReminderService(IServiceProvider services, ILogger<TimeSheetReminderService> logger)
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
                            var _reminderService = scope.ServiceProvider.GetRequiredService<IReminderService>();

                            //if(DateTime.Now.DayOfWeek == DayOfWeek.Friday)
                            //{
                            //    _reminderService.SendFillTimesheetReminder();
                            //}

                            //if (DateTime.Now.DayOfWeek == DayOfWeek.Monday)
                            //{
                            //    _reminderService.SendApproveTimesheetReminder();

                            //}

                            //timesheet reminder on cutoff periods

                            //_reminderService.SendFillTimesheetReminder();
                            _reminderService.SendFillTimesheetReminderToTeamMember();
                            _reminderService.SendCutOffTimesheetReminderToTeamMember();
                            _reminderService.SendOverdueTaskReminder();
                            _reminderService.SendOverdueSubTaskReminder();
                            _reminderService.SendProjectTimesheetReminder();
                            _reminderService.SendProjectTimesheetOverdueReminder();
                            

                            // create timesheet for the next day of the current week and month for all users
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