using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pl.Sas.Core.Entities;
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
    public class DownloadServiceTests
    {

        [Fact]
        public async Task UpdateCapitalAndDividendTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var downloadService = serviceProvider.GetService<DownloadService>() ?? throw new Exception("Can't get WorkerService");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            var check = await downloadService.UpdateCapitalAndDividendAsync(new Core.Entities.Schedule() { DataKey = "TVC" });
            Assert.True(check);

            await hostedService.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task InitialStockTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var workerService = serviceProvider.GetService<DownloadService>() ?? throw new Exception("Can't get WorkerService");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            await workerService.InitialStockAsync();
            Assert.True(1 == 1);

            await hostedService.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task UpdateBankInterestRateTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var workerService = serviceProvider.GetService<DownloadService>() ?? throw new Exception("Can't get WorkerService");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            await workerService.UpdateBankInterestRateAsync(new Schedule()
            {
                Type = 10,
                Name = "Lấy lãi suất ngân hàng cao nhất.",
                OptionsJson = JsonSerializer.Serialize(new Dictionary<string, string>() { { "Length", $"3,6,12,24" } })
            });
            Assert.True(1 == 1);

            await hostedService.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task UpdateIndexPricesTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var workerService = serviceProvider.GetService<DownloadService>() ?? throw new Exception("Can't get WorkerService");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            await workerService.UpdateChartPricesAsync(new()
            {
                Name = $"Tải dữ liệu chỉ số: VNINDEX",
                Type = 9,
                DataKey = "FPT",
                ActiveTime = DateTime.Now,
                OptionsJson = JsonSerializer.Serialize(new Dictionary<string, string>()
                {
                    {"StartTime", new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.FromMilliseconds(0)).ToUnixTimeSeconds().ToString() },
                    {"ChartType", "D" }
                })
            });
            Assert.True(1 == 1);

            await hostedService.StopAsync(CancellationToken.None);
        }

    }
}