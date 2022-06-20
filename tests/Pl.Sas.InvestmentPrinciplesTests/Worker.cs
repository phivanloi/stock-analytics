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
        private readonly IStockData _stockData;
        private readonly ICompanyData _companyData;
        public Worker(
            ICompanyData companyData,
            IStockData stockData,
            IChartPriceData chartPriceData,
            ILogger<Worker> logger)
        {
            _logger = logger;
            _chartPriceData = chartPriceData;
            _stockData = stockData;
            _companyData = companyData;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                Console.Clear();
                Console.OutputEncoding = Encoding.UTF8;
                DateTime fromDate = new(2010, 1, 1);
                DateTime toDate = new(2050, 1, 1);
                var symbol = "VND";
                var stock = await _stockData.FindBySymbolAsync(symbol);
                var company = await _companyData.FindBySymbolAsync(symbol);
                var chartPrices = await _chartPriceData.CacheFindAllAsync(symbol, "D") ?? throw new Exception("chartPrices is null");
                var indexChartPrices = await _chartPriceData.CacheFindAllAsync("VNINDEX", "D") ?? throw new Exception("indexChartPrices is null");
                chartPrices = chartPrices.OrderBy(q => q.TradingDate).ToList();
                var chartTrading = chartPrices.Where(q => q.TradingDate >= fromDate && q.TradingDate < toDate).OrderBy(q => q.TradingDate).ToList();
                var tradingHistory = chartPrices.Where(q => q.TradingDate < fromDate).OrderBy(q => q.TradingDate).ToList();
                var startPrice = chartTrading[0].ClosePrice;
                var endPrice = chartTrading[^1].ClosePrice;
                var trader = new ExperimentTradingV2(chartPrices, indexChartPrices);
                var tradingCase = trader.Trading(chartTrading, tradingHistory, stock.Exchange);
                Console.Clear();
                foreach (var note in tradingCase.ExplainNotes)
                {
                    note.Value.WriteConsole(note.Key > 0 ? ConsoleColor.Green : note.Key < 0 ? ConsoleColor.Red : ConsoleColor.White);
                }
                Console.WriteLine();
                Console.WriteLine($"kết quả {nameof(ExperimentTradingV2)} {symbol} trong {chartTrading.Count} phiên có {tradingCase.NumberDayInStock} phiên giữ cổ phiếu, {tradingCase.NumberDayInMoney} phiên giữ tiền");
                $"Số lần mua, bán thắng/thua {tradingCase.WinNumber}/{tradingCase.LoseNumber}. Số lần khớp giá tính toán/giá đóng cửa: {tradingCase.NumberPriceNeed}/{tradingCase.NumberPriceClose}".WriteConsole(tradingCase.FixedCapital <= tradingCase.Profit(chartTrading[^1].ClosePrice) ? ConsoleColor.Green : ConsoleColor.Red);
                $"Lợi nhuận {tradingCase.Profit(chartTrading[^1].ClosePrice):0,0} ({tradingCase.ProfitPercent(chartTrading[^1].ClosePrice):0,0.00}%), thuế {tradingCase.TotalTax:0,0}".WriteConsole(tradingCase.FixedCapital <= tradingCase.Profit(chartTrading[^1].ClosePrice) ? ConsoleColor.Green : ConsoleColor.Red);
                $"Trạng thái hôm nay: {tradingCase.AssetPosition}, mua/bán {tradingCase.IsBuy}/{tradingCase.IsSell}".WriteConsole();
                Console.WriteLine();
                Console.WriteLine($"Với chỉ mua và nắm giữ: {symbol}");
                $"Thực hiện mua và giữ {symbol} trong {chartTrading.Count} phiên: Giá đóng cửa đầu kỳ {chartTrading[0].TradingDate:dd-MM-yyyy}: {startPrice * 1000:0,0.00} giá đóng cửa cuối kỳ {chartTrading[^1].TradingDate:dd-MM-yyyy}: {endPrice * 1000:0,0.00} lợi nhuận {endPrice.GetPercent(startPrice):0.00}%.".WriteConsole(endPrice > startPrice ? ConsoleColor.Green : ConsoleColor.Red);
                $"Trạng thái hôm nay: 100% C".WriteConsole();
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