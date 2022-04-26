using Microsoft.Extensions.Logging;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using System.Diagnostics;

namespace Pl.Sas.Core.Services
{
    /// <summary>
    /// các nghiệp vụ được xử lý bởi worker
    /// </summary>
    public class WorkerService
    {
        private readonly IMarketData _marketData;
        private readonly ICrawlData _crawlData;
        private readonly ILogger<WorkerService> _logger;

        public WorkerService(
            ILogger<WorkerService> logger,
            ICrawlData crawlData,
            IMarketData marketData)
        {
            _marketData = marketData;
            _crawlData = crawlData;
            _logger = logger;
        }

        public async Task<QueueMessage?> HandleEventAsync(QueueMessage queueMessage)
        {
            var schedule = await _marketData.GetScheduleByIdAsync(queueMessage.Id);
            if (schedule != null)
            {
                Stopwatch stopWatch = new();
                stopWatch.Start();

                switch (schedule.Type)
                {
                    case 0:
                        return await InitialStockAsync();

                    case 1:
                        return await StockDownloadAndAnalyticsAsync(schedule);

                    default:
                        _logger.LogWarning("Worker process schedule id {Id}, type {Type} don't match any function", schedule.Id, schedule.Type);
                        break;
                }

                stopWatch.Stop();
                _logger.LogInformation("Worker process schedule {Id} type {Type} in {ElapsedMilliseconds} miniseconds.", schedule.Id, schedule.Type, stopWatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogWarning("Worker HandleEventAsync null scheduler Id: {Id}.", queueMessage.Id);
            }
            return null;
        }

        /// <summary>
        /// Xử lý thêm mới hoặc sửa thông tin chứng khoán hiện có
        /// </summary>
        /// <returns>Một message yêu cầu update memory</returns>
        /// <exception cref="Exception">download initial market stock error</exception>
        public virtual async Task<QueueMessage?> InitialStockAsync()
        {
            var ssiAllStock = await _crawlData.DownloadInitialMarketStockAsync() ?? throw new Exception("Can't download initial market stock.");
            var allStocks = await _marketData.GetStockDictionaryAsync();

            var insertSchedules = new List<Schedule>();
            var updateStocks = new List<Stock>();
            var insertStocks = new List<Stock>();
            foreach (var datum in ssiAllStock.Data)
            {
                if (string.IsNullOrEmpty(datum.Type) || datum.Type != "s" || datum.Code.Length > 3)
                {
                    _logger.LogInformation("Initial stock {Code} is ignore.", datum.Code);
                    continue;
                }
                var stockCode = datum.Code.ToUpper();
                var dbStock = allStocks.GetValueOrDefault(stockCode);
                if (dbStock != null)
                {
                    dbStock.Name = datum.Name;
                    dbStock.FullName = datum.FullName;
                    dbStock.Exchange = datum.Exchange;
                    updateStocks.Add(dbStock);
                }
                else
                {
                    var random = new Random();
                    var currentTime = DateTime.Now;
                    insertStocks.Add(new()
                    {
                        Name = datum.Name,
                        FullName = datum.FullName,
                        Exchange = datum.Exchange,
                        Type = datum.Type,
                        Symbol = stockCode
                    });
                    insertSchedules.Add(new()
                    {
                        Name = $"Tải và phân tích mã: {stockCode}",
                        Type = 1,
                        DataKey = stockCode,
                        ActiveTime = currentTime.AddMinutes(random.Next(0, 60))
                    });
                }
            }

            await _marketData.InitialStockAsync(insertStocks, updateStocks);
            await _marketData.InsertScheduleAsync(insertSchedules);

            if (updateStocks.Count > 0)
            {
                var queueMessage = new QueueMessage("UpdatedStocks");
                for (int i = 0; i < updateStocks.Count; i++)
                {
                    queueMessage.KeyValues.Add("Symbol" + i, updateStocks[i].Symbol);
                }
                return queueMessage;
            }
            return null;
        }

        /// <summary>
        /// Tải thông tin và xử lý phân tích chứng khoán
        /// </summary>
        /// <param name="schedule">Thông tin lịch</param>
        /// <returns>Một message yêu cầu update memory</returns>
        public virtual async Task<QueueMessage?> StockDownloadAndAnalyticsAsync(Schedule schedule)
        {
            var ssiAllStock = await _crawlData.DownloadInitialMarketStockAsync() ?? throw new Exception("Can't download initial market stock.");
            var allStocks = await _marketData.GetStockDictionaryAsync();

            var insertSchedules = new List<Schedule>();
            var updateStocks = new List<Stock>();
            var insertStocks = new List<Stock>();
            foreach (var datum in ssiAllStock.Data)
            {
                if (string.IsNullOrEmpty(datum.Type) || datum.Type != "s" || datum.Code.Length > 3)
                {
                    _logger.LogInformation("Initial stock {Code} is ignore.", datum.Code);
                    continue;
                }
                var stockCode = datum.Code.ToUpper();
                var dbStock = allStocks.GetValueOrDefault(stockCode);
                if (dbStock != null)
                {
                    dbStock.Name = datum.Name;
                    dbStock.FullName = datum.FullName;
                    dbStock.Exchange = datum.Exchange;
                    updateStocks.Add(dbStock);
                }
                else
                {
                    var random = new Random();
                    var currentTime = DateTime.Now;
                    insertStocks.Add(new()
                    {
                        Name = datum.Name,
                        FullName = datum.FullName,
                        Exchange = datum.Exchange,
                        Type = datum.Type,
                        Symbol = stockCode
                    });
                    insertSchedules.Add(new()
                    {
                        Name = $"Tải và phân tích mã: {stockCode}",
                        Type = 1,
                        DataKey = stockCode,
                        ActiveTime = currentTime.AddMinutes(random.Next(0, 60))
                    });
                }
            }

            await _marketData.InitialStockAsync(insertStocks, updateStocks);
            await _marketData.InsertScheduleAsync(insertSchedules);

            if (updateStocks.Count > 0)
            {
                var queueMessage = new QueueMessage("UpdatedStocks");
                for (int i = 0; i < updateStocks.Count; i++)
                {
                    queueMessage.KeyValues.Add("Symbol" + i, updateStocks[i].Symbol);
                }
                return queueMessage;
            }
            return null;
        }
    }
}
