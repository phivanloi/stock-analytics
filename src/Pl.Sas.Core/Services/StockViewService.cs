using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using Skender.Stock.Indicators;
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
        private readonly IStockPriceData _stockPriceData;
        private readonly IFinancialIndicatorData _financialIndicatorData;
        private readonly ITradingResultData _tradingResultData;
        private readonly IStockData _stockData;
        private readonly IKeyValueData _keyValueData;
        private readonly IVndStockScoreData _vndStockScoreData;

        public StockViewService(
            IVndStockScoreData vndStockScoreData,
            IKeyValueData keyValueData,
            IStockData stockData,
            ITradingResultData tradingResultData,
            IFinancialIndicatorData financialIndicatorData,
            IStockPriceData stockPriceData,
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
            _companyData = companyData;
            _chartPriceData = chartPriceData;
            _scheduleData = scheduleData;
            _industryData = industryData;
            _asyncCacheService = asyncCacheService;
            _logger = logger;
            _memoryCacheService = memoryCacheService;
            _keyValueData = keyValueData;
            _vndStockScoreData = vndStockScoreData;
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
                    case "all":
                        if (string.IsNullOrEmpty(symbol))
                        {
                            query = query.Where(q => q.KlptValue > 10000);
                        }
                        break;

                    case "me":
                        if (followSymbols?.Count > 0)
                        {
                            query = query.Where(q => followSymbols.Contains(q.Symbol));
                        }
                        break;

                    case "suggestion":
                        var industries = CacheGetIndustriesAsync().Result;
                        query = query.Where(q =>
                        q.ScoreValue > 15
                        && q.IndustryCode == industries.FirstOrDefault().Key);
                        break;
                }
            }

            if (!string.IsNullOrEmpty(ordinal))
            {
                return ordinal switch
                {
                    "klk" => query.OrderByDescending(q => q.KlhtValue).ToList(),
                    "ddgdn" => query.OrderByDescending(q => q.ScoreValue).ToList(),
                    "td1p" => query.OrderByDescending(q => q.Bd2Value).ToList(),
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
            if (_stockViews.ContainsKey(symbol))
            {
                _stockViews[symbol] = stockView;
            }
            else
            {
                _stockViews.TryAdd(symbol, stockView);
            }
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
            var company = await _companyData.FindBySymbolAsync(symbol);
            if (company is null)
            {
                _logger.LogWarning("Can't build view data for stock code {symbol} by company info is null.", symbol);
                return null;
            }
            var chartPrices = await _chartPriceData.CacheFindAllAsync(symbol, "D");
            var stockPrices = await _stockPriceData.GetForStockViewAsync(symbol, 10);
            var financialIndicators = await _financialIndicatorData.GetTopAsync(symbol, 15);
            var industry = await _industryData.GetByCodeAsync(company.SubsectorCode);
            var vndScore = await _vndStockScoreData.FindAsync(symbol, "100000");
            var bankInterestRate12 = await _keyValueData.CacheGetAsync(Constants.BankInterestRate12Key);
            var stockView = new StockView
            {
                Symbol = symbol,
                IndustryCode = company.SubsectorCode,
                Description = $"{company.Exchange} - {company.CompanyName} - {company.Supersector} - {company.Sector}",
                Exchange = company.Exchange.ToUpper().Trim(),
                Beta = company.Beta.ShowPercent(),
                BetaCss = "beta t-r" + (company.Beta <= 1 ? " t-wn" : company.Beta <= 1.4f ? "" : " t-s")
            };

            if (vndScore is not null)
            {
                stockView.ScoreValue = vndScore.Point;
                stockView.Score = vndScore.Point.ToString("0.0");
                stockView.ScoreCss = "score t-r" + (vndScore.Point <= 2 ? " t-wn" : vndScore.Point <= 5 ? "" : " t-s");
            }

            if (financialIndicators is not null && financialIndicators.Count > 0)//Chỉ số tài chính
            {
                stockView.Eps = financialIndicators[0]?.Eps ?? company.Eps;
                stockView.Pe = financialIndicators[0]?.Pe ?? company.Pe;
                stockView.Pb = financialIndicators[0]?.Pb ?? company.Pb;
                stockView.Roe = (financialIndicators[0]?.Roe ?? company.Roe) * 100;
                stockView.Roa = (financialIndicators[0]?.Roa ?? company.Roa) * 100;

                var sourceItem = financialIndicators.FirstOrDefault(q => q.LengthReport != 5);
                if (sourceItem is not null)
                {
                    var compareItem = financialIndicators.FirstOrDefault(q => q.LengthReport == sourceItem.LengthReport && q.YearReport == sourceItem.YearReport - 1);
                    if (compareItem is not null)
                    {
                        var quarterlyPercentProfit = sourceItem.Profit.GetPercent(compareItem.Profit);
                        stockView.Lnq = quarterlyPercentProfit.ToString("0,0.0");
                        if (quarterlyPercentProfit > 30)
                        {
                            stockView.LnqCss = "lnq t-r t-s";
                        }
                        else if (quarterlyPercentProfit < 5)
                        {
                            stockView.LnqCss = "lnq t-r t-d";
                        }
                    }
                }

                if (financialIndicators.Count > 3)
                {
                    var topYearly = financialIndicators.Where(q => q.LengthReport == 5).Take(3).ToList();
                    if (topYearly.Count >= 3)
                    {
                        var yearlyPercentProfit = (topYearly[0].Profit.GetPercent(topYearly[1].Profit) + topYearly[1].Profit.GetPercent(topYearly[2].Profit)) / 2;
                        stockView.Lnn = yearlyPercentProfit.ToString("0,0.0");
                        if (yearlyPercentProfit > 25)
                        {
                            stockView.LnnCss = "lnn t-r t-s";
                        }
                        else if (yearlyPercentProfit < 5)
                        {
                            stockView.LnnCss = "lnn t-r t-d";
                        }
                    }
                }
            }

            if ((chartPrices is null || chartPrices.Count <= 0) && stockPrices is not null && stockPrices.Count > 0)
            {
                chartPrices = stockPrices.Select(q => q.ToChartPrice()).ToList();
            }

            if (chartPrices is not null && chartPrices.Count > 0)
            {
                BindingCashFlows(ref stockView, chartPrices, industry);

                #region Hỗ trợ và kháng cự
                chartPrices = chartPrices.OrderBy(q => q.TradingDate).ToList();
                var quotes = chartPrices.Select(q => q.ToQuote()).OrderBy(q => q.Date).ToList();
                var zigZagPercentChange = Math.Ceiling((decimal)(company.Beta * 10 / 2)) + 2;
                if (zigZagPercentChange < 5)
                {
                    zigZagPercentChange = 5;
                }
                if (zigZagPercentChange > 15)
                {
                    zigZagPercentChange = 15;
                }
                var zigZagResults = quotes.GetZigZag(EndType.HighLow, zigZagPercentChange);
                var zigZagResultH = zigZagResults.LastOrDefault(q => q.PointType == "H" && ((decimal)chartPrices[^1].ClosePrice < (q.ZigZag - (q.ZigZag * 0.01m))));
                var zigZagResultL = zigZagResults.LastOrDefault(q => q.PointType == "L" && ((decimal)chartPrices[^1].ClosePrice > (q.ZigZag + (q.ZigZag * 0.01m))));
                if (zigZagResultH is not null && zigZagResultH.ZigZag.HasValue)
                {
                    stockView.Ngkc = zigZagResultH.ZigZag.Value.ToString("00.00");
                    stockView.NgkcCss = "ngkc t-r t-d";
                }

                if (zigZagResultL is not null && zigZagResultL.ZigZag.HasValue)
                {
                    stockView.Nght = zigZagResultL.ZigZag.Value.ToString("00.00");
                    stockView.NghtCss = "nght t-r t-s";
                }
                #endregion

                BindingPercentConvulsionToView(ref stockView, chartPrices);
            }

            var tradingResults = await _tradingResultData.GetForViewAsync(symbol);
            BindingTradingResultToView(ref stockView, tradingResults, bankInterestRate12?.GetValue<float>() ?? 6.8f);

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
                    _stockViews.TryAdd(stock.Symbol, stockView);
                }
            }
        }

        public static void BindingPercentConvulsionToView(ref StockView stockView, List<ChartPrice> chartPrices)
        {
            var checkChartPrices = chartPrices.OrderByDescending(q => q.TradingDate).ToList();
            var lastPercent = checkChartPrices[0].ClosePrice.GetPercent(checkChartPrices[^1].ClosePrice);
            stockView.Ght = checkChartPrices[0].ClosePrice.ShowPrice(1);

            stockView.KlhtValue = checkChartPrices[0].TotalMatchVol;
            stockView.Klht = checkChartPrices[0].TotalMatchVol.ShowMoney(1);

            #region Stoch Rsi
            var quotes = chartPrices.Select(q => q.ToQuote()).OrderBy(q => q.Date).ToList();
            var (Rsi, Score) = StockTechnicalAnalytics.StochRsiAnalytics(new(), quotes);
            stockView.Rsi14 = Rsi?.ToString("00.00") ?? "";
            stockView.Rsi14Css = Score switch
            {
                2 => "rsi14 t-r t-gt",
                1 => "rsi14 t-r t-s",
                -1 => "rsi14 t-r t-d",
                -2 => "rsi14 t-r t-gs",
                _ => "rsi14 t-r",
            };
            #endregion

            var currentPercent = 0f;
            if (checkChartPrices.Count >= 2)
            {
                currentPercent = checkChartPrices[0].ClosePrice.GetPercent(checkChartPrices[1].ClosePrice);
                stockView.KlptValue = checkChartPrices[1].TotalMatchVol;
                stockView.Bd2Value = currentPercent;
                stockView.Bd2 = currentPercent.ShowPercent();
                stockView.Bd2Css = "bd2 t-r " + currentPercent.GetTextColorCss();
                stockView.GhtCss = "ght t-r " + currentPercent.GetTextColorCss();

                var avg30MatchVol = chartPrices.Where(q => q.TradingDate < checkChartPrices[0].TradingDate)
                    .OrderByDescending(q => q.TradingDate)
                    .Take(30)
                    .Average(q => q.TotalMatchVol);

                if (checkChartPrices[0].TotalMatchVol > (avg30MatchVol + avg30MatchVol * 0.1))
                {
                    if (currentPercent > 0)
                    {
                        stockView.KlhtCss = "klht t-r t-gt";
                    }
                    else
                    {
                        stockView.KlhtCss = "klht t-r t-gs";
                    }
                }
                else
                {
                    stockView.KlhtCss = "klht t-r " + currentPercent.GetTextColorCss();
                }
            }
            else
            {
                stockView.Bd2Value = lastPercent;
                stockView.Bd2 = lastPercent.ShowPercent();
                stockView.Bd2Css = "bd2 t-r " + lastPercent.GetTextColorCss();
                stockView.GhtCss = "ght t-r " + lastPercent.GetTextColorCss();
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

        public static void BindingTradingResultToView(ref StockView stockView, List<TradingResult>? tradingResults, float bankInterestRate12)
        {
            if (tradingResults is not null && tradingResults.Count > 0)
            {
                foreach (var result in tradingResults)
                {
                    if (result.Principle == 0)
                    {
                        stockView.Lnnh = result.ProfitPercent.ShowPercent();
                        stockView.LnnhCss = $"lnnh t-r {result.ProfitPercent.GetTextColorCss(bankInterestRate12)}";
                        stockView.Knnh = result.AssetPosition;
                        if (result.IsBuy)
                        {
                            stockView.KnnhCss = "knnh t-c t-m";
                        }
                        else if (result.IsSell)
                        {
                            stockView.KnnhCss = "knnh t-c t-b";
                        }
                        else
                        {
                            stockView.KnnhCss = "knnh t-c";
                        }
                    }
                    else if (result.Principle == 1)
                    {
                        stockView.Lnth = result.ProfitPercent.ShowPercent();
                        stockView.LnthCss = $"lnth t-r {result.ProfitPercent.GetTextColorCss(bankInterestRate12)}";
                        stockView.Knth = result.AssetPosition;
                        if (result.IsBuy)
                        {
                            stockView.KnthCss = "knth t-c t-m";
                        }
                        else if (result.IsSell)
                        {
                            stockView.KnthCss = "knth t-c t-b";
                        }
                        else
                        {
                            stockView.KnthCss = "knth t-c";
                        }
                    }
                    else if (result.Principle == 2)
                    {
                        stockView.Lntn = result.ProfitPercent.ShowPercent();
                        stockView.LntnCss = $"lntn t-r {result.ProfitPercent.GetTextColorCss(bankInterestRate12)}";
                        stockView.Kntn = result.AssetPosition;
                        if (result.IsBuy)
                        {
                            stockView.KntnCss = "kntn t-c t-m";
                        }
                        else if (result.IsSell)
                        {
                            stockView.KntnCss = "kntn t-c t-b";
                        }
                        else
                        {
                            stockView.KntnCss = "kntn t-c";
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

        public static void BindingCashFlows(ref StockView stockView, List<ChartPrice> chartPrices, Industry? industry)
        {
            var score = StockTechnicalAnalytics.PriceTrend(new(), chartPrices);
            if (score > -1000)
            {
                stockView.Scf = score.ToString("0,0");
                if (score > 10)
                {
                    stockView.ScfCss = "scf t-r t-s";
                }
                if (score < -10)
                {
                    stockView.ScfCss = "scf t-r t-d";
                }
            }

            if (industry is not null)
            {
                stockView.Icf = industry.AutoRank.ToString("0,0");
                if (industry.AutoRank > 10)
                {
                    stockView.IcfCss = "scf t-r t-s";
                }
                if (industry.AutoRank < -10)
                {
                    stockView.IcfCss = "scf t-r t-d";
                }
            }
        }
    }
}