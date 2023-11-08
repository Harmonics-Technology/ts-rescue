using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Repositories.Interfaces;
using TimesheetBE.Utilities;

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


                            var allContract = _contractRepository.Query().ToList();

                            foreach (var contract in allContract)
                            {
                                if(DateTime.Now.Date > contract.EndDate.Date)
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
