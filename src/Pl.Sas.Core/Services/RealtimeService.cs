using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Core.Trading;
using Skender.Stock.Indicators;
using System.Diagnostics;
using System.Text.Json;

namespace Pl.Sas.Core.Services
{
    /// <summary>
    /// xử lý các nghiệp vụ realtime
    /// </summary>
    public class RealtimeService
    {
        private readonly ILogger<RealtimeService> _logger;
        private readonly IWorkerQueueService _workerQueueService;
        private readonly IScheduleData _scheduleData;
        private readonly IChartPriceData _chartPriceData;
        private readonly IAsyncCacheService _asyncCacheService;
        private readonly IKeyValueData _keyValueData;
        private readonly IStockData _stockData;

        public RealtimeService(
            IStockData stockData,
            IKeyValueData keyValueData,
            IAsyncCacheService asyncCacheService,
            IScheduleData scheduleData,
            IChartPriceData chartPriceData,
            IWorkerQueueService workerQueueService,
            ILogger<RealtimeService> logger)
        {
            _chartPriceData = chartPriceData;
            _logger = logger;
            _workerQueueService = workerQueueService;
            _scheduleData = scheduleData;
            _asyncCacheService = asyncCacheService;
            _keyValueData = keyValueData;
            _stockData = stockData;
        }

