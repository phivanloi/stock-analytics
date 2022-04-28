using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pl.Sas.Core.Services;
using Pl.Sas.Infrastructure.Loging;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pl.Sas.UnitTests
{
    public class WorkerServiceTests
    {
        [Fact]
        public async Task StockDownloadAndAnalyticsTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var workerService = serviceProvider.GetService<WorkerService>() ?? throw new Exception("Can't get WorkerService");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            var message = await workerService.StockDownloadAndAnalyticsAsync(new Core.Entities.Schedule()
            {
                Name = $"Tải và phân tích mã chứng khoán: HPG",
                Type = 10,
                DataKey = "HPG",
                OptionsJson = JsonSerializer.Serialize(new Dictionary<string, string>()
                {
                    {"CorporateActionCrawlSize","10000" },
                    {"StockPricesCrawlSize","10000" }
                })
            });
            Assert.True(message != null);

            await hostedService.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task UpdateCapitalAndDividendTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var workerService = serviceProvider.GetService<WorkerService>() ?? throw new Exception("Can't get WorkerService");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            var check = await workerService.UpdateCapitalAndDividendAsync(new Core.Entities.StockTracking("TVC"));
            Assert.True(check);

            await hostedService.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task InitialStockTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var workerService = serviceProvider.GetService<WorkerService>() ?? throw new Exception("Can't get WorkerService");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            await workerService.InitialStockAsync();
            Assert.True(1 == 1);

            await hostedService.StopAsync(CancellationToken.None);
        }
    }
}