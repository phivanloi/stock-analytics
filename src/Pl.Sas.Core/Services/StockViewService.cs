using Microsoft.Extensions.Logging;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Core.Trading;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Pl.Sas.Core.Services
{
    public class StockViewService
    {
        private static readonly ConcurrentDictionary<string, StockView> _stockViews = new();
        private readonly IAsyncCacheService _asyncCacheService;
        private readonly ILogger<StockViewService> _logger;
        private readonly IMemoryCacheService _memoryCacheService;
        private readonly IMarketData _marketData;
        private readonly IAnalyticsData _analyticsData;
        private readonly ISystemData _systemData;

        public StockViewService(
            ISystemData systemData,
            IAnalyticsData analyticsData,
            IMarketData marketData,
            ILogger<StockViewService> logger,
            IAsyncCacheService asyncCacheService,
            IMemoryCacheService memoryCacheService)
        {
            _asyncCacheService = asyncCacheService;
            _logger = logger;
            _memoryCacheService = memoryCacheService;
            _marketData = marketData;
            _analyticsData = analyticsData;
            _systemData = systemData;
        }

        public virtual async Task<Dictionary<string, string>> CacheGetIndustriesAsync()
        {
            var cacheKey = $"{Constants.IndustryCachePrefix}-CGIDS";
            return await _memoryCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                var industries = await _marketData.GetIndustriesAsync();
                var analyticsIndustries = await _analyticsData.GetIndustryAnalyticsAsync();
                var stats = from industry in industries
                            join analyticsIndustry in analyticsIndustries on industry.Code equals analyticsIndustry.Code
                            select new { industry.Code, industry.Name, analyticsIndustry.ManualScore, analyticsIndustry.Score };
                return stats.OrderByDescending(q => q.ManualScore).ThenByDescending(q => q.Score).ToDictionary(q => q.Code, q => $"{q.Name} ({q.ManualScore} - {q.Score})");
            }, Constants.DefaultCacheTime * 60 * 24);
        }

        public virtual List<StockView> GetMarketStocksView(int principle = 1, string? exchange = null, string? industryCode = null, string? symbol = null, string? ordinal = null, string? zone = null, List<string>? followSymbols = null)
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
                        && q.IndustryCode == industries.FirstOrDefault().Key
                        && q.PricePercentConvulsion10 > 10
                        && q.LastAvgTenTotalMatchVol > 300000);
                        break;
                }
            }

            if (!string.IsNullOrEmpty(ordinal))
            {
                return ordinal switch
                {
                    "lntn" => query.OrderByDescending(q => q.TradingViews[principle].ProfitPercent).ToList(),
                    "idt" => query.OrderByDescending(q => q.IndustryRank).ThenByDescending(q => q.TotalScore).ToList(),
                    "klmkn" => query.OrderByDescending(q => q.LastForeignPurchasingPower).ToList(),
                    "ddgdn" => query.OrderByDescending(q => q.TotalScore).ToList(),
                    "cpuad" => query.OrderByDescending(q => q.LastClosePricePercent).ToList(),
                    "mcdesc" => query.OrderByDescending(q => q.MarketCap).ToList(),
                    _ => query.OrderByDescending(q => q.LastTotalMatchVol).ToList(),
                };
            }
            return query.OrderByDescending(q => q.LastTotalMatchVol).ToList();
        }

        public virtual async Task UpdateChangeStockView()
        {
            var cacheKey = $"{Constants.StockViewCachePrefix}-ALL";
            var dictData = await _asyncCacheService.GetByKeyAsync<Dictionary<string, StockView>>(cacheKey);
            if (dictData is not null)
            {
                _stockViews.Clear();
                foreach (var dic in dictData)
                {
                    _stockViews.TryAdd(dic.Key, dic.Value);
                }
            }
        }

        public virtual async Task<QueueMessage?> HandleStockViewEventAsync(QueueMessage queueMessage)
        {
            var schedule = await _systemData.GetScheduleByIdAsync(queueMessage.Id);
            if (schedule != null)
            {
                Stopwatch stopWatch = new();
                stopWatch.Start();

                switch (schedule.Type)
                {
                    case 300:
                        return await BindingStocksViewAndSetCacheAsync();

                    default:
                        _logger.LogWarning("Process scheduler id {Id}, type: {Type} don't match any function", queueMessage.Id, schedule.Type);
                        break;
                }

                stopWatch.Stop();
                _logger.LogDebug("Process schedule {Id} type {Type} in {ElapsedMilliseconds} miniseconds.", schedule.Id, schedule.Type, stopWatch.ElapsedMilliseconds);
            }
            return null;
        }

        public virtual async Task<QueueMessage> BindingStocksViewAndSetCacheAsync()
        {
            var dictStockView = new Dictionary<string, StockView>();
            var companies = await _marketData.CacheGetCompaniesAsync();
            var stocks = await _marketData.GetStockByType("i");
            foreach (var stock in stocks)
            {
                if (!dictStockView.ContainsKey(stock.Symbol))
                {
                    var stockPrices = await _marketData.GetForStockViewAsync(stock.Symbol, 3840);
                    if (stockPrices.Count < 2)
                    {
                        _logger.LogWarning("Can't build view data for stock code {Key} by stock price is lower five item.", stock.Symbol);
                        continue;
                    }
                    var company = companies.FirstOrDefault(q => q.Symbol == stock.Symbol);
                    if (company is null)
                    {
                        _logger.LogWarning("Can't build view data for stock code {Key} by company info is null.", stock.Symbol);
                        continue;
                    }
                    var analyticsResults = await _analyticsData.CacheGetAnalyticsResultAsync(stock.Symbol);
                    if (analyticsResults is null)
                    {
                        _logger.LogWarning("Can't build view data for stock code {Code} by analytics result lower one item.", stock.Symbol);
                        continue;
                    }

                    var financialIndicators = await _marketData.GetFinancialIndicatorsAsync(stock.Symbol);
                    var lastFinancialIndicator = financialIndicators.OrderByDescending(q => q.YearReport).ThenByDescending(q => q.LengthReport).FirstOrDefault(q => q.LengthReport == 5);
                    var topThreeFinancialIndicator = financialIndicators.OrderByDescending(q => q.YearReport).ThenByDescending(q => q.LengthReport).Where(q => q.LengthReport == 5).Take(4).ToList();
                    var industry = await _analyticsData.GetIndustryAnalyticsAsync(company.SubsectorCode);
                    var topThreeHistories = stockPrices.Take(3).ToList();
                    var topFiveHistories = stockPrices.Take(5).ToList();
                    var topTenHistories = stockPrices.Take(10).ToList();
                    var stockView = new StockView()
                    {
                        Symbol = stock.Symbol,
                        IndustryCode = company.SubsectorCode,
                        Description = $"{stock.Exchange} - {company.CompanyName} - {company.Supersector} - {company.Sector}",
                        Exchange = stock.Exchange,

                        MacroeconomicsScore = analyticsResults.MarketScore,
                        IndustryRank = MarketAnalyticsService.IndustryTrend(new(), industry),

                        CompanyValueScore = analyticsResults.CompanyValueScore,
                        Eps = lastFinancialIndicator?.Eps ?? company.Eps,
                        Pe = lastFinancialIndicator?.Pe ?? company.Pe,
                        Pb = lastFinancialIndicator?.Pb ?? company.Pb,
                        Roe = (lastFinancialIndicator?.Roe ?? company.Roe) * 100,
                        Roa = (lastFinancialIndicator?.Roa ?? company.Roa) * 100,
                        MarketCap = company.MarketCap,

                        CompanyGrowthScore = analyticsResults.CompanyGrowthScore,

                        StockScore = analyticsResults.StockScore,
                        Beta = company.Beta,
                        LastClosePrice = topThreeHistories[0].ClosePrice,
                        LastOneClosePrice = topThreeHistories[1].ClosePrice,
                        LastTotalMatchVol = topThreeHistories[0].TotalMatchVol,
                        LastOneTotalMatchVol = topThreeHistories[1].TotalMatchVol,
                        LastForeignBuyVolTotal = topThreeHistories[0].ForeignBuyVolTotal,
                        LastForeignSellVolTotal = topThreeHistories[0].ForeignSellVolTotal,
                        LastOpenPrice = topThreeHistories[0].OpenPrice,
                        LastHighestPrice = topThreeHistories[0].HighestPrice,
                        LastLowestPrice = topThreeHistories[0].LowestPrice,
                        LastHistoryMinLowestPrice = topFiveHistories.Min(q => q.LowestPrice),
                        LastHistoryMinHighestPrice = topFiveHistories.Max(q => q.HighestPrice),

                        FiinScore = analyticsResults.FiinScore,
                        VndScore = analyticsResults.VndScore,
                        TargetPrice = analyticsResults.TargetPrice
                    };

                    if (topThreeFinancialIndicator.Count > 3)
                    {
                        var (_, _, _, _, _, _, percents) = topThreeFinancialIndicator.GetFluctuationsTopDown(q => q.Revenue);
                        stockView.YearlyRevenueGrowthPercent = percents?.Average() ?? 0;
                    }
                    if (topThreeFinancialIndicator.Count > 3)
                    {
                        var (_, _, _, _, _, _, percents) = topThreeFinancialIndicator.GetFluctuationsTopDown(q => q.Profit);
                        stockView.YearlyProfitGrowthPercent = percents?.Average() ?? 0;
                    }

                    stockView.LastAvgThreeTotalMatchVol = topThreeHistories?.Average(q => q.TotalMatchVol) ?? stockView.LastOneTotalMatchVol;
                    stockView.LastAvgFiveTotalMatchVol = topFiveHistories?.Average(q => q.TotalMatchVol) ?? stockView.LastAvgThreeTotalMatchVol;
                    stockView.LastAvgTenTotalMatchVol = topTenHistories?.Average(q => q.TotalMatchVol) ?? stockView.LastAvgFiveTotalMatchVol;
                    stockView.LastAvgThreeForeignBuyVolTotal = topThreeHistories?.Average(q => q.ForeignBuyVolTotal) ?? stockView.LastForeignBuyVolTotal;
                    stockView.LastAvgFiveForeignBuyVolTotal = topFiveHistories?.Average(q => q.ForeignBuyVolTotal) ?? stockView.LastAvgThreeForeignBuyVolTotal;
                    stockView.LastAvgTenForeignBuyVolTotal = topTenHistories?.Average(q => q.ForeignBuyVolTotal) ?? stockView.LastAvgFiveForeignBuyVolTotal;
                    stockView.LastAvgThreeForeignSellVolTotal = topThreeHistories?.Average(q => q.ForeignSellVolTotal) ?? stockView.LastForeignSellVolTotal;
                    stockView.LastAvgFiveForeignSellVolTotal = topFiveHistories?.Average(q => q.ForeignSellVolTotal) ?? stockView.LastAvgThreeForeignSellVolTotal;
                    stockView.LastAvgTenForeignSellVolTotal = topTenHistories?.Average(q => q.ForeignSellVolTotal) ?? stockView.LastAvgFiveForeignSellVolTotal;

                    #region Count change
                    for (int i = 0; i < stockPrices.Count - 1; i++)
                    {
                        if (stockPrices[i].ClosePrice > stockPrices[i + 1].ClosePrice)
                        {
                            stockView.NumberOfClosePriceIncreases++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    for (int i = 0; i < stockPrices.Count - 1; i++)
                    {
                        if (stockPrices[i].ClosePrice <= stockPrices[i + 1].ClosePrice)
                        {
                            stockView.NumberOfClosePriceDecrease++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    #endregion

                    #region Convulsion
                    if (stockPrices.Count >= 1)
                    {
                        stockView.PricePercentConvulsion1 = stockPrices[0].ClosePriceAdjusted.GetPercent(stockPrices[1].ClosePriceAdjusted);
                    }
                    if (stockPrices.Count >= 5)
                    {
                        stockView.PricePercentConvulsion5 = stockPrices[0].ClosePriceAdjusted.GetPercent(stockPrices[4].ClosePriceAdjusted);
                    }
                    else
                    {
                        stockView.PricePercentConvulsion5 = stockPrices[0].ClosePriceAdjusted.GetPercent(stockPrices.Last().ClosePriceAdjusted);
                    }
                    if (stockPrices.Count >= 10)
                    {
                        stockView.PricePercentConvulsion10 = stockPrices[0].ClosePriceAdjusted.GetPercent(stockPrices[9].ClosePriceAdjusted);
                    }
                    else
                    {
                        stockView.PricePercentConvulsion10 = stockPrices[0].ClosePriceAdjusted.GetPercent(stockPrices.Last().ClosePriceAdjusted);
                    }
                    if (stockPrices.Count >= 30)
                    {
                        stockView.PricePercentConvulsion30 = stockPrices[0].ClosePriceAdjusted.GetPercent(stockPrices[29].ClosePriceAdjusted);
                    }
                    else
                    {
                        stockView.PricePercentConvulsion30 = stockPrices[0].ClosePriceAdjusted.GetPercent(stockPrices.Last().ClosePriceAdjusted);
                    }
                    if (stockPrices.Count >= 50)
                    {
                        stockView.PricePercentConvulsion50 = stockPrices[0].ClosePriceAdjusted.GetPercent(stockPrices[49].ClosePriceAdjusted);
                    }
                    else
                    {
                        stockView.PricePercentConvulsion50 = stockPrices[0].ClosePriceAdjusted.GetPercent(stockPrices.Last().ClosePriceAdjusted);
                    }
                    if (stockPrices.Count >= 60)
                    {
                        stockView.PricePercentConvulsion60 = stockPrices[0].ClosePriceAdjusted.GetPercent(stockPrices[59].ClosePriceAdjusted);
                    }
                    else
                    {
                        stockView.PricePercentConvulsion60 = stockPrices[0].ClosePriceAdjusted.GetPercent(stockPrices.Last().ClosePriceAdjusted);
                    }
                    if (stockPrices.Count >= 90)
                    {
                        stockView.PricePercentConvulsion90 = stockPrices[0].ClosePriceAdjusted.GetPercent(stockPrices[89].ClosePriceAdjusted);
                    }
                    else
                    {
                        stockView.PricePercentConvulsion90 = stockPrices[0].ClosePriceAdjusted.GetPercent(stockPrices.Last().ClosePriceAdjusted);
                    }
                    if (stockPrices.Count >= 120)
                    {
                        stockView.PricePercentConvulsion120 = stockPrices[0].ClosePriceAdjusted.GetPercent(stockPrices[119].ClosePriceAdjusted);
                    }
                    else
                    {
                        stockView.PricePercentConvulsion120 = stockPrices[0].ClosePriceAdjusted.GetPercent(stockPrices.Last().ClosePriceAdjusted);
                    }
                    if (stockPrices.Count >= 240)
                    {
                        stockView.PricePercentConvulsion240 = stockPrices[0].ClosePriceAdjusted.GetPercent(stockPrices[239].ClosePriceAdjusted);
                    }
                    else
                    {
                        stockView.PricePercentConvulsion240 = stockPrices[0].ClosePriceAdjusted.GetPercent(stockPrices.Last().ClosePriceAdjusted);
                    }
                    if (stockPrices.Count >= 480)
                    {
                        stockView.PricePercentConvulsion480 = stockPrices[0].ClosePriceAdjusted.GetPercent(stockPrices[479].ClosePriceAdjusted);
                    }
                    else
                    {
                        stockView.PricePercentConvulsion480 = stockPrices[0].ClosePriceAdjusted.GetPercent(stockPrices.Last().ClosePriceAdjusted);
                    }
                    if (stockPrices.Count >= 960)
                    {
                        stockView.PricePercentConvulsion960 = stockPrices[0].ClosePriceAdjusted.GetPercent(stockPrices[959].ClosePriceAdjusted);
                    }
                    else
                    {
                        stockView.PricePercentConvulsion960 = stockPrices[0].ClosePriceAdjusted.GetPercent(stockPrices.Last().ClosePriceAdjusted);
                    }
                    if (stockPrices.Count >= 1920)
                    {
                        stockView.PricePercentConvulsion1920 = stockPrices[0].ClosePriceAdjusted.GetPercent(stockPrices[1919].ClosePriceAdjusted);
                    }
                    else
                    {
                        stockView.PricePercentConvulsion1920 = stockPrices[0].ClosePriceAdjusted.GetPercent(stockPrices.Last().ClosePriceAdjusted);
                    }
                    if (stockPrices.Count >= 3840)
                    {
                        stockView.PricePercentConvulsion3840 = stockPrices[0].ClosePriceAdjusted.GetPercent(stockPrices[3839].ClosePriceAdjusted);
                    }
                    else
                    {
                        stockView.PricePercentConvulsion3840 = stockPrices[0].ClosePriceAdjusted.GetPercent(stockPrices.Last().ClosePriceAdjusted);
                    }

                    var startTradingItem = stockPrices.LastOrDefault(q => q.TradingDate >= Constants.StartTime);
                    if (startTradingItem is not null)
                    {
                        stockView.PricePercentConvulsionStartTrading = stockPrices[0].ClosePriceAdjusted.GetPercent(startTradingItem.ClosePriceAdjusted);
                    }
                    else
                    {
                        stockView.PricePercentConvulsionStartTrading = stockView.PricePercentConvulsion960;
                    }

                    #endregion

                    #region Trading info

                    var tradingResults = await _analyticsData.CacheGetTradingResultAsync(stock.Symbol);
                    var indicatorSet = BaseTrading.BuildIndicatorSet(stockPrices);
                    var principles = new int[] { 0, 1, 2, 3, 4 };

                    foreach (var principle in principles)
                    {
                        var tadingResult = tradingResults.FirstOrDefault(q => q.Principle == principle);
                        if (tadingResult is not null)
                        {
                            var judgeResult = new JudgeResult()
                            {
                                OptimalBuyPrice = tadingResult.BuyPrice,
                                OptimalSellPrice = tadingResult.SellPrice,
                                ProfitPercent = tadingResult.ProfitPercent,
                                TodayIsBuy = tadingResult.IsBuy,
                                TodayIsSell = tadingResult.IsSell
                            };
                            if (indicatorSet.ContainsKey(stockPrices[0].DatePath))
                            {
                                var todaySet = indicatorSet[stockPrices[0].DatePath];
                            }
                            stockView.TradingViews.Add(principle, judgeResult);
                        }
                    }

                    #endregion
                    dictStockView.TryAdd(stock.Symbol, stockView);
                }
            }
            var cacheKey = $"{Constants.StockViewCachePrefix}-ALL";
            await _asyncCacheService.SetValueAsync(cacheKey, dictStockView, Constants.DefaultCacheTime * 60 * 24);
            return new("UpdateStockView");
        }
    }
}