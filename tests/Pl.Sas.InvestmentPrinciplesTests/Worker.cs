using Pl.Sas.Core;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Core.Services;
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
        private readonly AnalyticsService _analyticsService;
        public Worker(
            AnalyticsService analyticsService,
            ICompanyData companyData,
            IStockData stockData,
            IChartPriceData chartPriceData,
            ILogger<Worker> logger)
        {
            _logger = logger;
            _chartPriceData = chartPriceData;
            _stockData = stockData;
            _companyData = companyData;
            _analyticsService = analyticsService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Clear();
            try
            {
                //await FindThebestAsync("HPG");
                await TestTradingAsync("VIX");
            }
            catch (Exception ex)
            {
                ex.ToString().WriteConsole(ConsoleColor.Red);
                _logger.LogError(ex, "");
            }
        }

        public async Task TestTradingAsync(string symbol)
        {
            Console.Clear();
            Console.OutputEncoding = Encoding.UTF8;
            DateTime fromDate = new(2010, 1, 1);
            DateTime toDate = new(2023, 1, 1);
            var stock = await _stockData.FindBySymbolAsync(symbol);
            var company = await _companyData.FindBySymbolAsync(symbol);
            var chartPrices = await _chartPriceData.CacheFindAllAsync(symbol, "D") ?? throw new Exception("ChartPrices is null");
            var indexChartPrices = await _chartPriceData.CacheFindAllAsync("VNINDEX", "D") ?? throw new Exception("indexChartPrices is null");
            chartPrices = chartPrices.OrderBy(q => q.TradingDate).ToList();
            var chartTrading = chartPrices.Where(q => q.TradingDate >= fromDate && q.TradingDate < toDate).OrderBy(q => q.TradingDate).ToList();
            var tradingHistory = chartPrices.Where(q => q.TradingDate < fromDate).OrderBy(q => q.TradingDate).ToList();
            var startPrice = chartTrading[0].ClosePrice;
            var endPrice = chartTrading[^1].ClosePrice;
            Console.Clear();
            Console.Clear();
            var trader = new SmaTrading(chartPrices, 6, 23);
            var tradingCase = trader.Trading(chartTrading, tradingHistory, stock.Exchange);
            foreach (var note in tradingCase.ExplainNotes)
            {
                note.Value.WriteConsole(note.Key > 0 ? ConsoleColor.Green : note.Key < 0 ? ConsoleColor.Red : ConsoleColor.White);
            }
            Console.WriteLine();
            Console.WriteLine($"kết quả {trader.GetType().Name} {symbol} trong {chartTrading.Count} phiên có {tradingCase.NumberDayInStock} phiên giữ cổ phiếu, {tradingCase.NumberDayInMoney} phiên giữ tiền");
            $"Số lần mua, bán thắng/thua {tradingCase.WinNumber}/{tradingCase.LoseNumber}. Số lần khớp giá tính toán/giá đóng cửa: {tradingCase.NumberPriceNeed}/{tradingCase.NumberPriceClose}".WriteConsole(tradingCase.FixedCapital <= tradingCase.Profit(chartTrading[^1].ClosePrice) ? ConsoleColor.Green : ConsoleColor.Red);
            $"Lợi nhuận {tradingCase.Profit(chartTrading[^1].ClosePrice):0,0} ({tradingCase.ProfitPercent(chartTrading[^1].ClosePrice):0,0.00}%), thuế {tradingCase.TotalTax:0,0}".WriteConsole(tradingCase.FixedCapital <= tradingCase.Profit(chartTrading[^1].ClosePrice) ? ConsoleColor.Green : ConsoleColor.Red);
            $"Trạng thái hôm nay: {tradingCase.AssetPosition}, mua/bán {tradingCase.IsBuy}/{tradingCase.IsSell}".WriteConsole();
            Console.WriteLine();
            Console.WriteLine($"Với chỉ mua và nắm giữ: {symbol}");
            $"Thực hiện mua và giữ {symbol} trong {chartTrading.Count} phiên: Giá đóng cửa đầu kỳ {chartTrading[0].TradingDate:dd-MM-yyyy}: {startPrice * 1000:0,0.00} giá đóng cửa cuối kỳ {chartTrading[^1].TradingDate:dd-MM-yyyy}: {endPrice * 1000:0,0.00} lợi nhuận {endPrice.GetPercent(startPrice):0.00}%.".WriteConsole(endPrice > startPrice ? ConsoleColor.Green : ConsoleColor.Red);
            $"Trạng thái hôm nay: 100% C".WriteConsole();
            Console.WriteLine();
            var year = toDate.Year - fromDate.Year;
            var bankProfit = BaseTrading.BankProfit(tradingCase.FixedCapital, year, 6.8f);
            $"Gửi ngân hàng: {tradingCase.FixedCapital:0,0.00} trong {year} năm lãi suất 6.8 kết quả: {bankProfit:0,0.00}({bankProfit.GetPercent(tradingCase.FixedCapital):0.00}%)".WriteConsole(ConsoleColor.Green);
        }

        public async Task FindThebestAsync(string symbol)
        {
            var chartPrices = await _chartPriceData.CacheFindAllAsync(symbol, "D") ?? throw new Exception("ChartPrices is null");
            var (Case, Feature) = await _analyticsService.FindStockFeatureAsync(symbol);
            if (Case is null || Feature is null)
            {
                return;
            }
            foreach (var note in Case.ExplainNotes)
            {
                note.Value.WriteConsole(note.Key > 0 ? ConsoleColor.Green : note.Key < 0 ? ConsoleColor.Red : ConsoleColor.White);
            }
            Console.WriteLine();
            $"Tham số tối ưu: FastSma: {Feature.FastSma}, SlowSma: {Feature.SlowSma}, sô trường hợp thắng/thua: {Feature.SmaWin}/{Feature.SmaLose}".WriteConsole();
            $"Số lần mua, bán thắng/thua {Case.WinNumber}/{Case.LoseNumber}. Số lần khớp giá tính toán/giá đóng cửa: {Case.NumberPriceNeed}/{Case.NumberPriceClose}".WriteConsole(Case.FixedCapital <= Case.Profit(chartPrices[0].ClosePrice) ? ConsoleColor.Green : ConsoleColor.Red);
            $"Lợi nhuận {Case.Profit(chartPrices[0].ClosePrice):0,0} ({Case.ProfitPercent(chartPrices[0].ClosePrice):0,0.00}%), thuế {Case.TotalTax:0,0}".WriteConsole(Case.FixedCapital <= Case.Profit(chartPrices[0].ClosePrice) ? ConsoleColor.Green : ConsoleColor.Red);
            Console.WriteLine();
        }
    }
}