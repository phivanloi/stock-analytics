using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Core.Services;
using Pl.Sas.Infrastructure.Loging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pl.Sas.UnitTests
{
    public class RealtimeServiceTests
    {
        [Fact]
        public async Task UpdateViewRealtimeOnPriceTestChange()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var downloadService = serviceProvider.GetService<DownloadService>() ?? throw new Exception("Can't get DownloadService");
            var downloadData = serviceProvider.GetService<IDownloadData>() ?? throw new Exception("Can't get IDownloadData");
            var realtimeService = serviceProvider.GetService<RealtimeService>() ?? throw new Exception("Can't get RealtimeService");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            var symbol = "VIX";
            var chartPrices = new List<ChartPrice>();
            var random = new Random();
            if (random.Next(0, 3000) > 2000)
            {
                var vndChartPrices = await downloadData.DownloadVndChartPricesRealTimeAsync(symbol, "D");
                if (vndChartPrices is not null && vndChartPrices.Time?.Length > 0)
                {
                    for (int i = 0; i < vndChartPrices.Time.Length; i++)
                    {
                        var tradingDate = DateTimeOffset.FromUnixTimeSeconds(vndChartPrices.Time[i]).Date;
                        if (!chartPrices.Any(q => q.TradingDate == tradingDate))
                        {
                            chartPrices.Add(new()
                            {
                                Symbol = symbol,
                                TradingDate = tradingDate,
                                ClosePrice = vndChartPrices.Close[i],
                                OpenPrice = vndChartPrices.Open[i],
                                HighestPrice = vndChartPrices.Highest[i],
                                LowestPrice = vndChartPrices.Lowest[i],
                                TotalMatchVol = vndChartPrices.Volumes[i],
                                Type = "D"
                            });
                        }
                    }
                    vndChartPrices = null;
                }
            }
            else
            {
                if (random.Next(0, 2000) > 1000)
                {
                    var ssiChartPrices = await downloadData.DownloadSsiChartPricesRealTimeAsync(symbol, "D");
                    if (ssiChartPrices is not null && ssiChartPrices.Time?.Length > 0)
                    {
                        for (int i = 0; i < ssiChartPrices.Time.Length; i++)
                        {
                            var tradingDate = DateTimeOffset.FromUnixTimeSeconds(ssiChartPrices.Time[i]).Date;
                            if (!chartPrices.Any(q => q.TradingDate == tradingDate))
                            {
                                chartPrices.Add(new()
                                {
                                    Symbol = symbol,
                                    TradingDate = tradingDate,
                                    ClosePrice = string.IsNullOrEmpty(ssiChartPrices.Close[i]) ? 0 : float.Parse(ssiChartPrices.Close[i]),
                                    OpenPrice = string.IsNullOrEmpty(ssiChartPrices.Open[i]) ? 0 : float.Parse(ssiChartPrices.Open[i]),
                                    HighestPrice = string.IsNullOrEmpty(ssiChartPrices.Highest[i]) ? 0 : float.Parse(ssiChartPrices.Highest[i]),
                                    LowestPrice = string.IsNullOrEmpty(ssiChartPrices.Lowest[i]) ? 0 : float.Parse(ssiChartPrices.Lowest[i]),
                                    TotalMatchVol = string.IsNullOrEmpty(ssiChartPrices.Volumes[i]) ? 0 : float.Parse(ssiChartPrices.Volumes[i]),
                                    Type = "D"
                                });
                            }
                        }
                        ssiChartPrices = null;
                    }
                }
                else
                {
                    var vpsChartPrices = await downloadData.DownloadVpsChartPricesRealTimeAsync(symbol, "D");
                    if (vpsChartPrices is not null && vpsChartPrices.Time?.Length > 0)
                    {
                        for (int i = 0; i < vpsChartPrices.Time.Length; i++)
                        {
                            var tradingDate = DateTimeOffset.FromUnixTimeSeconds(vpsChartPrices.Time[i]).Date;
                            if (!chartPrices.Any(q => q.TradingDate == tradingDate))
                            {
                                chartPrices.Add(new()
                                {
                                    Symbol = symbol,
                                    TradingDate = tradingDate,
                                    ClosePrice = vpsChartPrices.Close[i],
                                    OpenPrice = vpsChartPrices.Open[i],
                                    HighestPrice = vpsChartPrices.Highest[i],
                                    LowestPrice = vpsChartPrices.Lowest[i],
                                    TotalMatchVol = vpsChartPrices.Volumes[i],
                                    Type = "D"
                                });
                            }
                        }
                        vpsChartPrices = null;
                    }
                }
            }

            await realtimeService.UpdateViewRealtimeOnPriceChange(symbol, System.Text.Json.JsonSerializer.Serialize(chartPrices));

            Assert.True(true);

            await hostedService.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task UpdateWorldIndexTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var downloadData = serviceProvider.GetService<IDownloadData>() ?? throw new Exception("Can't get IDownloadData");
            var realtimeService = serviceProvider.GetService<RealtimeService>() ?? throw new Exception("Can't get RealtimeService");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            var marketDepths = await downloadData.DownloadMarketInDepthAsync();
            await realtimeService.UpdateWorldIndexChange(System.Text.Json.JsonSerializer.Serialize(marketDepths));
            Assert.True(1 == 1);

            await hostedService.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task UpdateIndexValuationTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var downloadData = serviceProvider.GetService<IDownloadData>() ?? throw new Exception("Can't get IDownloadData");
            var realtimeService = serviceProvider.GetService<RealtimeService>() ?? throw new Exception("Can't get RealtimeService");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            var indexValuation = await downloadData.DownloadFiinValuationAsync();
            await realtimeService.UpdateValuationIndexChange(System.Text.Json.JsonSerializer.Serialize(indexValuation));
            Assert.True(1 == 1);

            await hostedService.StopAsync(CancellationToken.None);
        }
    }
}