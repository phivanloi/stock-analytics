using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pl.Sas.Core;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Core.Services;
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
        public async Task BindingStocksViewAndSetCacheTestAsync()
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
    }
}