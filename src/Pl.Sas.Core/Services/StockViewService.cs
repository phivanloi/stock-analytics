using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;

namespace Pl.Sas.Core.Services
{
    public class StockViewService
    {
        private static readonly ConcurrentDictionary<string, StockView> _stockViews = new();
        private readonly IAsyncCacheService _asyncCacheService;
        private readonly ILogger<StockViewService> _logger;
        private readonly IMemoryCacheService _memoryCacheService;
        private readonly IIndustryData _industryData;
        private readonly IScheduleData _scheduleData;
        private readonly IChartPriceData _chartPriceData;
        private readonly ICompanyData _companyData;
        private readonly IAnalyticsResultData _analyticsResultData;
        private readonly IStockPriceData _stockPriceData;
        private readonly IFinancialIndicatorData _financialIndicatorData;
        private readonly ITradingResultData _tradingResultData;
        private readonly IStockData _stockData;
        private readonly IKeyValueData _keyValueData;

        public StockViewService(
            IKeyValueData keyValueData,
            IStockData stockData,
            ITradingResultData tradingResultData,
            IFinancialIndicatorData financialIndicatorData,
            IStockPriceData stockPriceData,
            IAnalyticsResultData analyticsResultData,
            ICompanyData companyData,
            IChartPriceData chartPriceData,
            IScheduleData scheduleData,
            IIndustryData industryData,
            ILogger<StockViewService> logger,
            IAsyncCacheService asyncCacheService,
            IMemoryCacheService memoryCacheService)
        {
            _stockData = stockData;
            _tradingResultData = tradingResultData;
            _financialIndicatorData = financialIndicatorData;
            _stockPriceData = stockPriceData;
            _analyticsResultData = analyticsResultData;
            _companyData = companyData;
            _chartPriceData = chartPriceData;
            _scheduleData = scheduleData;
            _industryData = industryData;
            _asyncCacheService = asyncCacheService;
            _logger = logger;
            _memoryCacheService = memoryCacheService;
            _keyValueData = keyValueData;
        }

