using Microsoft.Extensions.Options;
using Pl.Sas.Core;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Core.Services;

namespace Pl.Sas.Worker
{
    /// <summary>
    /// Thực hiện tạo task từ bảng
    /// </summary>
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly AppSettings _appSettings;
        private readonly IWorkerQueueService _workerQueueService;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public Worker(
            IServiceScopeFactory serviceScopeFactory,
            IWorkerQueueService workerQueueService,
            IOptions<AppSettings> optionsAppSettings,
            ILogger<Worker> logger)
        {
            _workerQueueService = workerQueueService;
            _logger = logger;
            _appSettings = optionsAppSettings.Value;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(100000, stoppingToken);
            }
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Worker {version} starting at: {time}", _appSettings.AppVersion, DateTimeOffset.Now);

            _workerQueueService.SubscribeDownloadTask(async (message) =>
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var downloadService = scope.ServiceProvider.GetRequiredService<DownloadService>();
                await downloadService.HandleEventAsync(message);
                scope.Dispose();
            });

            _workerQueueService.SubscribeAnalyticsTask(async (message) =>
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var analyticsService = scope.ServiceProvider.GetRequiredService<AnalyticsService>();
                await analyticsService.HandleEventAsync(message);
                scope.Dispose();
            });

            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Worker {version} stopping at: {time}", _appSettings.AppVersion, DateTimeOffset.Now);
            _workerQueueService.Dispose();
            return base.StopAsync(cancellationToken);
        }
    }
}
