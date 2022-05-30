using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Core.Trading;
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

        public RealtimeService(
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

            var chartPrices = await _chartPriceData.CacheFindAllAsync(symbol, "D");
            if (chartPrices is null || chartPrices.Count <= 0)
            {
                return;
            }

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
            var tradingHistories = chartPrices.Where(q => q.TradingDate >= Constants.StartTime).OrderBy(q => q.TradingDate).ToList();

            var stockView = await stockViewTask;
            if (stockView is null)
            {
                return;
            }

            var listTradingResult = new List<TradingResult>();
            #region Experiment Trading
            ExperimentTrading.LoadIndicatorSet(chartPrices);
            var experCase = ExperimentTrading.Trading(tradingHistories, false);
            listTradingResult.Add(new()
            {
                Symbol = symbol,
                Principle = 0,
                IsBuy = experCase.IsBuy,
                IsSell = experCase.IsSell,
                BuyPrice = experCase.BuyPrice,
                SellPrice = experCase.SellPrice,
                FixedCapital = experCase.FixedCapital,
                Profit = experCase.Profit(tradingHistories[^1].ClosePrice),
                TotalTax = experCase.TotalTax,
                TradingNotes = null,
                AssetPosition = experCase.AssetPosition,
                LoseNumber = experCase.LoseNumber,
                WinNumber = experCase.WinNumber,
            });
            experCase = null;
            ExperimentTrading.Dispose();
            #endregion

            #region Main Trading
            MacdTrading.LoadIndicatorSet(chartPrices);
            var macdCase = MacdTrading.Trading(tradingHistories, false);
            listTradingResult.Add(new()
            {
                Symbol = symbol,
                Principle = 1,
                IsBuy = macdCase.IsBuy,
                IsSell = macdCase.IsSell,
                BuyPrice = macdCase.BuyPrice,
                SellPrice = macdCase.SellPrice,
                FixedCapital = macdCase.FixedCapital,
                Profit = macdCase.Profit(tradingHistories[^1].ClosePrice),
                TotalTax = macdCase.TotalTax,
                TradingNotes = null,
                AssetPosition = macdCase.AssetPosition,
                LoseNumber = macdCase.LoseNumber,
                WinNumber = macdCase.WinNumber,
            });
            macdCase = null;
            MacdTrading.Dispose();
            #endregion

            #region Buy and wait
            var startPrice = tradingHistories[0].ClosePrice;
            var endPrice = tradingHistories[^1].ClosePrice;
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
                AssetPosition = "100% cổ phiếu",
                LoseNumber = startPrice <= endPrice ? 1 : 0,
                WinNumber = startPrice > endPrice ? 1 : 0,
            });
            #endregion

            stockView.LastClosePrice = chartPrices[^1].OpenPrice;
            stockView.LastOpenPrice = chartPrices[^1].OpenPrice;
            stockView.LastHighestPrice = chartPrices[^1].HighestPrice;
            stockView.LastLowestPrice = chartPrices[^1].LowestPrice;
            stockView.LastTotalMatchVol = chartPrices[^1].TotalMatchVol;
            StockViewService.BindingTradingResultToView(ref stockView, listTradingResult);
            StockViewService.BindingPercentConvulsionToView(ref stockView, chartPrices);

            var sendMessage = new QueueMessage("UpdateRealtimeView");
            sendMessage.KeyValues.Add("Data", JsonSerializer.Serialize(stockView));
            sendMessage.KeyValues.Add("Symbol", symbol);
            _workerQueueService.BroadcastViewUpdatedTask(sendMessage);
            listTradingResult = null;
            chartPrices = null;
            tradingHistories = null;
            stockView = null;
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
                if (transactionCont > 100000)
                {
                    type14.AddOrUpdateOptions("SleepTime", "10");
                }
                else if (transactionCont > 50000)
                {
                    type14.AddOrUpdateOptions("SleepTime", "15");
                }
                else if (transactionCont > 10000)
                {
                    type14.AddOrUpdateOptions("SleepTime", "30");
                }
                else if (transactionCont > 5000)
                {
                    type14.AddOrUpdateOptions("SleepTime", "60");
                }
                else if (transactionCont > 1000)
                {
                    type14.AddOrUpdateOptions("SleepTime", "150");
                }
                else if (transactionCont > 500)
                {
                    type14.AddOrUpdateOptions("SleepTime", "300");
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
    }
}