        public virtual async Task<Dictionary<string, string>> CacheGetIndustriesAsync()
        {
            var cacheKey = $"{Constants.IndustryCachePrefix}-CGIDS";
            return await _memoryCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                var industries = await _industryData.FindAllAsync();
                return industries.OrderByDescending(q => q.AutoRank).ThenByDescending(q => q.Rank).ToDictionary(q => q.Code, q => $"{q.Name} ({q.AutoRank} - {q.Rank})");
            }, Constants.DefaultCacheTime * 60 * 24);
        }

        public virtual List<StockView> GetMarketStocksView(string? exchange = null, string? industryCode = null, string? symbol = null, string? ordinal = null, string? zone = null, List<string>? followSymbols = null)
        {
            var query = _stockViews.Select(q => q.Value).AsQueryable();
            if (!string.IsNullOrEmpty(exchange))
            {
                query = query.Where(q => q.Exchange == exchange);
            }

            if (!string.IsNullOrEmpty(industryCode))
            {
                query = query.Where(q => q.IndustryCode == industryCode);
            }

            if (!string.IsNullOrEmpty(symbol))
            {
                query = query.Where(q => q.Symbol == symbol.ToUpper());
            }

            if (!string.IsNullOrEmpty(zone))
            {
                switch (zone)
                {
                    case "me":
                        if (followSymbols?.Count > 0)
                        {
                            query = query.Where(q => followSymbols.Contains(q.Symbol));
                        }
                        break;

                    case "suggestion":
                        var industries = CacheGetIndustriesAsync().Result;
                        query = query.Where(q =>
                        q.TotalScore > 15
                        && q.IndustryCode == industries.FirstOrDefault().Key);
                        break;
                }
            }

            if (!string.IsNullOrEmpty(ordinal))
            {
                return ordinal switch
                {
                    "idt" => query.OrderByDescending(q => q.IndustryRank).ThenByDescending(q => q.TotalScore).ToList(),
                    "ddgdn" => query.OrderByDescending(q => q.TotalScore).ToList(),
                    "mcdesc" => query.OrderByDescending(q => q.MarketCap).ToList(),
                    _ => query.OrderByDescending(q => q.KlhtValue).ToList(),
                };
            }
            return query.OrderByDescending(q => q.KlhtValue).ToList();
        }

        public virtual void UpdateChangeStockView(QueueMessage queueMessage)
        {
            var symbol = queueMessage.KeyValues["Symbol"];
            var stockView = JsonSerializer.Deserialize<StockView>(queueMessage.KeyValues["Data"]);
            if (string.IsNullOrEmpty(symbol) || stockView is null)
            {
                return;
            }
            _stockViews.AddOrUpdate(symbol, stockView, (key, oldValue) => stockView);
        }

        public virtual async Task<QueueMessage?> HandleStockViewEventAsync(QueueMessage queueMessage)
        {
            var schedule = await _scheduleData.GetByIdAsync(queueMessage.Id);
            if (schedule != null)
            {
                Stopwatch stopWatch = new();
                stopWatch.Start();

                switch (schedule.Type)
                {
                    case 300:
                        return await BindingStocksViewAndSetCacheAsync(schedule.DataKey ?? throw new Exception("Can't build view for null data key."));

                    default:
                        _logger.LogWarning("Process scheduler id {Id}, type: {Type} don't match any function", queueMessage.Id, schedule.Type);
                        break;
                }
                stopWatch.Stop();
                _logger.LogDebug("Process schedule {Id} type {Type} in {ElapsedMilliseconds} miniseconds.", schedule.Id, schedule.Type, stopWatch.ElapsedMilliseconds);
            }
            return null;
        }

        public virtual async Task<QueueMessage?> BindingStocksViewAndSetCacheAsync(string symbol)
        {
            Guard.Against.NullOrEmpty(symbol, nameof(symbol));
            var company = await _companyData.GetByCodeAsync(symbol);
            if (company is null)
            {
                _logger.LogWarning("Can't build view data for stock code {symbol} by company info is null.", symbol);
                return null;
            }
            var chartPrices = await _chartPriceData.CacheFindAllAsync(symbol, "D");
            var stockPrices = await _stockPriceData.GetForStockViewAsync(symbol, 10);
            var analyticsResults = await _analyticsResultData.FindAsync(symbol);
            var financialIndicators = await _financialIndicatorData.GetTopAsync(symbol, 10);
            var industry = await _industryData.GetByCodeAsync(company.SubsectorCode);
            var bankInterestRate12 = await _keyValueData.CacheGetAsync(Constants.BankInterestRate12Key);
            var stockView = new StockView()
            {
                Symbol = symbol,
                IndustryCode = company.SubsectorCode,
                Description = $"{company.Exchange} - {company.CompanyName} - {company.Supersector} - {company.Sector}",
                Exchange = company.Exchange.ToUpper().Trim(),
                Beta = company.Beta.ShowPercent(),
                MarketCap = company.MarketCap,
                IndustryRank = MarketAnalyticsService.IndustryTrend(new(), industry),
            };

            if (analyticsResults is not null)
            {
                stockView.MacroeconomicsScore = analyticsResults.MarketScore;
                stockView.CompanyValueScore = analyticsResults.CompanyValueScore;
                stockView.CompanyGrowthScore = analyticsResults.CompanyGrowthScore;
                stockView.StockScore = analyticsResults.StockScore;
                stockView.FiinScore = analyticsResults.MarketScore;
                stockView.VndScore = analyticsResults.VndScore;
                stockView.TargetPrice = analyticsResults.TargetPrice;
            }

            if (financialIndicators is not null && financialIndicators.Count > 0)//Chỉ số tài chính
            {
                stockView.Eps = financialIndicators[0]?.Eps ?? company.Eps;
                stockView.Pe = financialIndicators[0]?.Pe ?? company.Pe;
                stockView.Pb = financialIndicators[0]?.Pb ?? company.Pb;
                stockView.Roe = (financialIndicators[0]?.Roe ?? company.Roe) * 100;
                stockView.Roa = (financialIndicators[0]?.Roa ?? company.Roa) * 100;

                if (financialIndicators.Count > 2)
                {
                    var topTwoYear = financialIndicators.OrderByDescending(q => q.YearReport).Where(q => q.LengthReport == 5).Take(2).ToList();
                    if (topTwoYear is not null && topTwoYear.Count > 1)
                    {
                        stockView.YearlyRevenueGrowthPercent = topTwoYear[0].Revenue.GetPercent(topTwoYear[1].Revenue);
                        stockView.YearlyRevenueGrowthPercent = topTwoYear[0].Profit.GetPercent(topTwoYear[1].Profit);
                    }
                }
                financialIndicators = null;
            }

            if ((chartPrices is null || chartPrices.Count <= 0) && stockPrices is not null && stockPrices.Count > 0)
            {
                chartPrices = stockPrices.Select(q => q.ToChartPrice()).ToList();
            }
            BindingPercentConvulsionToView(ref stockView, chartPrices);
            chartPrices = null;

            if (financialIndicators is not null && financialIndicators.Count > 2)
            {
                var topTwoYear = financialIndicators.OrderByDescending(q => q.YearReport).Where(q => q.LengthReport == 5).Take(2).ToList();
                if (topTwoYear is not null && topTwoYear.Count > 1)
                {
                    stockView.YearlyRevenueGrowthPercent = topTwoYear[0].Revenue.GetPercent(topTwoYear[1].Revenue);
                    stockView.YearlyRevenueGrowthPercent = topTwoYear[0].Profit.GetPercent(topTwoYear[1].Profit);
                }
                financialIndicators = null;
            }

            var tradingResults = await _tradingResultData.GetForViewAsync(symbol);
            BindingTradingResultToView(ref stockView, tradingResults, bankInterestRate12?.GetValue<float>() ?? 6.8f);
            tradingResults = null;

            industry = null;
            company = null;
            var cacheKey = $"{Constants.StockViewCachePrefix}-SM-{symbol}";
            var setCacheTask = _asyncCacheService.SetValueAsync(cacheKey, stockView, Constants.DefaultCacheTime * 60 * 24 * 30);
            var sendMessage = new QueueMessage("UpdateStockView");
            sendMessage.KeyValues.Add("Data", JsonSerializer.Serialize(stockView));
            sendMessage.KeyValues.Add("Symbol", symbol);
            await setCacheTask;
            return sendMessage;
        }

        public virtual async Task InitialAsync()
        {
            var stocks = await _stockData.FindAllAsync("s");
            foreach (var stock in stocks)
            {
                var cacheKey = $"{Constants.StockViewCachePrefix}-SM-{stock.Symbol}";
                var stockView = await _asyncCacheService.GetByKeyAsync<StockView>(cacheKey);
                if (stockView != null)
                {
                    _stockViews.AddOrUpdate(stock.Symbol, stockView, (key, oldValue) => stockView);
                }
            }
            stocks = null;
        }

        public static void BindingPercentConvulsionToView(ref StockView stockView, List<ChartPrice>? chartPrices)
        {
            if (chartPrices is not null && chartPrices.Count > 0)
            {
                var checkChartPrices = chartPrices.OrderByDescending(q => q.TradingDate).ToList();
                var lastPercent = checkChartPrices[0].ClosePrice.GetPercent(checkChartPrices[^1].ClosePrice);
                stockView.Ght = checkChartPrices[0].ClosePrice.ShowPrice(1);
                stockView.GhtCss = "ght t-r " + lastPercent.GetTextColorCss();

                stockView.KlhtValue = checkChartPrices[0].TotalMatchVol;
                stockView.Klht = checkChartPrices[0].TotalMatchVol.ShowMoney(1);
                stockView.KlhtCss = "klht t-r ";

                if (checkChartPrices.Count > 2)
                {
                    var avg30MatchVol = chartPrices.Where(q => q.TradingDate < checkChartPrices[0].TradingDate)
                        .OrderByDescending(q => q.TradingDate)
                        .Take(30)
                        .Average(q => q.TotalMatchVol);

                    stockView.KlhtCss += checkChartPrices[0].TotalMatchVol.GetTextColorCss(avg30MatchVol);
                }

                var currentPercent = 0f;
                if (checkChartPrices.Count >= 2)
                {
                    currentPercent = checkChartPrices[0].ClosePrice.GetPercent(checkChartPrices[1].ClosePrice);
                    stockView.Bd2Value = currentPercent;
                    stockView.Bd2 = currentPercent.ShowPercent();
                    stockView.Bd2Css = "bd2 t-r " + currentPercent.GetTextColorCss();
                }
                else
                {
                    stockView.Bd2Value = lastPercent;
                    stockView.Bd2 = lastPercent.ShowPercent();
                    stockView.Bd2Css = "bd2 t-r " + lastPercent.GetTextColorCss();
                }

                if (checkChartPrices.Count >= 5)
                {
                    currentPercent = checkChartPrices[0].ClosePrice.GetPercent(checkChartPrices[4].ClosePrice);
                    stockView.Bd5 = currentPercent.ShowPercent();
                    stockView.Bd5Css = "bd5 t-r " + currentPercent.GetTextColorCss();
                }
                else
                {
                    stockView.Bd5 = lastPercent.ShowPercent();
                    stockView.Bd5Css = "bd5 t-r " + lastPercent.GetTextColorCss();
                }

                if (checkChartPrices.Count >= 10)
                {
                    currentPercent = checkChartPrices[0].ClosePrice.GetPercent(checkChartPrices[9].ClosePrice);
                    stockView.Bd10 = currentPercent.ShowPercent();
                    stockView.Bd10Css = "bd10 t-r " + currentPercent.GetTextColorCss();
                }
                else
                {
                    stockView.Bd10 = lastPercent.ShowPercent();
                    stockView.Bd10Css = "bd10 t-r " + lastPercent.GetTextColorCss();
                }

                if (checkChartPrices.Count >= 30)
                {
                    currentPercent = checkChartPrices[0].ClosePrice.GetPercent(checkChartPrices[29].ClosePrice);
                    stockView.Bd30 = currentPercent.ShowPercent();
                    stockView.Bd30Css = "bd30 t-r " + currentPercent.GetTextColorCss();
                }
                else
                {
                    stockView.Bd30 = lastPercent.ShowPercent();
                    stockView.Bd30Css = "bd30 t-r " + lastPercent.GetTextColorCss();
                }

                if (checkChartPrices.Count >= 60)
                {
                    currentPercent = checkChartPrices[0].ClosePrice.GetPercent(checkChartPrices[1].ClosePrice);
                    stockView.Bd60 = currentPercent.ShowPercent();
                    stockView.Bd60Css = "bd60 t-r " + currentPercent.GetTextColorCss();
                }
                else
                {
                    stockView.Bd60 = lastPercent.ShowPercent();
                    stockView.Bd60Css = "bd60 t-r " + lastPercent.GetTextColorCss();
                }
            }
        }

        public static void BindingTradingResultToView(ref StockView stockView, List<TradingResult>? tradingResults, float bankInterestRate12)
        {
            if (tradingResults is not null && tradingResults.Count > 0)
            {
                foreach (var result in tradingResults)
                {
                    if (result.Principle == 0)
                    {
                        stockView.Lntn = result.ProfitPercent.ShowPercent();
                        stockView.LntnCss = $"lntn t-r {result.ProfitPercent.GetTextColorCss(bankInterestRate12)}";
                        stockView.Kntn = result.AssetPosition;
                        stockView.KntnCss = "kntn t-c";
                        if (result.IsBuy || result.IsSell)
                        {
                            stockView.KntnCss += " t-i";
                        }
                    }
                    else if (result.Principle == 1)
                    {
                        stockView.Lnc = result.ProfitPercent.ShowPercent();
                        stockView.LncCss = $"lnc t-r {result.ProfitPercent.GetTextColorCss(bankInterestRate12)}";
                        stockView.Knc = result.AssetPosition;
                        stockView.KncCss = "knc t-c";
                        if (result.IsBuy || result.IsSell)
                        {
                            stockView.KntnCss += " t-i";
                        }
                    }
                    else if (result.Principle == 3)
                    {
                        stockView.Lnmg = result.ProfitPercent.ShowPercent();
                        stockView.LnmgCss = $"lnmg t-r {result.ProfitPercent.GetTextColorCss(bankInterestRate12)}";
                    }
                }
            }
        }
    }
}