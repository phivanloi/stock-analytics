using Pl.Sas.Core;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Core.Trading;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Pl.Sas.InvestmentPrinciplesTests
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private IMarketData _marketData = null!;
        private readonly IServiceProvider _serviceProvider;
        public Worker(
            ILogger<Worker> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                Console.Clear();
                Console.OutputEncoding = Encoding.UTF8;
                using var scope = _serviceProvider.CreateScope();
                _marketData = scope.ServiceProvider.GetRequiredService<IMarketData>();
                DateTime fromDate = new(2018, 6, 1);
                DateTime toDate = new(2019, 10, 1);
                var symbol = "FPT";
                var chartPrices = (await _marketData.GetChartPricesAsync(symbol)).OrderBy(q => q.TradingDate).ToList();
                MacdTrading.BuildIndicatorSet(chartPrices);
                //BTrading.ShowIndicator();
                var tradingCase = MacdTrading.BuildCase(true);
                var tradingHistories = chartPrices.Where(q => q.TradingDate >= fromDate).OrderBy(q => q.TradingDate).ToList();
                var startPrice = tradingHistories[0].ClosePrice;
                var endPrice = tradingHistories[^1].ClosePrice;
                var (isBuy, isSell) = MacdTrading.Trading(tradingCase, tradingHistories);
                var lastChartPrice = tradingHistories[^1];
                var optimalBuyPrice = MacdTrading.CalculateOptimalBuyPrice(lastChartPrice.OpenPrice, lastChartPrice.LowestPrice, lastChartPrice.ClosePrice);
                var optimalSellPrice = MacdTrading.CalculateOptimalSellPrice(lastChartPrice.OpenPrice, lastChartPrice.HighestPrice, lastChartPrice.ClosePrice);
                Console.WriteLine($"Quá trình đầu tư ngắn hạn:");
                Console.WriteLine($"Bắt đầu--------------------------------");
                foreach (var note in tradingCase.ExplainNotes)
                {
                    note.Value.WriteConsole(note.Key > 0 ? ConsoleColor.Green : note.Key < 0 ? ConsoleColor.Red : ConsoleColor.White);
                }
                Console.WriteLine($"Kết thúc--------------------------------");
                Console.WriteLine();
                Console.WriteLine($"Hôm nay đánh giá mua: {(isBuy ? "mua" : "theo dõi")}");
                Console.WriteLine($"Hôm nay đánh giá bán: {(isSell ? "bán" : "theo dõi")}");
                Console.WriteLine($"Hôm nay mua với giá: {optimalBuyPrice:0.00}");
                Console.WriteLine($"Hôm nay bán với giá: {optimalSellPrice:0.00}");
                Console.WriteLine();
                tradingCase.ExplainNotes = new();
                JsonSerializer.Serialize(tradingCase, new JsonSerializerOptions() { WriteIndented = true }).WriteConsole(ConsoleColor.Yellow);
                Console.WriteLine();
                Console.WriteLine($"kết quả -----------------------------------------------");
                $"Lợi nhuận {tradingCase.Profit(lastChartPrice.ClosePrice):0,0} ({tradingCase.ProfitPercent(lastChartPrice.ClosePrice):0,0.00}%), thuế {tradingCase.TotalTax:0,0}".WriteConsole(tradingCase.FixedCapital <= tradingCase.Profit(lastChartPrice.ClosePrice) ? ConsoleColor.Green : ConsoleColor.Red);
                Console.WriteLine();
                Console.WriteLine($"Với chỉ mua và nắm giữ --------------------------------");
                $"Thực hiện đầu tư {tradingHistories.Count} phiên: Giá đóng cửa đầu kỳ {tradingHistories[0].TradingDate:dd-MM-yyyy}: {startPrice * 1000:0,0.00} giá đóng cửa cuối kỳ {tradingHistories[^1].TradingDate:dd-MM-yyyy}: {endPrice * 1000:0,0.00} lợi nhuận {endPrice.GetPercent(startPrice):0.00}%.".WriteConsole(endPrice > startPrice ? ConsoleColor.White : ConsoleColor.Red);
                Console.WriteLine();
                //foreach (var indicator in indicatorSet)
                //{
                //    if (indicator.Value.Values.ContainsKey("ema-12"))
                //    {
                //        $"Ngay: {indicator.Key}\t C: {indicator.Value.ClosePrice:00.00}\t Ema-9: {indicator.Value.Values["ema-9"]:00.0000}\t Ema-12: {indicator.Value.Values["ema-12"]:00.0000}".WriteConsole(ConsoleColor.White);
                //    }
                //    else
                //    {
                //        $"Ngay: {indicator.Key}\t C: {indicator.Value.ClosePrice:00.00}\t Ema-9: \t Ema-12: ".WriteConsole(ConsoleColor.White);
                //    }
                //}

                //await EmaStochTradingV2Async("HPG", new(2017, 1, 1), null);
                //await EmaStochTradingAsync("VND", new(2021, 1, 1), null);
                //await FindIndicatorsTradingAsync("VND", new(2020, 1, 1), null);
            }
            catch (Exception ex)
            {
                ex.ToString().WriteConsole(ConsoleColor.Red);
                _logger.LogError(ex, "");
            }
        }
    }
}