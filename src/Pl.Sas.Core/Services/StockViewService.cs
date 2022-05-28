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

        public StockViewService(
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

        public virtual void UpdateChangeStockView(QueueMessage queueMessage)
        {
            var symbol = queueMessage.KeyValues["Symbol"];
            var stockView = JsonSerializer.Deserialize<StockView>(queueMessage.KeyValues["Data"]);
            if (string.IsNullOrEmpty(symbol) || stockView is null)
            {
                return;
            }
            _stockViews.AddOrUpdate(symbol, stockView, (key, oldValue) => stockView);
            stockView = null;
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
            var stockView = new StockView()
            {
                Symbol = symbol,
                IndustryCode = company.SubsectorCode,
                Description = $"{company.Exchange} - {company.CompanyName} - {company.Supersector} - {company.Sector}",
                Exchange = company.Exchange.ToUpper().Trim(),
                Beta = company.Beta,
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

            if (stockPrices is not null && stockPrices.Count > 1)
            {
                var topThreeHistories = stockPrices.Take(3).ToList();
                var topFiveHistories = stockPrices.Take(5).ToList();

                stockView.LastClosePrice = stockPrices[0].ClosePrice;
                stockView.LastOneClosePrice = stockPrices[1].ClosePrice;
                stockView.LastTotalMatchVol = stockPrices[0].TotalMatchVol;
                stockView.LastOneTotalMatchVol = stockPrices[1].TotalMatchVol;
                stockView.LastForeignBuyVolTotal = stockPrices[0].ForeignBuyVolTotal;
                stockView.LastForeignSellVolTotal = stockPrices[0].ForeignSellVolTotal;
                stockView.LastOpenPrice = stockPrices[0].OpenPrice;
                stockView.LastHighestPrice = stockPrices[0].HighestPrice;
                stockView.LastLowestPrice = stockPrices[0].LowestPrice;

                if (topThreeHistories.Count > 2)
                {
                    stockView.LastHistoryMinLowestPrice = topFiveHistories.Min(q => q.LowestPrice);
                    stockView.LastHistoryMinHighestPrice = topFiveHistories.Max(q => q.HighestPrice);
                    stockView.LastAvgThreeTotalMatchVol = topThreeHistories?.Average(q => q.TotalMatchVol) ?? stockView.LastOneTotalMatchVol;
                    stockView.LastAvgFiveTotalMatchVol = topFiveHistories?.Average(q => q.TotalMatchVol) ?? stockView.LastAvgThreeTotalMatchVol;
                    stockView.LastAvgTenTotalMatchVol = stockPrices?.Average(q => q.TotalMatchVol) ?? stockView.LastAvgFiveTotalMatchVol;
                    stockView.LastAvgThreeForeignBuyVolTotal = topThreeHistories?.Average(q => q.ForeignBuyVolTotal) ?? stockView.LastForeignBuyVolTotal;
                    stockView.LastAvgFiveForeignBuyVolTotal = topFiveHistories?.Average(q => q.ForeignBuyVolTotal) ?? stockView.LastAvgThreeForeignBuyVolTotal;
                    stockView.LastAvgTenForeignBuyVolTotal = stockPrices?.Average(q => q.ForeignBuyVolTotal) ?? stockView.LastAvgFiveForeignBuyVolTotal;
                    stockView.LastAvgThreeForeignSellVolTotal = topThreeHistories?.Average(q => q.ForeignSellVolTotal) ?? stockView.LastForeignSellVolTotal;
                    stockView.LastAvgFiveForeignSellVolTotal = topFiveHistories?.Average(q => q.ForeignSellVolTotal) ?? stockView.LastAvgThreeForeignSellVolTotal;
                    stockView.LastAvgTenForeignSellVolTotal = stockPrices?.Average(q => q.ForeignSellVolTotal) ?? stockView.LastAvgFiveForeignSellVolTotal;
                }

                stockPrices = null;
            }

            if (chartPrices is not null && chartPrices.Count > 0)
            {
                chartPrices = chartPrices.OrderByDescending(q => q.TradingDate).ToList();
                for (int i = 0; i < chartPrices.Count - 1; i++)
                {
                    if (chartPrices[i].ClosePrice > chartPrices[i + 1].ClosePrice)
                    {
                        stockView.NumberOfClosePriceIncreases++;
                    }
                    else
                    {
                        break;
                    }
                }
                for (int i = 0; i < chartPrices.Count - 1; i++)
                {
                    if (chartPrices[i].ClosePrice <= chartPrices[i + 1].ClosePrice)
                    {
                        stockView.NumberOfClosePriceDecrease++;
                    }
                    else
                    {
                        break;
                    }
                }
                #region Convulsion
                if (chartPrices.Count >= 2)
                {
                    stockView.PricePercentConvulsion1 = chartPrices[0].ClosePrice.GetPercent(chartPrices[1].ClosePrice);
                }
                if (chartPrices.Count >= 5)
                {
                    stockView.PricePercentConvulsion5 = chartPrices[0].ClosePrice.GetPercent(chartPrices[4].ClosePrice);
                }
                else
                {
                    stockView.PricePercentConvulsion5 = chartPrices[0].ClosePrice.GetPercent(chartPrices.Last().ClosePrice);
                }
                if (chartPrices.Count >= 10)
                {
                    stockView.PricePercentConvulsion10 = chartPrices[0].ClosePrice.GetPercent(chartPrices[9].ClosePrice);
                }
                else
                {
                    stockView.PricePercentConvulsion10 = chartPrices[0].ClosePrice.GetPercent(chartPrices.Last().ClosePrice);
                }
                if (chartPrices.Count >= 30)
                {
                    stockView.PricePercentConvulsion30 = chartPrices[0].ClosePrice.GetPercent(chartPrices[29].ClosePrice);
                }
                else
                {
                    stockView.PricePercentConvulsion30 = chartPrices[0].ClosePrice.GetPercent(chartPrices.Last().ClosePrice);
                }
                if (chartPrices.Count >= 50)
                {
                    stockView.PricePercentConvulsion50 = chartPrices[0].ClosePrice.GetPercent(chartPrices[49].ClosePrice);
                }
                else
                {
                    stockView.PricePercentConvulsion50 = chartPrices[0].ClosePrice.GetPercent(chartPrices.Last().ClosePrice);
                }
                if (chartPrices.Count >= 60)
                {
                    stockView.PricePercentConvulsion60 = chartPrices[0].ClosePrice.GetPercent(chartPrices[59].ClosePrice);
                }
                else
                {
                    stockView.PricePercentConvulsion60 = chartPrices[0].ClosePrice.GetPercent(chartPrices.Last().ClosePrice);
                }
                #endregion
                chartPrices = null;
            }

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

            #region Trading info
            var tradingResults = await _tradingResultData.FindAllAsync(symbol);
            var trading = tradingResults.FirstOrDefault(q => q.Principle == 0);
            if (trading is not null)
            {
                stockView.ExperimentTradingProfitPercent = trading.ProfitPercent
            }

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
                    stockView.TradingViews.Add(principle, judgeResult);
                }
            }
            #endregion

            industry = null;
            company = null;

            var cacheKey = $"{Constants.StockViewCachePrefix}-SM-{symbol}";
            await _asyncCacheService.SetValueAsync(cacheKey, stockView, Constants.DefaultCacheTime * 60 * 24 * 30);
            var sendMessage = new QueueMessage("UpdateStockView");
            sendMessage.KeyValues.Add("Data", JsonSerializer.Serialize(stockView));
            sendMessage.KeyValues.Add("Symbol", symbol);
            return sendMessage;
        }

        public virtual async Task InitialAsync()
        {
            var stocks = await _stockData.FindAllAsync();
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
    }
}