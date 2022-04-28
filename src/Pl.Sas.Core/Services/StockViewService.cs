using Microsoft.Extensions.Logging;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Core.Trading;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace Pl.Sas.Core.Services
{
    public class StockViewService : IStockViewService
    {
        private static readonly ConcurrentDictionary<string, StockView> _stockViews = new();
        private static readonly int _defaultCacheTime = 1;
        private readonly ICompanyData _companyData;
        private readonly IIndustryData _industryData;
        private readonly IStockData _stockData;
        private readonly IStockPriceData _stockPriceData;
        private readonly IAsyncCacheService _asyncCacheService;
        private readonly IAnalyticsResultData _analyticsResultData;
        private readonly ILogger<StockViewService> _logger;
        private readonly IScheduleData _scheduleData;
        private readonly ITradingResultData _tradingResultData;
        private readonly IFinancialIndicatorData _financialIndicatorData;
        private readonly IOperationRetry _operationRetry;
        private readonly IMemoryCacheService _memoryCacheService;
        private readonly IZipHelper _zipHelper;
        private readonly IFollowStockData _followStockData;
        private readonly IUserNotificationData _userNotificationData;
        private readonly ICorporateActionData _corporateActionData;

        public StockViewService(
            IZipHelper zipHelper,
            IFollowStockData followStockData,
            IUserNotificationData userNotificationData,
            ICorporateActionData corporateActionData,
            IFinancialIndicatorData financialIndicatorData,
            ITradingResultData tradingResultData,
            IScheduleData scheduleData,
            IOperationRetry operationRetry,
            ILogger<StockViewService> logger,
            IAnalyticsResultData analyticsResultData,
            IAsyncCacheService asyncCacheService,
            IStockPriceData stockPriceData,
            IStockData stockData,
            ICompanyData companyData,
            IIndustryData industryData,
            IMemoryCacheService memoryCacheService)
        {
            _corporateActionData = corporateActionData;
            _userNotificationData = userNotificationData;
            _followStockData = followStockData;
            _zipHelper = zipHelper;
            _industryData = industryData;
            _companyData = companyData;
            _stockData = stockData;
            _stockPriceData = stockPriceData;
            _asyncCacheService = asyncCacheService;
            _analyticsResultData = analyticsResultData;
            _logger = logger;
            _scheduleData = scheduleData;
            _tradingResultData = tradingResultData;
            _financialIndicatorData = financialIndicatorData;
            _operationRetry = operationRetry;
            _memoryCacheService = memoryCacheService;
        }

        public virtual async Task<List<string>> CacheGetExchangesAsync()
        {
            var cacheKey = CacheKeyService.GetStockExchangeCacheKey();
            return await _memoryCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return (await _stockData.GetExchanges()).ToList();
            }, _defaultCacheTime);
        }

        public virtual async Task<Dictionary<string, string>> CacheGetIndustriesSelectAsync()
        {
            var cacheKey = CacheKeyService.GetIndustriesCacheKey();
            return await _memoryCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                var industries = await _industryData.FindAllAsync();
                return industries.OrderByDescending(q => q.AutoRank).ThenByDescending(q => q.Rank).ToDictionary(q => q.Code, q => $"{q.Name} ({q.AutoRank} - {q.Rank})");
            }, _defaultCacheTime);
        }

        public virtual List<StockView> GetMarketStocksView(int principle = 1, string exchange = null, string industryCode = null, string symbol = null, string ordinal = null, string zone = null, List<string> followSymbols = null)
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
                query = query.Where(q => q.Code == symbol.ToUpper());
            }

            if (!string.IsNullOrEmpty(zone))
            {
                switch (zone)
                {
                    case "me":
                        if (followSymbols?.Count > 0)
                        {
                            query = query.Where(q => followSymbols.Contains(q.Code));
                        }
                        break;

                    case "suggestion":
                        var industries = CacheGetIndustriesSelectAsync().Result;
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
            var cacheKey = CacheKeyService.GetAllStockViewCacheKey();
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

        public virtual async Task<List<StockPrice>> CacheGetStockPricesForDetailPageAsync(string symbol)
        {
            var cacheKey = CacheKeyService.GetStockPricesByCodeCacheKey(symbol, 100000, null);
            return await _memoryCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _operationRetry.Retry(() => _stockPriceData.GetForDetailPageAsync(symbol, 100000), 10, TimeSpan.FromMilliseconds(100));
            }, _defaultCacheTime);
        }

        public virtual async Task<Stock> CacheGetStockByCodeAsync(string symbol)
        {
            var cacheKey = CacheKeyService.GetStockByCodeCacheKey(symbol);
            return await _memoryCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _stockData.GetByCodeAsync(symbol);
            }, _defaultCacheTime);
        }

        public virtual async Task<AnalyticsResult> CacheGetAnalyticsResultAsync(string symbol, string datePath)
        {
            var cacheKey = CacheKeyService.GetAnalyticsResultCacheKey(symbol, datePath);
            return await _memoryCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                var analyticsResults = await _operationRetry.Retry(() => _analyticsResultData.FindAllAsync(symbol, datePath, 1), 5, TimeSpan.FromMilliseconds(100));
                return analyticsResults.FirstOrDefault();
            }, _defaultCacheTime);
        }

        public virtual async Task<Company> CacheGetCompanyByCodeAsync(string symbol)
        {
            var cacheKey = CacheKeyService.GetCompanyByCodeCacheKey(symbol);
            return await _memoryCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _companyData.GetByCodeAsync(symbol);
            }, _defaultCacheTime);
        }

        public virtual async Task<QueueMessage> HandleStockViewEventAsync(QueueMessage queueMessage)
        {
            var schedule = await _operationRetry.Retry(() => _scheduleData.GetByIdAsync(queueMessage.Id), 5, TimeSpan.FromMilliseconds(100));
            if (schedule != null)
            {
                Stopwatch stopWatch = new();
                stopWatch.Start();

                switch (schedule.Type)
                {
                    case 300:
                        return await BindingStocksViewAndSetCacheAsync();

                    case 301:

                        return await NotificationAnalyticsAsync(schedule);

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
            var tradingDatePath = Utilities.GetTradingDatePath();
            var dictStockView = new Dictionary<string, StockView>();
            var companies = await _operationRetry.Retry(() => _companyData.FindAllAsync(null), 10, TimeSpan.FromMilliseconds(100));
            var stocks = await _stockData.FindAllAsync();
            foreach (var stock in stocks)
            {
                if (!dictStockView.ContainsKey(stock.Code))
                {
                    var stockPrices = await _operationRetry.Retry(() => _stockPriceData.GetForStockViewAsync(stock.Code, 3840), 10, TimeSpan.FromMilliseconds(100));
                    if (stockPrices?.Count < 2)
                    {
                        _logger.LogWarning("Can't build view data for stock code {Code} by stock price is lower five item.", stock.Code);
                        continue;
                    }
                    var company = companies.FirstOrDefault(q => q.Symbol == stock.Code);
                    if (company is null)
                    {
                        _logger.LogWarning("Can't build view data for stock code {Code} by company info is null.", stock.Code);
                        continue;
                    }
                    var analyticsResults = await _operationRetry.Retry(() => _analyticsResultData.FindAllAsync(stock.Code, tradingDatePath, 1), 10, TimeSpan.FromMilliseconds(100));
                    if (analyticsResults?.Count < 1)
                    {
                        _logger.LogWarning("Can't build view data for stock code {Code} by analytics result lower one item.", stock.Code);
                        continue;
                    }

                    var lastFinancialIndicator = await _financialIndicatorData.GetLastYearAsync(stock.Code);
                    var topThreeFinancialIndicator = await _financialIndicatorData.GetTopYearlyAsync(stock.Code, 4);
                    var stockPriceAdjs = BaseTrading.ConvertStockPricesToStockPriceAdj(stockPrices.Where(q => q.ClosePriceAdjusted > 0 && q.ClosePrice > 0));
                    var industry = await _industryData.GetByCodeAsync(company.SubsectorCode);
                    var topThreeHistories = stockPrices.Take(3).ToList();
                    var topFiveHistories = stockPrices.Take(5).ToList();
                    var topTenHistories = stockPrices.Take(10).ToList();
                    var stockView = new StockView()
                    {
                        Code = stock.Code,
                        IndustryCode = company.SubsectorCode,
                        Description = $"{stock.Exchange} - {stock.Description} - {company.Supersector} - {company.Sector}",
                        Exchange = stock.Exchange,

                        MacroeconomicsScore = analyticsResults[0].MacroeconomicsScore,
                        IndustryRank = MacroeconomicsAnalyticsService.IndustryTrend(new(), industry),

                        CompanyValueScore = analyticsResults[0].CompanyValueScore,
                        Eps = lastFinancialIndicator?.Eps ?? company.Eps,
                        Pe = lastFinancialIndicator?.Pe ?? company.Pe,
                        Pb = lastFinancialIndicator?.Pb ?? company.Pb,
                        Roe = (lastFinancialIndicator?.Roe ?? company.Roe) * 100,
                        Roa = (lastFinancialIndicator?.Roa ?? company.Roa) * 100,
                        MarketCap = company.MarketCap,

                        CompanyGrowthScore = analyticsResults[0].CompanyGrowthScore,

                        StockScore = analyticsResults[0].StockScore,
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

                        FiinScore = analyticsResults[0].FiinScore,
                        VndScore = analyticsResults[0].VndScore,
                        TargetPrice = analyticsResults[0].TargetPrice,

                        SsaPerdictPrice = analyticsResults[0].SsaPerdictPrice,
                        FttPerdictPrice = analyticsResults[0].FttPerdictPrice,
                        TrendPrediction = analyticsResults[0].SdcaPriceTrend,
                    };

                    if (topThreeFinancialIndicator?.Count > 3)
                    {
                        var (_, _, _, _, _, _, percents) = topThreeFinancialIndicator.GetFluctuationsTopDown(q => q.Revenue);
                        stockView.YearlyRevenueGrowthPercent = percents?.Average() ?? 0;
                    }
                    if (topThreeFinancialIndicator?.Count > 3)
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

                    #endregion Count change

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

                    #endregion Convulsion

                    #region Trading info

                    var tradingResults = await _tradingResultData.GetForViewAsync(stock.Code, 10);
                    var indicatorSet = BaseTrading.BuildIndicatorSet(stockPriceAdjs);
                    var tradingCases = AnalyticsService.AnalyticsBuildTradingCases();
                    var principles = new int[] { 0, 1, 2, 3, 4 };

                    foreach (var principle in principles)
                    {
                        var sunTradingResults = tradingResults.Where(q => q.Principle == principle).OrderBy(q => q.TradingDate).ToList();
                        if (sunTradingResults?.Count > 0)
                        {
                            var judgeResult = new JudgeResult()
                            {
                                OptimalBuyPrice = sunTradingResults[^1].BuyPrice,
                                OptimalSellPrice = sunTradingResults[^1].SellPrice,
                                ProfitPercent = sunTradingResults[^1].ProfitPercent,
                                TodayIsBuy = sunTradingResults[^1].IsBuy,
                                TodayIsSell = sunTradingResults[^1].IsSell
                            };
                            if (indicatorSet.ContainsKey(stockPrices[0].DatePath) && tradingCases.ContainsKey(principle))
                            {
                                var todaySet = indicatorSet[stockPrices[0].DatePath];
                                var tradingCase = tradingCases[principle];
                                judgeResult.Stochastic = todaySet.Values[$"stochastic-{tradingCase.Stochastic}"];
                                judgeResult.BuySubtractionEma = todaySet.Values[$"ema-{tradingCase.FirstEmaBuy}"] - todaySet.Values[$"ema-{tradingCase.SecondEmaBuy}"];
                                judgeResult.SellSubtractionEma = todaySet.Values[$"ema-{tradingCase.FirstEmaSell}"] - todaySet.Values[$"ema-{tradingCase.SecondEmaSell}"];
                                var currentBuyEmaType = (todaySet.Values[$"ema-{tradingCase.FirstEmaBuy}"] - todaySet.Values[$"ema-{tradingCase.SecondEmaBuy}"]) > 0;
                                var currentSellEmaType = (todaySet.Values[$"ema-{tradingCase.FirstEmaSell}"] - todaySet.Values[$"ema-{tradingCase.SecondEmaSell}"]) > 0;
                                for (int i = indicatorSet.Count - 1; i >= 0; i--)
                                {
                                    var checkItem = indicatorSet.ElementAt(i).Value;
                                    if (currentBuyEmaType == ((checkItem.Values[$"ema-{tradingCase.FirstEmaBuy}"] - checkItem.Values[$"ema-{tradingCase.SecondEmaBuy}"]) > 0))
                                    {
                                        judgeResult.BuyEmaReverseCount++;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                for (int i = indicatorSet.Count - 1; i >= 0; i--)
                                {
                                    var checkItem = indicatorSet.ElementAt(i).Value;
                                    if (currentSellEmaType == ((checkItem.Values[$"ema-{tradingCase.FirstEmaSell}"] - checkItem.Values[$"ema-{tradingCase.SecondEmaSell}"]) > 0))
                                    {
                                        judgeResult.SellEmaReverseCount++;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            stockView.TradingViews.Add(principle, judgeResult);
                        }
                    }

                    #endregion Trading info

                    dictStockView.TryAdd(stock.Code, stockView);
                }
            }
            var cacheKey = CacheKeyService.GetAllStockViewCacheKey();
            await _asyncCacheService.SetValueAsync(cacheKey, dictStockView, _defaultCacheTime * 3800);
            return new()
            {
                Id = "UpdateStockView"
            };
        }

        public async Task<QueueMessage> NotificationAnalyticsAsync(Schedule schedule)
        {
            if (!string.IsNullOrEmpty(schedule.DataKey))
            {
                var todayDate = DateTime.Now.Date;
                var stockCodes = new List<string>();

                var userFollows = await _followStockData.FindAllAsync(schedule.DataKey);
                foreach (var userFollow in userFollows)
                {
                    if (!stockCodes.Contains(userFollow.Symbol))
                    {
                        stockCodes.Add(userFollow.Symbol);
                    }
                }

                if (stockCodes?.Count > 0)
                {
                    var insertList = new List<UserNotification>();
                    foreach (var code in stockCodes)
                    {
                        var stock = await _stockData.GetByCodeAsync(code);
                        if (stock is null)
                        {
                            continue;
                        }
                        //Thông tin sư kiện
                        var corporateActions = await _corporateActionData.NotificationFindAllAsync(code, todayDate);
                        foreach (var corporateAction in corporateActions)
                        {
                            insertList.Add(new UserNotification()
                            {
                                Symbol = code,
                                Readed = false,
                                UserId = schedule.DataKey,
                                Title = corporateAction.EventTitle,
                                ZipMessage = corporateAction.ZipDescription
                            });
                        }

                        //Phân tích và đánh giá doanh nghiệp
                        var datePath = Utilities.GetTradingDatePath();
                        var lastDatePath = Utilities.GetLastTradingDatePath();
                        var analytic = await _analyticsResultData.FindAsync(code, datePath);
                        var lastAnalytic = await _analyticsResultData.FindAsync(code, lastDatePath);
                        if (analytic is not null && lastAnalytic is not null)
                        {
                            if (analytic.TotalScore > lastAnalytic.TotalScore)
                            {
                                insertList.Add(new UserNotification()
                                {
                                    Symbol = code,
                                    Readed = false,
                                    UserId = schedule.DataKey,
                                    Title = $"Cổ phiếu '{code}' có sự thay đổi về đánh giá doanh nghiệp hiện tại {analytic.TotalScore}, trước đó {lastAnalytic.TotalScore}",
                                    ZipMessage = _zipHelper.ZipByte(Encoding.UTF8.GetBytes($"Cổ phiếu '{code}' có sự biến về đánh giá doanh nghiệp hiện tại {analytic.TotalScore}, trước đó {lastAnalytic.TotalScore}. Chi tiết xin xem <a href=\"#\" onclick=\"OpenModalDetails('{code}', '{stock.Exchange} {stock.CompanyName}', 1)\">tại đây</a>"))
                                });
                            }
                        }

                        //Phân tích và đánh giá trading
                        var tradingResults = await _tradingResultData.FindAllAsync(code, datePath, null, 999);
                        foreach (var tradingResult in tradingResults)
                        {
                            if (!tradingResult.IsBuy && !tradingResult.IsSell)
                            {
                                continue;
                            }

                            var principleName = Utilities.GetPrincipleName(tradingResult.Principle);
                            if (tradingResult.IsBuy && tradingResult.IsSell)
                            {
                                insertList.Add(new UserNotification()
                                {
                                    Symbol = code,
                                    Readed = false,
                                    UserId = schedule.DataKey,
                                    Title = $"{principleName} đánh giá '{code}' mua và bán.",
                                    ZipMessage = _zipHelper.ZipByte(Encoding.UTF8.GetBytes($"Cổ phiếu '{code}' được phương pháp {principleName} đánh giá mua với giá {tradingResult.BuyPrice / 1000:0.00}, và bán với giá {tradingResult.SellPrice / 1000:0.00}. Chi tiết xin xem <a href=\"#\" onclick=\"OpenModalDetails('{code}', '{stock.Exchange} {stock.CompanyName}', 1)\">tại đây</a>"))
                                });
                                continue;
                            }
                            if (tradingResult.IsBuy)
                            {
                                insertList.Add(new UserNotification()
                                {
                                    Symbol = code,
                                    Readed = false,
                                    UserId = schedule.DataKey,
                                    Title = $"{principleName} đánh giá '{code}' mua.",
                                    ZipMessage = _zipHelper.ZipByte(Encoding.UTF8.GetBytes($"Cổ phiếu '{code}' được phương pháp {principleName} đánh giá mua với giá {tradingResult.BuyPrice / 1000:0.00}. Chi tiết xin xem <a href=\"#\" onclick=\"OpenModalDetails('{code}', '{stock.Exchange} {stock.CompanyName}', 1)\">tại đây</a>"))
                                });
                                continue;
                            }
                            else
                            {
                                insertList.Add(new UserNotification()
                                {
                                    Symbol = code,
                                    Readed = false,
                                    UserId = schedule.DataKey,
                                    Title = $"{principleName} đánh giá '{code}' bán.",
                                    ZipMessage = _zipHelper.ZipByte(Encoding.UTF8.GetBytes($"Cổ phiếu '{code}' được phương pháp {principleName} đánh giá bán với giá {tradingResult.SellPrice / 1000:0.00}. Chi tiết xin xem <a href=\"#\" onclick=\"OpenModalDetails('{code}', '{stock.Exchange} {stock.CompanyName}', 1)\">tại đây</a>"))
                                });
                                continue;
                            }
                        }
                    }
                    await _userNotificationData.BulkInserAsync(insertList);
                    return new()
                    {
                        Id = "UpdateNotification",
                        KeyValues = new Dictionary<string, string>() { { "UserId", schedule.DataKey } }
                    };
                }
            }
            return null;
        }
    }
}