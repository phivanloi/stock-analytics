using Microsoft.Extensions.Options;
using Pl.Sas.Core;
using Pl.Sas.Core.Interfaces;

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
        private readonly IScheduleData _scheduleData;

        public Worker(
            IScheduleData scheduleData,
            ISchedulerQueueService schedulerQueueService,
            IOptions<AppSettings> optionsAppSettings,
            ILogger<Worker> logger)
        {
            _schedulerQueueService = schedulerQueueService;
            _logger = logger;
            _appSettings = optionsAppSettings.Value;
            _scheduleData = scheduleData;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Scheduler running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000 * 10, stoppingToken);//Delay for migration db
            while (!stoppingToken.IsCancellationRequested)
            {
                var schedules = await _scheduleData.GetIdsForActiveEventAsync(DateTime.Now, 10);
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

                    if (schedules.Count == 1)
                    {
                        await _scheduleData.SetActiveTimeAsync(schedules[0].Id, schedules[0].ActiveTime);
                    }
                    else
                    {
                        await _scheduleData.BulkSetActiveTimeAsync(schedules);
                    }
                }
                else
                {
                    await Task.Delay(1000 * 3, stoppingToken);
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
