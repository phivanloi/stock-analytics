using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
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
        private readonly IZipHelper _zipHelper;
        private readonly IWorkerQueueService _workerQueueService;
        private readonly IScheduleData _scheduleData;
        private readonly ICompanyData _companyData;
        private readonly IStockPriceData _stockPriceData;
        private readonly IKeyValueData _keyValueData;
        private readonly IFinancialIndicatorData _financialIndicatorData;
        private readonly IFiinEvaluatedData _fiinEvaluatedData;
        private readonly IVndStockScoreData _vndStockScoreData;
        private readonly IFinancialGrowthData _financialGrowthData;
        private readonly IAnalyticsResultData _analyticsResultData;
        private readonly IStockData _stockData;
        private readonly IStockRecommendationData _stockRecommendationData;
        private readonly IChartPriceData _chartPriceData;
        private readonly ITradingResultData _tradingResultData;
        private readonly IIndustryData _industryData;
        private readonly ICorporateActionData _corporateActionData;

        public RealtimeService(
            ICorporateActionData corporateActionData,
            IIndustryData industryData,
            ITradingResultData tradingResultData,
            IChartPriceData chartPriceData,
            IStockRecommendationData stockRecommendationData,
            IStockData stockData,
            IAnalyticsResultData analyticsResultData,
            IFinancialGrowthData financialGrowthData,
            IVndStockScoreData vndStockScoreData,
            IFiinEvaluatedData fiinEvaluatedData,
            IFinancialIndicatorData financialIndicatorData,
            IKeyValueData keyValueData,
            IStockPriceData stockPriceData,
            ICompanyData companyData,
            IScheduleData scheduleData,
            IWorkerQueueService workerQueueService,
            IZipHelper zipHelper,
            ILogger<RealtimeService> logger)
        {
            _corporateActionData = corporateActionData;
            _industryData = industryData;
            _tradingResultData = tradingResultData;
            _chartPriceData = chartPriceData;
            _stockRecommendationData = stockRecommendationData;
            _stockData = stockData;
            _analyticsResultData = analyticsResultData;
            _financialGrowthData = financialGrowthData;
            _vndStockScoreData = vndStockScoreData;
            _fiinEvaluatedData = fiinEvaluatedData;
            _financialIndicatorData = financialIndicatorData;
            _keyValueData = keyValueData;
            _stockPriceData = stockPriceData;
            _companyData = companyData;
            _scheduleData = scheduleData;
            _logger = logger;
            _zipHelper = zipHelper;
            _workerQueueService = workerQueueService;
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
                    await TestTradingOnPriceChange(queueMessage.KeyValues["Symbol"], queueMessage.KeyValues["ChartPrices"]);
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
        public virtual async Task TestTradingOnPriceChange(string symbol, string chartPricesJsonString)
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
                    type14.AddOrUpdateOptions("SleepTime", "15");
                }
                else if (transactionCont > 50000)
                {
                    type14.AddOrUpdateOptions("SleepTime", "30");
                }
                else if (transactionCont > 10000)
                {
                    type14.AddOrUpdateOptions("SleepTime", "60");
                }
                else if (transactionCont > 5000)
                {
                    type14.AddOrUpdateOptions("SleepTime", "120");
                }
                else if (transactionCont > 1000)
                {
                    type14.AddOrUpdateOptions("SleepTime", "300");
                }
                else if (transactionCont > 500)
                {
                    type14.AddOrUpdateOptions("SleepTime", "600");
                }
                else if (transactionCont > 100)
                {
                    type14.AddOrUpdateOptions("SleepTime", "1200");
                }
                else
                {
                    type14.AddOrUpdateOptions("SleepTime", "2400");
                }
                await _scheduleData.UpdateAsync(type14);
            }
            return false;
        }
    }
}
