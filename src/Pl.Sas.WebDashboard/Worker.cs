using Microsoft.Extensions.Options;
using Pl.Sas.Core;
using Pl.Sas.Core.Interfaces;

namespace Pl.Sas.WebDashboard
{
    /// <summary>
    /// Thực hiện tạo task từ bảng
    /// </summary>
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly AppSettings _appSettings;
        private readonly IWebDashboardQueueService _webDashboardQueueService;
        private readonly IMemoryUpdateService _memoryUpdateService;

        public Worker(
            IMemoryUpdateService memoryUpdateService,
            IWebDashboardQueueService webDashboardQueueService,
            IOptions<AppSettings> optionsAppSettings,
            ILogger<Worker> logger)
        {
            _webDashboardQueueService = webDashboardQueueService;
            _logger = logger;
            _appSettings = optionsAppSettings.Value;
            _memoryUpdateService = memoryUpdateService;
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

            _webDashboardQueueService.SubscribeUpdateMemoryTask((message) =>
            {
                _memoryUpdateService.HandleUpdate(message);
            });

            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Worker {version} stopping at: {time}", _appSettings.AppVersion, DateTimeOffset.Now);
            _webDashboardQueueService.Dispose();
            return base.StopAsync(cancellationToken);
        }
    }
}
