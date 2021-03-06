using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pl.Sas.Core.Services;
using Pl.Sas.Infrastructure.Loging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pl.Sas.UnitTests
{
    public class StockAnalyticsServiceTests
    {
        [Fact]
        public async Task MarketSentimentAnalyticsTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var analyticsService = serviceProvider.GetService<AnalyticsService>() ?? throw new Exception("Can't get WorkerService");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            await analyticsService.MarketSentimentAnalyticsAsync(new()
            {
                DataKey = "VNINDEX"
            });
            Assert.True(true);

            await hostedService.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task TestTradingAnalyticsTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var analyticsService = serviceProvider.GetService<AnalyticsService>() ?? throw new Exception("Can't get WorkerService");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            await analyticsService.TestTradingAnalyticsAsync("THN");
            Assert.True(true);

            await hostedService.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task StockPriceTechnicalAnalyticsTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var analyticsService = serviceProvider.GetService<AnalyticsService>() ?? throw new Exception("Can't get WorkerService");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            await analyticsService.StockPriceTechnicalAnalyticsAsync("BCB");
            Assert.True(true);

            await hostedService.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task CompanyValueAnalyticsTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var analyticsService = serviceProvider.GetService<AnalyticsService>() ?? throw new Exception("Can't get WorkerService");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            await analyticsService.CompanyValueAnalyticsAsync("VIC");
            Assert.True(true);

            await hostedService.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task CompanyGrowthAnalyticsTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var analyticsService = serviceProvider.GetService<AnalyticsService>() ?? throw new Exception("Can't get WorkerService");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            await analyticsService.CompanyGrowthAnalyticsAsync("VIC");
            Assert.True(true);

            await hostedService.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task FiinAnalyticsTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var analyticsService = serviceProvider.GetService<AnalyticsService>() ?? throw new Exception("Can't get WorkerService");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            await analyticsService.FiinAnalyticsAsync("BSR");
            Assert.True(true);

            await hostedService.StopAsync(CancellationToken.None);
        }
    }
}