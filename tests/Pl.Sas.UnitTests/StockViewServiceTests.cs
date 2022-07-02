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
    public class StockViewServiceTests
    {
        [Fact]
        public async Task BindingStocksViewAndSetCacheTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var stockViewService = serviceProvider.GetService<StockViewService>() ?? throw new Exception("Can't get StockViewService");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            await stockViewService.BindingStocksViewAndSetCacheAsync("HAG");
            Assert.True(true);

            await hostedService.StopAsync(CancellationToken.None);
        }
    }
}