        public async Task HandleEventAsync(QueueMessage queueMessage)
        {
            Stopwatch stopWatch = new();
            stopWatch.Start();

            switch (queueMessage.Id)
            {
                case "SetupRealtimeSleepTimeByTransactionCount":
                    await UpdateSleepTimeForRealtimeTaskAsync(queueMessage.KeyValues["Symbol"], long.Parse(queueMessage.KeyValues["TransactionCount"]));
                    break;
                case "TestTradingOnPriceChange":
                    await UpdateViewRealtimeOnPriceChange(queueMessage.KeyValues["Symbol"], queueMessage.KeyValues["ChartPrices"]);
                    break;
                case "UpdateWorldIndex":
                    await UpdateWorldIndexChange(queueMessage.KeyValues["WorldIndexs"]);
                    break;
                case "IndexValuationChange":
                    await UpdateValuationIndexChange(queueMessage.KeyValues["IndexValuation"]);
                    break;
                default:
                    _logger.LogWarning("Realtime task id {Id} don't match any function", queueMessage.Id);
                    break;
            }

            stopWatch.Stop();
            _logger.LogInformation("Realtime task {Id} run in {ElapsedMilliseconds} miniseconds.", queueMessage.Id, stopWatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Hàm xử lý cập nhập thời gian xử lý realtime cho các mã chứng khoán có ít giao dịch
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <param name="chartPricesJsonString">Danh sách lịch sử giá cập nhập tự động</param>
        /// <returns></returns>
        public virtual async Task UpdateViewRealtimeOnPriceChange(string symbol, string chartPricesJsonString)
        {
            Guard.Against.NullOrEmpty(symbol, nameof(symbol));
            Guard.Against.NullOrEmpty(chartPricesJsonString, nameof(chartPricesJsonString));
            var chartPricesRealtime = JsonSerializer.Deserialize<List<ChartPrice>>(chartPricesJsonString);
            if (chartPricesRealtime is null || chartPricesRealtime.Count <= 0)
            {
                return;
            }

            var stock = await _stockData.FindBySymbolAsync(symbol);
            var chartPrices = await _chartPriceData.CacheFindAllAsync(symbol, "D");
            if (chartPrices is null || chartPrices.Count <= 0 || stock is null)
            {
                return;
            }

            var bankInterestRate12 = await _keyValueData.CacheGetAsync(Constants.BankInterestRate12Key);
            var cacheKey = $"{Constants.StockViewCachePrefix}-SM-{symbol}";
            var stockViewTask = _asyncCacheService.GetByKeyAsync<StockView>(cacheKey);

            foreach (var realtimeItem in chartPricesRealtime)
            {
                var chartPrice = chartPrices.FirstOrDefault(q => q.TradingDate == realtimeItem.TradingDate);
                if (chartPrice is null)
                {
                    chartPrices.Add(realtimeItem);
                }
                else
                {
                    chartPrice.TotalMatchVol = realtimeItem.TotalMatchVol;
                    chartPrice.ClosePrice = realtimeItem.ClosePrice;
                    chartPrice.LowestPrice = realtimeItem.LowestPrice;
                    chartPrice.HighestPrice = realtimeItem.HighestPrice;
                    chartPrice.OpenPrice = realtimeItem.OpenPrice;
                }
            }
            chartPrices = chartPrices.OrderBy(q => q.TradingDate).ToList();
            var chartTrading = chartPrices.Where(q => q.TradingDate >= Constants.StartTime).OrderBy(q => q.TradingDate).ToList();

            var stockView = await stockViewTask;
            if (stockView is null)
            {
                _logger.LogWarning("UpdateViewRealtimeOnPriceChange {symbol} is null stockView from cache", symbol);
                return;
            }

            var listTradingResult = new List<TradingResult>();
            #region Experiment Trading
            var experimentTrading = new ExperimentTrading(chartPrices);
            var tradingHistory = chartPrices.Where(q => q.TradingDate < Constants.StartTime).OrderBy(q => q.TradingDate).ToList();
            var experCase = experimentTrading.Trading(chartTrading, tradingHistory, stock.Exchange, false);
            listTradingResult.Add(new()
            {
                Symbol = symbol,
                Principle = 0,
                IsBuy = experCase.IsBuy,
                IsSell = experCase.IsSell,
                BuyPrice = experCase.BuyPrice,
                SellPrice = experCase.SellPrice,
                FixedCapital = experCase.FixedCapital,
                Profit = experCase.Profit(chartTrading[^1].ClosePrice),
                TotalTax = experCase.TotalTax,
                TradingNotes = null,
                AssetPosition = experCase.AssetPosition,
                LoseNumber = experCase.LoseNumber,
                WinNumber = experCase.WinNumber,
            });
            #endregion

            #region Main Trading
            var macdTrading = new MacdTrading(chartPrices);
            tradingHistory = chartPrices.Where(q => q.TradingDate < Constants.StartTime).OrderBy(q => q.TradingDate).ToList();
            var macdCase = macdTrading.Trading(chartTrading, tradingHistory, stock.Exchange, false);
            listTradingResult.Add(new()
            {
                Symbol = symbol,
                Principle = 1,
                IsBuy = macdCase.IsBuy,
                IsSell = macdCase.IsSell,
                BuyPrice = macdCase.BuyPrice,
                SellPrice = macdCase.SellPrice,
                FixedCapital = macdCase.FixedCapital,
                Profit = macdCase.Profit(chartTrading[^1].ClosePrice),
                TotalTax = macdCase.TotalTax,
                TradingNotes = null,
                AssetPosition = macdCase.AssetPosition,
                LoseNumber = macdCase.LoseNumber,
                WinNumber = macdCase.WinNumber,
            });
            #endregion

            #region Buy and wait
            var startPrice = chartTrading[0].ClosePrice;
            var endPrice = chartTrading[^1].ClosePrice;
            listTradingResult.Add(new TradingResult()
            {
                Symbol = symbol,
                Principle = 3,
                IsBuy = false,
                IsSell = false,
                BuyPrice = chartPrices[^1].ClosePrice,
                SellPrice = chartPrices[^1].ClosePrice,
                FixedCapital = 100000000,
                Profit = (100000000 / startPrice) * endPrice,
                TotalTax = 0,
                TradingNotes = null,
                AssetPosition = "100% C",
                LoseNumber = startPrice <= endPrice ? 1 : 0,
                WinNumber = startPrice > endPrice ? 1 : 0,
            });
            #endregion

            StockViewService.BindingPercentConvulsionToView(ref stockView, chartPrices);
            StockViewService.BindingTradingResultToView(ref stockView, listTradingResult, bankInterestRate12?.GetValue<float>() ?? 6.8f);

            var setViewCacheKey = $"{Constants.StockViewCachePrefix}-SM-{symbol}";
            var setCacheTask = _asyncCacheService.SetValueAsync(setViewCacheKey, stockView, Constants.DefaultCacheTime * 60 * 24 * 30);
            var sendMessage = new QueueMessage("UpdateRealtimeView");
            sendMessage.KeyValues.Add("Data", JsonSerializer.Serialize(stockView));
            sendMessage.KeyValues.Add("Symbol", symbol);
            _workerQueueService.BroadcastViewUpdatedTask(sendMessage);
            await setCacheTask;
        }

        /// <summary>
        /// Hàm xử lý cập nhập thời gian xử lý realtime cho các mã chứng khoán có ít giao dịch
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <param name="transactionCont">Số giao dịch trong ngày</param>
        /// <returns></returns>
        public virtual async Task<bool> UpdateSleepTimeForRealtimeTaskAsync(string symbol, long transactionCont)
        {
            Guard.Against.NullOrEmpty(symbol, nameof(symbol));
            var type14 = await _scheduleData.FindAsync(14, symbol);
            if (type14 != null)
            {
                if (transactionCont > 10000)
                {
                    type14.AddOrUpdateOptions("SleepTime", "30");
                }
                else if (transactionCont > 8000)
                {
                    type14.AddOrUpdateOptions("SleepTime", "60");
                }
                else if (transactionCont > 5000)
                {
                    type14.AddOrUpdateOptions("SleepTime", "100");
                }
                else if (transactionCont > 3000)
                {
                    type14.AddOrUpdateOptions("SleepTime", "140");
                }
                else if (transactionCont > 1000)
                {
                    type14.AddOrUpdateOptions("SleepTime", "200");
                }
                else if (transactionCont > 500)
                {
                    type14.AddOrUpdateOptions("SleepTime", "360");
                }
                else if (transactionCont > 100)
                {
                    type14.AddOrUpdateOptions("SleepTime", "600");
                }
                else
                {
                    type14.AddOrUpdateOptions("SleepTime", "1200");
                }
                return await _scheduleData.UpdateAsync(type14);
            }
            return false;
        }

        /// <summary>
        /// Hàm xử lý update khi thay đổi chỉ số quốc tế
        /// </summary>
        /// <param name="worldIndexsJsonData">Danh sách lịch sử giá cập nhập tự động</param>
        public virtual async Task UpdateWorldIndexChange(string worldIndexsJsonData)
        {
            Guard.Against.NullOrEmpty(worldIndexsJsonData, nameof(worldIndexsJsonData));
            var marketDepths = JsonSerializer.Deserialize<List<Entities.DownloadObjects.MarketDepth>>(worldIndexsJsonData);
            if (marketDepths is null || marketDepths.Count <= 0)
            {
                return;
            }

            var setViewCacheKey = $"{Constants.IndexViewCachePrefix}-ALL";
            var indexView = await _asyncCacheService.GetByKeyAsync<IndexView>(setViewCacheKey);
            if (indexView == null)
            {
                indexView = new IndexView();
            }

            var indexDepth = marketDepths.FirstOrDefault(q => q.WorldIndexCode == "DJI");
            if (indexDepth is not null)
            {
                indexView.Dji = $"{indexDepth.IndexValue:0,0.00} ({indexDepth.IndexChange:0,0.00})";
                indexView.DjiCss = "dji t-s";
                if (indexDepth.IndexChange == 0)
                {
                    indexView.DjiCss = "dji t-wn";
                }
                else if (indexDepth.IndexChange < 0)
                {
                    indexView.DjiCss = "dji t-d";
                }
            }

            indexDepth = marketDepths.FirstOrDefault(q => q.WorldIndexCode == "NASDAQ");
            if (indexDepth is not null)
            {
                indexView.Nasdaq = $"{indexDepth.IndexValue:0,0.00} ({indexDepth.IndexChange:0,0.00})";
                indexView.NasdaqCss = "nasdaq t-s";
                if (indexDepth.IndexChange == 0)
                {
                    indexView.NasdaqCss = "nasdaq t-wn";
                }
                else if (indexDepth.IndexChange < 0)
                {
                    indexView.NasdaqCss = "nasdaq t-d";
                }
            }

            indexDepth = marketDepths.FirstOrDefault(q => q.WorldIndexCode == "DAX");
            if (indexDepth is not null)
            {
                indexView.Dax = $"{indexDepth.IndexValue:0,0.00} ({indexDepth.IndexChange:0,0.00})";
                indexView.DaxCss = "dax t-s";
                if (indexDepth.IndexChange == 0)
                {
                    indexView.DaxCss = "dax t-wn";
                }
                else if (indexDepth.IndexChange < 0)
                {
                    indexView.DaxCss = "dax t-d";
                }
            }

            indexDepth = marketDepths.FirstOrDefault(q => q.WorldIndexCode == "HANGSENG");
            if (indexDepth is not null)
            {
                indexView.Hangseng = $"{indexDepth.IndexValue:0,0.00} ({indexDepth.IndexChange:0,0.00})";
                indexView.HangsengCss = "hangseng t-s";
                if (indexDepth.IndexChange == 0)
                {
                    indexView.HangsengCss = "hangseng t-wn";
                }
                else if (indexDepth.IndexChange < 0)
                {
                    indexView.HangsengCss = "hangseng t-d";
                }
            }

            indexDepth = marketDepths.FirstOrDefault(q => q.WorldIndexCode == "SHANGHAI");
            if (indexDepth is not null)
            {
                indexView.Shanghai = $"{indexDepth.IndexValue:0,0.00} ({indexDepth.IndexChange:0,0.00})";
                indexView.ShanghaiCss = "shanghai t-s";
                if (indexDepth.IndexChange == 0)
                {
                    indexView.ShanghaiCss = "shanghai t-wn";
                }
                else if (indexDepth.IndexChange < 0)
                {
                    indexView.ShanghaiCss = "shanghai t-d";
                }
            }

            indexDepth = marketDepths.FirstOrDefault(q => q.WorldIndexCode == "VNINDEX");
            if (indexDepth is not null)
            {
                indexView.Vnindex = $"{indexDepth.IndexValue:0,0.00} ({indexDepth.IndexChange:0,0.00})";
                indexView.VnindexCss = "vnindex t-s";
                if (indexDepth.IndexChange == 0)
                {
                    indexView.VnindexCss = "vnindex t-wn";
                }
                else if (indexDepth.IndexChange < 0)
                {
                    indexView.VnindexCss = "vnindex t-d";
                }
            }

            var setCacheTask = _asyncCacheService.SetValueAsync(setViewCacheKey, indexView, Constants.DefaultCacheTime * 60 * 24);
            var sendMessage = new QueueMessage("UpdateIndexView");
            sendMessage.KeyValues.Add("Data", JsonSerializer.Serialize(indexView));
            _workerQueueService.BroadcastViewUpdatedTask(sendMessage);
            await setCacheTask;
        }

        /// <summary>
        /// Hàm xử lý khi thay đổi định giá
        /// </summary>
        /// <param name="valuationIndexsJsonData">Danh sách các định giá thị trường</param>
        public virtual async Task UpdateValuationIndexChange(string valuationIndexsJsonData)
        {
            Guard.Against.NullOrEmpty(valuationIndexsJsonData, nameof(valuationIndexsJsonData));
            var indexValuation = JsonSerializer.Deserialize<Dictionary<string, List<Entities.DownloadObjects.FinIndexValuation>>>(valuationIndexsJsonData);
            if (indexValuation is null || indexValuation.Count <= 0 || !indexValuation.ContainsKey("VNINDEX"))
            {
                return;
            }

            var setViewCacheKey = $"{Constants.IndexViewCachePrefix}-ALL";
            var indexView = await _asyncCacheService.GetByKeyAsync<IndexView>(setViewCacheKey);
            if (indexView == null)
            {
                indexView = new IndexView();
            }

            var chartPrices = await _chartPriceData.CacheFindAllAsync("VNINDEX", "D");
            if (chartPrices is not null)
            {
                chartPrices = chartPrices.OrderBy(q => q.TradingDate).ToList();
                var quotes = chartPrices.Select(q => q.ToQuote()).OrderBy(q => q.Date).ToList();
                var zigZagResults = quotes.GetZigZag(EndType.HighLow, 5);
                var zigZagResultH = zigZagResults.LastOrDefault(q => q.PointType == "H" && ((decimal)chartPrices[^1].ClosePrice < (q.ZigZag - (q.ZigZag * 0.01m))));
                var zigZagResultL = zigZagResults.LastOrDefault(q => q.PointType == "L" && ((decimal)chartPrices[^1].ClosePrice > (q.ZigZag + (q.ZigZag * 0.01m))));
                if (zigZagResultH is not null && zigZagResultH.ZigZag.HasValue)
                {
                    indexView.KcVnindex = zigZagResultH.ZigZag.Value.ToString("00.00");
                }

                if (zigZagResultL is not null && zigZagResultL.ZigZag.HasValue)
                {
                    indexView.HtVnindex = zigZagResultL.ZigZag.Value.ToString("00.00");
                }
            }

            var vnindexValuation = indexValuation["VNINDEX"].OrderByDescending(q => q.TradingDate).FirstOrDefault();
            if (vnindexValuation is not null)
            {
                indexView.VnindexPe = $"{vnindexValuation.R21:0,0.00}";
                indexView.VnindexPb = $"{vnindexValuation.R25:0,0.00}";
            }

            var setCacheTask = _asyncCacheService.SetValueAsync(setViewCacheKey, indexView, Constants.DefaultCacheTime * 60 * 24);
            var sendMessage = new QueueMessage("UpdateIndexView");
            sendMessage.KeyValues.Add("Data", JsonSerializer.Serialize(indexView));
            _workerQueueService.BroadcastViewUpdatedTask(sendMessage);
            await setCacheTask;
        }
    }
}
