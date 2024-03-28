using Microsoft.AspNetCore.Hosting;
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
using TimesheetBE.Repositories.Interfaces;
using TimesheetBE.Utilities;
using TimesheetBE.Utilities.Abstrctions;
using TimesheetBE.Utilities.Constants;

namespace TimesheetBE.Services.HostedServices
{
    public class UpdateContractStatus : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private Timer _timer;
        DateTime aliveSince;
        public IServiceProvider Services { get; }
        private ILogger<UpdateContractStatus> _logger;
        public UpdateContractStatus(IServiceProvider services, ILogger<UpdateContractStatus> logger)
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
                            var _contractRepository = scope.ServiceProvider.GetRequiredService<IContractRepository>();
                            var _userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                            var _appSettings = scope.ServiceProvider.GetRequiredService<IOptions<Globals>>();
                            var _emailHandler = scope.ServiceProvider.GetRequiredService<IEmailHandler>();


                            var allContract = _contractRepository.Query().Include(x => x.EmployeeInformation).ToList();

                            foreach (var contract in allContract)
                            {
                                

                                if(DateTime.Now.Date == contract.EndDate.Date.AddDays(14))
                                {
                                    var user = _userRepository.Query().FirstOrDefault(x => x.Id == contract.EmployeeInformation.UserId);

                                    if (user == null) continue;

                                    var superAdmin = _userRepository.Query().FirstOrDefault(x => x.Id == user.SuperAdminId);

                                    if (superAdmin == null) continue;

                                    List<KeyValuePair<string, string>> EmailParameters = new()
                                    {
                                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_USERNAME, superAdmin.FirstName),
                                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_TEAMMEMBER_NAME, user.FirstName),
                                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_LOGO_URL, _appSettings.Value.LOGO),
                                    };


                                    var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.CONTRACT_EXPIRY_NOTIFICATION_FILENAME, EmailParameters);
                                    var SendEmail = _emailHandler.SendEmail(superAdmin.Email, "CONTRACT EXPIRATION", EmailTemplate, "");
                                }
                            
                                
                                if (DateTime.Now.Date > contract.EndDate.Date && contract.StatusId == (int)Statuses.ACTIVE)
                                {
                                    contract.StatusId = (int)Statuses.TERMINATED;
                                    _contractRepository.Update(contract);
                                }
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
