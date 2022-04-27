using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Infrastructure.Loging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pl.Sas.UnitTests
{
    public class CrawlTests
    {
        [Fact]
        public async Task DownloadInitialMarketStockTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var crawlData = serviceProvider.GetService<ICrawlData>() ?? throw new Exception("Can't get ICrawlData");
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
            var crawlData = serviceProvider.GetService<ICrawlData>() ?? throw new Exception("Can't get ICrawlData");
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
            var crawlData = serviceProvider.GetService<ICrawlData>() ?? throw new Exception("Can't get ICrawlData");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            var capitalAndDividend = await crawlData.DownloadCapitalAndDividendAsync("TVC");
            Assert.True(capitalAndDividend != null);

            await hostedService.StopAsync(CancellationToken.None);
        }
    }
}