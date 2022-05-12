using Microsoft.Extensions.Options;
using Pl.Sas.Core;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Infrastructure.System;

namespace Pl.Sas.Scheduler
{
    /// <summary>
    /// Thực hiện tạo task từ bảng Schedules
    /// </summary>
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ISchedulerQueueService _schedulerQueueService;
        private readonly AppSettings _appSettings;
        private readonly IServiceProvider _serviceProvider;

        public Worker(
            IServiceProvider serviceProvider,
            ISchedulerQueueService schedulerQueueService,
            IOptions<AppSettings> optionsAppSettings,
            ILogger<Worker> logger)
        {
            _schedulerQueueService = schedulerQueueService;
            _logger = logger;
            _appSettings = optionsAppSettings.Value;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Scheduler running at: {time}", DateTimeOffset.Now);
            var scope = _serviceProvider.CreateScope();
            while (!stoppingToken.IsCancellationRequested)
            {
                var systemDbContext = scope.ServiceProvider.GetRequiredService<SystemDbContext>();
                var schedules = systemDbContext.Schedules.Where(q => q.ActiveTime <= DateTime.Now).OrderBy(q => q.ActiveTime).Take(10).ToList();
                if (schedules.Count > 0)
                {
                    foreach (var schedule in schedules)
                    {
                        if (schedule.Type >= 300)
                        {
                            _schedulerQueueService.PublishViewWorkerTask(new(schedule.Id));
                        }
                        else if (schedule.Type >= 200)
                        {
                            _schedulerQueueService.PublishAnalyticsWorkerTask(new(schedule.Id));
                        }
                        else
                        {
                            _schedulerQueueService.PublishDownloadTask(new(schedule.Id));
                        }
                        schedule.ApplyActiveTime(DateTime.Now);
                    }
                    systemDbContext.SaveChanges();
                }
                else
                {
                    scope.Dispose();
                    await Task.Delay(1000 * 3, stoppingToken);
                    scope = _serviceProvider.CreateScope();
                }
            }
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Scheduler {version} starting at: {time}", _appSettings.AppVersion, DateTimeOffset.Now);
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Scheduler {version} stopping at: {time}", _appSettings.AppVersion, DateTimeOffset.Now);
            _schedulerQueueService.Dispose();
            return base.StopAsync(cancellationToken);
        }
    }
}
