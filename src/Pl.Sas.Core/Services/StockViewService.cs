using Microsoft.Extensions.Logging;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Core.Trading;
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

        public StockViewService(
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

        public virtual void UpdateChangeStockView(QueueMessage queueMessage)
        {
            var symbol = queueMessage.KeyValues["Symbol"];
            var stockView = JsonSerializer.Deserialize<StockView>(queueMessage.KeyValues["Data"]);
            if (string.IsNullOrEmpty(symbol) || stockView is null)
            {
                return;
            }
            _stockViews.AddOrUpdate(symbol, stockView, (key, oldValue) =>
            {
                return stockView;
            });
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
            var chartPrices = await _chartPriceData.FindAllAsync(symbol, "D");
            if (chartPrices.Count < 5)
            {
                _logger.LogWarning("Can't build view data for stock code {symbol} by stock price is lower five item.", symbol);
                return null;
            }
            var company = await _companyData.GetByCodeAsync(symbol);
            if (company is null)
            {
                _logger.LogWarning("Can't build view data for stock code {symbol} by company info is null.", symbol);
                return null;
            }
            var analyticsResults = await _analyticsResultData.FindAsync(symbol);
            if (analyticsResults is null)
            {
                _logger.LogWarning("Can't build view data for stock code {symbol} by analytics result lower one item.", symbol);
                return null;
            }
            var stockPrices = await _stockPriceData.GetForStockViewAsync(symbol, 10);

            var financialIndicators = await _financialIndicatorData.FindAllAsync(symbol);
            var lastFinancialIndicator = financialIndicators.OrderByDescending(q => q.YearReport).ThenByDescending(q => q.LengthReport).FirstOrDefault(q => q.LengthReport == 5);
            var topThreeFinancialIndicator = financialIndicators.OrderByDescending(q => q.YearReport).ThenByDescending(q => q.LengthReport).Where(q => q.LengthReport == 5).Take(4).ToList();
            var industry = await _industryData.GetByCodeAsync(company.SubsectorCode);
            var topThreeHistories = stockPrices.Take(3).ToList();
            var topFiveHistories = stockPrices.Take(5).ToList();
            var stockView = new StockView()
            {
                Symbol = symbol,
                IndustryCode = company.SubsectorCode,
                Description = $"{company.Exchange} - {company.CompanyName} - {company.Supersector} - {company.Sector}",
                Exchange = company.Exchange ?? "",

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
                LastForeignBuyVolTotal = stockPrices[0].ForeignBuyVolTotal,
                LastForeignSellVolTotal = stockPrices[0].ForeignSellVolTotal,
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
            stockView.LastAvgTenTotalMatchVol = stockPrices?.Average(q => q.TotalMatchVol) ?? stockView.LastAvgFiveTotalMatchVol;
            stockView.LastAvgThreeForeignBuyVolTotal = topThreeHistories?.Average(q => q.ForeignBuyVolTotal) ?? stockView.LastForeignBuyVolTotal;
            stockView.LastAvgFiveForeignBuyVolTotal = topFiveHistories?.Average(q => q.ForeignBuyVolTotal) ?? stockView.LastAvgThreeForeignBuyVolTotal;
            stockView.LastAvgTenForeignBuyVolTotal = stockPrices?.Average(q => q.ForeignBuyVolTotal) ?? stockView.LastAvgFiveForeignBuyVolTotal;
            stockView.LastAvgThreeForeignSellVolTotal = topThreeHistories?.Average(q => q.ForeignSellVolTotal) ?? stockView.LastForeignSellVolTotal;
            stockView.LastAvgFiveForeignSellVolTotal = topFiveHistories?.Average(q => q.ForeignSellVolTotal) ?? stockView.LastAvgThreeForeignSellVolTotal;
            stockView.LastAvgTenForeignSellVolTotal = stockPrices?.Average(q => q.ForeignSellVolTotal) ?? stockView.LastAvgFiveForeignSellVolTotal;

            #region Count change
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

            #endregion

            #region Convulsion
            if (chartPrices.Count >= 1)
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
            if (chartPrices.Count >= 90)
            {
                stockView.PricePercentConvulsion90 = chartPrices[0].ClosePrice.GetPercent(chartPrices[89].ClosePrice);
            }
            else
            {
                stockView.PricePercentConvulsion90 = chartPrices[0].ClosePrice.GetPercent(chartPrices.Last().ClosePrice);
            }
            if (chartPrices.Count >= 120)
            {
                stockView.PricePercentConvulsion120 = chartPrices[0].ClosePrice.GetPercent(chartPrices[119].ClosePrice);
            }
            else
            {
                stockView.PricePercentConvulsion120 = chartPrices[0].ClosePrice.GetPercent(chartPrices.Last().ClosePrice);
            }
            if (chartPrices.Count >= 240)
            {
                stockView.PricePercentConvulsion240 = chartPrices[0].ClosePrice.GetPercent(chartPrices[239].ClosePrice);
            }
            else
            {
                stockView.PricePercentConvulsion240 = chartPrices[0].ClosePrice.GetPercent(chartPrices.Last().ClosePrice);
            }
            if (chartPrices.Count >= 480)
            {
                stockView.PricePercentConvulsion480 = chartPrices[0].ClosePrice.GetPercent(chartPrices[479].ClosePrice);
            }
            else
            {
                stockView.PricePercentConvulsion480 = chartPrices[0].ClosePrice.GetPercent(chartPrices.Last().ClosePrice);
            }
            if (chartPrices.Count >= 960)
            {
                stockView.PricePercentConvulsion960 = chartPrices[0].ClosePrice.GetPercent(chartPrices[959].ClosePrice);
            }
            else
            {
                stockView.PricePercentConvulsion960 = chartPrices[0].ClosePrice.GetPercent(chartPrices.Last().ClosePrice);
            }
            if (chartPrices.Count >= 1920)
            {
                stockView.PricePercentConvulsion1920 = chartPrices[0].ClosePrice.GetPercent(chartPrices[1919].ClosePrice);
            }
            else
            {
                stockView.PricePercentConvulsion1920 = chartPrices[0].ClosePrice.GetPercent(chartPrices.Last().ClosePrice);
            }
            if (chartPrices.Count >= 3840)
            {
                stockView.PricePercentConvulsion3840 = chartPrices[0].ClosePrice.GetPercent(chartPrices[3839].ClosePrice);
            }
            else
            {
                stockView.PricePercentConvulsion3840 = chartPrices[0].ClosePrice.GetPercent(chartPrices.Last().ClosePrice);
            }

            var startTradingItem = chartPrices.LastOrDefault(q => q.TradingDate >= Constants.StartTime);
            if (startTradingItem is not null)
            {
                stockView.PricePercentConvulsionStartTrading = chartPrices[0].ClosePrice.GetPercent(startTradingItem.ClosePrice);
            }
            else
            {
                stockView.PricePercentConvulsionStartTrading = stockView.PricePercentConvulsion960;
            }

            #endregion

            #region Trading info

            var tradingResults = await _tradingResultData.FindAllAsync(symbol);
            var indicatorSet = BaseTrading.BuildIndicatorSet(chartPrices);
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
                    if (indicatorSet.ContainsKey(chartPrices[0].DatePath))
                    {
                        var todaySet = indicatorSet[chartPrices[0].DatePath];
                    }
                    stockView.TradingViews.Add(principle, judgeResult);
                }
            }

            #endregion

            var cacheKey = $"{Constants.StockViewCachePrefix}-SM-{symbol}";
            await _asyncCacheService.SetValueAsync(cacheKey, stockView, Constants.DefaultCacheTime * 60 * 24);
            var sendMessage = new QueueMessage("UpdateStockView");
            sendMessage.KeyValues.Add("Data", JsonSerializer.Serialize(stockView));
            sendMessage.KeyValues.Add("Symbol", symbol);
            return sendMessage;
        }
    }
}