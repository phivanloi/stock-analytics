using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Core.Services;
using Pl.Sas.Infrastructure.Loging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pl.Sas.UnitTests
{
    public class DataTests
    {
        [Fact]
        public async Task StockBulkUpdateTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var stockData = serviceProvider.GetService<IStockData>() ?? throw new Exception("Can't get StockViewService");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            await stockData.BulkUpdateAsync(new System.Collections.Generic.List<Core.Entities.Stock>()
            {
                new Core.Entities.Stock(){
                    Id = "0590EMEDKE2DGKDIGPOS2A",
                    Symbol = "RAL",
                    Name = "RAL",
                    FullName = "RAL",
                    Exchange = "HOSE",
                    Type = "s"
                }
            });
            Assert.True(true);

            await hostedService.StopAsync(CancellationToken.None);
        }
    }
}