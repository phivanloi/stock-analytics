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
                DateTime fromDate = new(2018, 1, 1);
                DateTime toDate = new(2019, 10, 1);
                var symbol = "LTG";
                var chartPrices = (await _marketData.GetChartPricesAsync(symbol)).OrderBy(q => q.TradingDate).ToList();
                var tradingHistories = chartPrices.Where(q => q.TradingDate >= fromDate).OrderBy(q => q.TradingDate).ToList();
                var startPrice = tradingHistories[0].ClosePrice;
                var endPrice = tradingHistories[^1].ClosePrice;
                var tradingCase = SarTrading.Trading(tradingHistories);
                var lastChartPrice = tradingHistories[^1];
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
                Console.WriteLine();
                Console.WriteLine($"Với chỉ mua và nắm giữ -------------------------------- {symbol}");
                $"Thực hiện đầu tư {tradingHistories.Count} phiên: Giá đóng cửa đầu kỳ {tradingHistories[0].TradingDate:dd-MM-yyyy}: {startPrice * 1000:0,0.00} giá đóng cửa cuối kỳ {tradingHistories[^1].TradingDate:dd-MM-yyyy}: {endPrice * 1000:0,0.00} lợi nhuận {endPrice.GetPercent(startPrice):0.00}%.".WriteConsole(endPrice > startPrice ? ConsoleColor.Green : ConsoleColor.Red);
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                ex.ToString().WriteConsole(ConsoleColor.Red);
                _logger.LogError(ex, "");
            }
        }
    }
}