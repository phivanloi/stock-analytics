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
    }
}