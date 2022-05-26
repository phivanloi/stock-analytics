using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Pl.Sas.Core;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Core.Services;
using Pl.Sas.WebDashboard.RealtimeHub;

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
        private readonly StockViewService _stockViewService;
        private readonly IHubContext<StockRealtimeHub> _stockRealtimeHub;

        public Worker(
            IHubContext<StockRealtimeHub> stockRealtimeHub,
            StockViewService stockViewService,
            IMemoryUpdateService memoryUpdateService,
            IWebDashboardQueueService webDashboardQueueService,
            IOptions<AppSettings> optionsAppSettings,
            ILogger<Worker> logger)
        {
            _webDashboardQueueService = webDashboardQueueService;
            _logger = logger;
            _appSettings = optionsAppSettings.Value;
            _memoryUpdateService = memoryUpdateService;
            _stockViewService = stockViewService;
            _stockRealtimeHub = stockRealtimeHub;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _stockViewService.InitialAsync();
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

            _webDashboardQueueService.SubscribeViewUpdatedTask(async (message) =>
            {
                switch (message.Id)
                {
                    case "UpdateStockView":
                        _stockViewService.UpdateChangeStockView(message);
                        await _stockRealtimeHub.Clients.All.SendAsync("UpdateStockView");
                        break;

                    default:
                        _logger.LogWarning("ViewMessage id {Id}, don't match any function", message.Id);
                        break;
                }
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
