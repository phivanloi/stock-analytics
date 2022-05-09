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
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public Worker(
            IServiceScopeFactory serviceScopeFactory,
            ISchedulerQueueService schedulerQueueService,
            IOptions<AppSettings> optionsAppSettings,
            ILogger<Worker> logger)
        {
            _schedulerQueueService = schedulerQueueService;
            _logger = logger;
            _appSettings = optionsAppSettings.Value;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Scheduler running at: {time}", DateTimeOffset.Now);
            using var scope = _serviceScopeFactory.CreateScope();
            while (!stoppingToken.IsCancellationRequested)
            {
                var marketDbContext = scope.ServiceProvider.GetRequiredService<MarketDbContext>();
                var schedules = marketDbContext.Schedules.Where(q => q.ActiveTime <= DateTime.Now).OrderBy(q => q.ActiveTime).Take(10).ToList();
                if (schedules.Count > 0)
                {
                    foreach (var schedule in schedules)
                    {
                        if (schedule.Type < 99)
                        {
                            _schedulerQueueService.PublishDownloadTask(new(schedule.Id));
                        }
                        
                        schedule.ApplyActiveTime(DateTime.Now);
                    }
                    marketDbContext.SaveChanges();
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
