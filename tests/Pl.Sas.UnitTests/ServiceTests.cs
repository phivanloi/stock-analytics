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
    public class ServiceTests
    {

        [Fact]
        public async Task UpdateCapitalAndDividendTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var stockViewService = serviceProvider.GetService<StockViewService>() ?? throw new Exception("Can't get WorkerService");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            var message = await stockViewService.BindingStocksViewAndSetCacheAsync("HPG");
            Assert.True(message is not null);

            await hostedService.StopAsync(CancellationToken.None);
        }

    }
}