using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pl.Sas.Core;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Infrastructure.Loging;
using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Pl.Sas.UnitTests
{
    public class IndicatorsTests
    {
        private readonly ITestOutputHelper _output;
        public IndicatorsTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task GetSlopeTest()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var chartPriceData = serviceProvider.GetService<IChartPriceData>() ?? throw new Exception("Can't get IChartPriceData");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            var listQuotes = new List<Quote>() {
                new Quote() {
                    Date = DateTime.Now.Date,
                    Close = 10
                },
                new Quote() {
                    Date = DateTime.Now.Date.AddDays(-1),
                    Close = 12
                },
                new Quote() {
                    Date = DateTime.Now.Date.AddDays(-2),
                    Close = 8
                },
                new Quote() {
                    Date = DateTime.Now.Date.AddDays(-3),
                    Close = 9
                },
                new Quote() {
                    Date = DateTime.Now.Date.AddDays(-4),
                    Close = 7
                },
                new Quote() {
                    Date = DateTime.Now.Date.AddDays(-5),
                    Close = 4
                },
            };
            listQuotes = listQuotes.OrderBy(q => q.Date).ToList();
            var slopes = listQuotes.GetSlope(3);
            foreach (var slope in slopes)
            {
                _output.WriteLine($"{slope.Date:yyyy:MM:dd}, Slope:{slope.Slope:00.00}, Intercept:{slope.Intercept:00.00}, StdDev:{slope.StdDev:00.00}, RSquared:{slope.RSquared:00.00}, Line:{slope.Line:00.00}");
            }

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
        public async Task ParabolicSarTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var chartPriceData = serviceProvider.GetService<IChartPriceData>() ?? throw new Exception("Can't get IChartPriceData");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            var chartPrices = await chartPriceData.FindAllAsync("VND", "D");
            var quotes = chartPrices.Select(q => q.ToQuote()).OrderBy(q => q.Date).ToList();
            var reverseSignals = quotes.GetParabolicSar(0.02, 0.2).ToList();
            foreach (var reverseSignal in reverseSignals)
            {
                _output.WriteLine($"{reverseSignal.Date:yyyy:MM:dd}, Sar:{reverseSignal.Sar:00.00}, IsReversal:{reverseSignal.IsReversal ?? false}");
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

            var chartPrices = await chartPriceData.FindAllAsync("VND");
            var quotes = chartPrices.Select(q => q.ToQuote()).OrderBy(q => q.Date).ToList();
            var dojiResults = quotes.GetDoji(0.2);
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

            var chartPrices = await chartPriceData.FindAllAsync("VND");
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