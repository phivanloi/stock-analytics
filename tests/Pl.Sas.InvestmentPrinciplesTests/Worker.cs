using Pl.Sas.Core;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Core.Trading;
using System.Text;

namespace Pl.Sas.InvestmentPrinciplesTests
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IChartPriceData _chartPriceData;
        public Worker(
            IChartPriceData chartPriceData,
            ILogger<Worker> logger)
        {
            _logger = logger;
            _chartPriceData = chartPriceData;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                Console.Clear();
                Console.OutputEncoding = Encoding.UTF8;
                DateTime fromDate = new(2020, 1, 1);
                DateTime toDate = new(2019, 10, 1);
                var symbol = "VND";
                var chartPrices = (await _chartPriceData.FindAllAsync(symbol)).OrderBy(q => q.TradingDate).ToList();
                var tradingCharts = chartPrices.Where(q => q.TradingDate >= fromDate).OrderBy(q => q.TradingDate).ToList();
                var tradingHistory = chartPrices.Where(q => q.TradingDate < fromDate).OrderBy(q => q.TradingDate).ToList();
                var startPrice = tradingCharts[0].ClosePrice;
                var endPrice = tradingCharts[^1].ClosePrice;
                var tradingCase = ExperimentTrading.Trading(tradingCharts, tradingHistory);
                var lastChartPrice = tradingCharts[^1];
                Console.WriteLine($"Quá trình đầu tư ngắn hạn:");
                Console.WriteLine($"Bắt đầu--------------------------------");
                foreach (var note in tradingCase.ExplainNotes)
                {
                    note.Value.WriteConsole(note.Key > 0 ? ConsoleColor.Green : note.Key < 0 ? ConsoleColor.Red : ConsoleColor.White);
                }
                Console.WriteLine($"Kết thúc--------------------------------");
                Console.WriteLine();
                Console.WriteLine($"kết quả ----------------------------------------------- trading {symbol}");
                $"Số lần mua bán thắng/thua {tradingCase.WinNumber}/{tradingCase.LoseNumber}".WriteConsole(tradingCase.FixedCapital <= tradingCase.Profit(lastChartPrice.ClosePrice) ? ConsoleColor.Green : ConsoleColor.Red);
                $"Lợi nhuận {tradingCase.Profit(lastChartPrice.ClosePrice):0,0} ({tradingCase.ProfitPercent(lastChartPrice.ClosePrice):0,0.00}%), thuế {tradingCase.TotalTax:0,0}".WriteConsole(tradingCase.FixedCapital <= tradingCase.Profit(lastChartPrice.ClosePrice) ? ConsoleColor.Green : ConsoleColor.Red);
                tradingCase.AssetPosition.WriteConsole();
                Console.WriteLine();
                Console.WriteLine($"Với chỉ mua và nắm giữ -------------------------------- {symbol}");
                $"Thực hiện đầu tư {tradingCharts.Count} phiên: Giá đóng cửa đầu kỳ {tradingCharts[0].TradingDate:dd-MM-yyyy}: {startPrice * 1000:0,0.00} giá đóng cửa cuối kỳ {tradingCharts[^1].TradingDate:dd-MM-yyyy}: {endPrice * 1000:0,0.00} lợi nhuận {endPrice.GetPercent(startPrice):0.00}%.".WriteConsole(endPrice > startPrice ? ConsoleColor.Green : ConsoleColor.Red);
                Console.WriteLine();
                chartPrices = null;
                tradingCharts = null;
                tradingCase = null;
                ExperimentTrading.Dispose();
            }
            catch (Exception ex)
            {
                ex.ToString().WriteConsole(ConsoleColor.Red);
                _logger.LogError(ex, "");
            }
        }
    }
}