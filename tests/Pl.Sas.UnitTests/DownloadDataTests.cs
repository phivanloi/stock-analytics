using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Infrastructure.Loging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pl.Sas.UnitTests
{
    public class DownloadDataTests
    {
        [Fact]
        public async Task DownloadInitialMarketStockTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var crawlData = serviceProvider.GetService<IDownloadData>() ?? throw new Exception("Can't get ICrawlData");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            var ssiAllStock = await crawlData.DownloadInitialMarketStockAsync();
            Assert.True(ssiAllStock != null);

            await hostedService.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task DownloadCompanyInfoTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var crawlData = serviceProvider.GetService<IDownloadData>() ?? throw new Exception("Can't get ICrawlData");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            var companyInfo = await crawlData.DownloadCompanyInfoAsync("TVC");
            Assert.True(companyInfo != null);

            await hostedService.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task DownloadCapitalAndDividendTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var crawlData = serviceProvider.GetService<IDownloadData>() ?? throw new Exception("Can't get ICrawlData");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            var capitalAndDividend = await crawlData.DownloadCapitalAndDividendAsync("HTH");
            Assert.True(capitalAndDividend != null);

            await hostedService.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task DownloadStockPricesTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var crawlData = serviceProvider.GetService<IDownloadData>() ?? throw new Exception("Can't get ICrawlData");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            var ssiStockPrices = await crawlData.DownloadStockPricesAsync("TVC");
            Assert.True(ssiStockPrices != null);

            await hostedService.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task DownloadTransactionTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var crawlData = serviceProvider.GetService<IDownloadData>() ?? throw new Exception("Can't get ICrawlData");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            var transactions = await crawlData.DownloadTransactionAsync("hnx:12072");
            Assert.True(transactions != null);

            await hostedService.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task DownloadFiinStockEvaluatesTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var crawlData = serviceProvider.GetService<IDownloadData>() ?? throw new Exception("Can't get ICrawlData");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            var fiinStockEvaluates = await crawlData.DownloadFiinStockEvaluatesAsync("HNR");
            Assert.True(fiinStockEvaluates != null);

            await hostedService.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task DownloadBankInterestRateTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var crawlData = serviceProvider.GetService<IDownloadData>() ?? throw new Exception("Can't get ICrawlData");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            var bankInterestRate = await crawlData.DownloadBankInterestRateAsync();
            Assert.True(bankInterestRate != null);

            await hostedService.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task DownloadSsiChartPricesRealTimeTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var crawlData = serviceProvider.GetService<IDownloadData>() ?? throw new Exception("Can't get ICrawlData");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            var chartRealtime = await crawlData.DownloadSsiChartPricesRealTimeAsync("VND");
            Assert.True(chartRealtime != null);

            await hostedService.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task DownloadVndChartPricesRealTimeTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var crawlData = serviceProvider.GetService<IDownloadData>() ?? throw new Exception("Can't get ICrawlData");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            var chartRealtime = await crawlData.DownloadVndChartPricesRealTimeAsync("VND");
            Assert.True(chartRealtime != null);

            await hostedService.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task DownloadVpsChartPricesRealTimeTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var crawlData = serviceProvider.GetService<IDownloadData>() ?? throw new Exception("Can't get ICrawlData");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            var chartRealtime = await crawlData.DownloadVpsChartPricesRealTimeAsync("VND");
            Assert.True(chartRealtime != null);

            await hostedService.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task DownloadMarketInDepthTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var crawlData = serviceProvider.GetService<IDownloadData>() ?? throw new Exception("Can't get ICrawlData");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            var chartRealtime = await crawlData.DownloadMarketInDepthAsync();
            Assert.True(chartRealtime != null);

            await hostedService.StopAsync(CancellationToken.None);
        }
    }
}