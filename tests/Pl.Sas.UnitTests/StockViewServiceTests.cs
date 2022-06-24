using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pl.Sas.Core;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Core.Services;
using Pl.Sas.Infrastructure.Loging;
using Skender.Stock.Indicators;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Pl.Sas.UnitTests
{
    public class StockViewServiceTests
    {
        private readonly ITestOutputHelper _output;
        public StockViewServiceTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task BindingStocksViewAndSetCacheTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var stockViewService = serviceProvider.GetService<StockViewService>() ?? throw new Exception("Can't get StockViewService");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            await stockViewService.BindingStocksViewAndSetCacheAsync("GAS");
            Assert.True(true);

            await hostedService.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task ZigZagTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var chartPriceData = serviceProvider.GetService<IChartPriceData>() ?? throw new Exception("Can't get IChartPriceData");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            var chartPrices = await chartPriceData.FindAllAsync("HPG", "D");
            var quotes = chartPrices.Select(q => q.ToQuote()).OrderBy(q => q.Date).ToList();
            var zigZagResults = quotes.GetZigZag(EndType.HighLow, 6);
            foreach (var zigZag in zigZagResults)
            {
                _output.WriteLine($"{zigZag.Date:yyyy:MM:dd}, Z:{zigZag.ZigZag:00.00}, PT:{zigZag.PointType ?? " "}, RH:{zigZag.RetraceHigh:00.00}, RL:{zigZag.RetraceLow:00.00}");
            }

            Assert.True(true);

            await hostedService.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task RsiTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var chartPriceData = serviceProvider.GetService<IChartPriceData>() ?? throw new Exception("Can't get IChartPriceData");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            var chartPrices = await chartPriceData.FindAllAsync("VND");
            var quotes = chartPrices.Select(q => q.ToQuote()).OrderBy(q => q.Date).ToList();
            var rsiResults = quotes.GetRsi(14);
            foreach (var rsi in rsiResults)
            {
                _output.WriteLine($"{rsi.Date:yyyy:MM:dd}, Z:{rsi.Rsi:00.00}");
            }

            Assert.True(true);

            await hostedService.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task DojiCandlestickPatternsTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var chartPriceData = serviceProvider.GetService<IChartPriceData>() ?? throw new Exception("Can't get IChartPriceData");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            var chartPrices = await chartPriceData.FindAllAsync("DCM");
            var quotes = chartPrices.Select(q => q.ToQuote()).OrderBy(q => q.Date).ToList();
            var dojiResults = quotes.GetDoji(0.1);
            foreach (var doji in dojiResults)
            {
                _output.WriteLine($"{doji.Date:yyyy:MM:dd}, doji:{doji.Match}, json {System.Text.Json.JsonSerializer.Serialize(doji)}");
            }

            Assert.True(true);

            await hostedService.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task MarubozuCandlestickPatternsTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var chartPriceData = serviceProvider.GetService<IChartPriceData>() ?? throw new Exception("Can't get IChartPriceData");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            var chartPrices = await chartPriceData.FindAllAsync("DCM");
            var quotes = chartPrices.Select(q => q.ToQuote()).OrderBy(q => q.Date).ToList();
            var dojiResults = quotes.GetMarubozu(95);
            foreach (var doji in dojiResults)
            {
                _output.WriteLine($"{doji.Date:yyyy:MM:dd}, doji:{doji.Match}, json {System.Text.Json.JsonSerializer.Serialize(doji)}");
            }

            Assert.True(true);

            await hostedService.StopAsync(CancellationToken.None);
        }
    }
}