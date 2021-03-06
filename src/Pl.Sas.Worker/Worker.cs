using Microsoft.Extensions.Options;
using Pl.Sas.Core;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Core.Services;

namespace Pl.Sas.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly AppSettings _appSettings;
        private readonly IWorkerQueueService _workerQueueService;
        private readonly DownloadService _downloadService;
        private readonly AnalyticsService _analyticsService;
        private readonly StockViewService _stockViewService;
        private readonly RealtimeService _realtimeService;
        private readonly IMemoryUpdateService _memoryUpdateService;

        public Worker(
            IMemoryUpdateService memoryUpdateService,
            RealtimeService realtimeService,
            StockViewService stockViewService,
            AnalyticsService analyticsService,
            DownloadService downloadService,
            IWorkerQueueService workerQueueService,
            IOptions<AppSettings> optionsAppSettings,
            ILogger<Worker> logger)
        {
            _workerQueueService = workerQueueService;
            _logger = logger;
            _appSettings = optionsAppSettings.Value;
            _downloadService = downloadService;
            _analyticsService = analyticsService;
            _stockViewService = stockViewService;
            _realtimeService = realtimeService;
            _memoryUpdateService = memoryUpdateService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(10000, stoppingToken);
            }
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Worker {version} starting at: {time}", _appSettings.AppVersion, DateTimeOffset.Now);

            _workerQueueService.SubscribeUpdateMemoryTask((message) =>
            {
                _memoryUpdateService.HandleUpdate(message);
                GC.Collect();
            });

            _workerQueueService.SubscribeDownloadTask(async (message) =>
            {
                await _downloadService.HandleEventAsync(message);
                GC.Collect();
            });

            _workerQueueService.SubscribeAnalyticsTask(async (message) =>
            {
                await _analyticsService.HandleEventAsync(message);
                GC.Collect();
            });

            _workerQueueService.SubscribeBuildViewTask(async (message) =>
            {
                var queueMessage = await _stockViewService.HandleStockViewEventAsync(message);
                if (queueMessage is not null)
                {
                    _workerQueueService.BroadcastViewUpdatedTask(queueMessage);
                }
                GC.Collect();
            });

            _workerQueueService.SubscribeRealtimeTask(async (message) =>
            {
                await _realtimeService.HandleEventAsync(message);
                GC.Collect();
            });

            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Worker {version} stopping at: {time}", _appSettings.AppVersion, DateTimeOffset.Now);
            _workerQueueService.Dispose();
            return base.StopAsync(cancellationToken);
        }
    }
}
