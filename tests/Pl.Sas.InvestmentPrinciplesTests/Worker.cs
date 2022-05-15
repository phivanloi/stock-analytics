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
                DateTime fromDate = new(2021, 1, 1);
                DateTime? toDate = null;
                var stockPrices = (await _marketData.GetForTradingAsync("VND", fromDate.AddYears(-1000), toDate)).OrderBy(q => q.TradingDate).ToList();
                var indicatorSet = BaseTrading.BuildIndicatorSetV2(stockPrices);
                foreach (var indicator in indicatorSet)
                {
                    if (indicator.Value.Values.ContainsKey("ema-12"))
                    {
                        $"Ngay: {indicator.Key}\t C: {indicator.Value.ClosePrice/1000:00.00}\t Ema-9: {indicator.Value.Values["ema-9"] / 1000:00.00}\t Ema-12: {indicator.Value.Values["ema-12"] / 1000:00.00}".WriteConsole(ConsoleColor.White);
                    }
                    else
                    {
                        $"Ngay: {indicator.Key}\t C: {indicator.Value.ClosePrice / 1000:00.00}\t Ema-9: \t Ema-12: ".WriteConsole(ConsoleColor.White);
                    }
                }

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

        private async Task EmaStochTradingAsync(string symbol, DateTime fromDate, DateTime? toDate)
        {
            var stock = await _marketData.GetStockByCode(symbol);
            if (stock is null)
            {
                return;
            }

            var stockPrices = await _marketData.GetForTradingAsync(symbol, fromDate.AddYears(-1000), toDate);
            var indicatorSet = BaseTrading.BuildIndicatorSet(stockPrices);
            var tradingHistories = stockPrices.Where(q => q.TradingDate >= fromDate).OrderBy(q => q.TradingDate).ToList();
            var tradingCases = new List<TradingCase>() {
                new TradingCase(){
                      FixedCapital = 10000000,
                      FirstEmaBuy= 12,
                      SecondEmaBuy= 36,
                      FirstEmaSell= 12,
                      SecondEmaSell= 36,
                      Stochastic= 14,
                      HighestStochasticSell = 50,
                      LowestStochasticBuy = 50,
                }
            };
            var totalCase = 1;// EmaStochTradingV2.GetNumberCase();
            //var tradingCases = EmaStochTradingV2.BuildTradingCases();
            //var totalCase = EmaStochTradingV2.GetNumberCase();
            var startPrice = tradingHistories[0].ClosePrice;
            var endPrice = tradingHistories[^1].ClosePrice;
            Stopwatch stopWatch = new();
            stopWatch.Start();

            var testCaseCount = 0;
            var winCase = 0;
            var loseCase = 0;
            var goodResult = new TradingCase();
            foreach (var tradingCase in tradingCases)
            {
                testCaseCount++;
                Console.Write($"\r{testCaseCount}/{totalCase} cases.");
                EmaStochTrading.Trading(tradingCase, tradingHistories, indicatorSet);
                if (goodResult.TradingTestResult < tradingCase.TradingTestResult)
                {
                    goodResult = tradingCase;
                }
                if (tradingCase.TradingTestResult > tradingCase.FixedCapital)
                {
                    winCase++;
                }
                else
                {
                    loseCase++;
                }
            }

            _logger.LogInformation(JsonSerializer.Serialize(goodResult, new JsonSerializerOptions() { WriteIndented = true }));
            var (isBuy, isSell) = EmaStochTrading.Trading(goodResult, tradingHistories, indicatorSet, true);
            var lastStockPrice = tradingHistories[^1];
            var optimalBuyPrice = EmaStochTrading.CalculateOptimalBuyPrice(lastStockPrice);
            var optimalSellPrice = EmaStochTrading.CalculateOptimalSellPriceOnLoss(lastStockPrice);
            stopWatch.Stop();

            Console.WriteLine($"Quá trình đầu tư ngắn hạn:");
            Console.WriteLine($"Bắt đầu--------------------------------");
            foreach (var note in goodResult.ExplainNotes)
            {
                note.Value.WriteConsole(note.Key > 0 ? ConsoleColor.Green : note.Key < 0 ? ConsoleColor.Red : ConsoleColor.White);
            }
            Console.WriteLine($"Kết thúc--------------------------------");
            Console.WriteLine();
            Console.WriteLine($"Hôm nay đánh giá mua: {(isBuy ? "mua" : "theo dõi")}");
            Console.WriteLine($"Hôm nay đánh giá bán: {(isSell ? "bán" : "theo dõi")}");
            Console.WriteLine($"Hôm nay mua với giá: {optimalBuyPrice / 1000:0.00}");
            Console.WriteLine($"Hôm nay bán với giá: {optimalSellPrice / 1000:0.00}");
            Console.WriteLine();
            $"Quá trình đầu tư cần {stopWatch.ElapsedMilliseconds} miniseconds. Như vậy 1700 mã chạy 30 worker cần {(stopWatch.ElapsedMilliseconds * 1700) / 1800000} phút. ".WriteConsole(ConsoleColor.Yellow);
            goodResult.ExplainNotes = new();
            JsonSerializer.Serialize(goodResult, new JsonSerializerOptions() { WriteIndented = true }).WriteConsole(ConsoleColor.Yellow);
            Console.WriteLine();
            Console.WriteLine($"kết quả -----------------------------------------------");
            $"Thực hiện đầu cơ thử nghiệm mã {symbol} với {testCaseCount} bộ chỉ số trên {tradingHistories.Count} phiên giao dịch. tỉ lệ thắng/thua: {winCase}/{loseCase}".WriteConsole(ConsoleColor.White);
            goodResult.ResultString().WriteConsole(goodResult.FixedCapital <= goodResult.TradingTestResult ? ConsoleColor.Green : ConsoleColor.Red);
            Console.WriteLine();
            Console.WriteLine($"Với chỉ mua và nắm giữ --------------------------------");
            $"Thực hiện đầu tư {tradingHistories.Count} phiên: Giá đóng cửa đầu kỳ {tradingHistories[0].TradingDate:dd-MM-yyyy}:{startPrice:0,0} giá đóng cửa cuối kỳ {tradingHistories[^1].TradingDate:dd-MM-yyyy}:{endPrice:0,0} lợi nhuận {endPrice.GetPercent(startPrice):0.00}%.".WriteConsole(endPrice > startPrice ? ConsoleColor.White : ConsoleColor.Red);
        }
    }
}