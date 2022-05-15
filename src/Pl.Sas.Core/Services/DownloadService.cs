﻿using Microsoft.Extensions.Logging;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Web;

namespace Pl.Sas.Core.Services
{
    /// <summary>
    /// Các nghiệp vụ download của hệ thống
    /// </summary>
    public class DownloadService
    {
        private readonly IWorkerQueueService _workerQueueService;
        private readonly IMarketData _marketData;
        private readonly ISystemData _systemData;
        private readonly IDownloadData _crawlData;
        private readonly IZipHelper _zipHelper;
        private readonly ILogger<DownloadService> _logger;
        private readonly IMemoryCacheService _memoryCacheService;
        private readonly DateTimeOffset _indexStartDownloadTime = new(2000, 1, 1, 0, 0, 0, TimeSpan.FromMilliseconds(0));

        public DownloadService(
            IMemoryCacheService memoryCacheService,
            IWorkerQueueService workerQueueService,
            IZipHelper zipHelper,
            ISystemData systemData,
            ILogger<DownloadService> logger,
            IDownloadData crawlData,
            IMarketData marketData)
        {
            _marketData = marketData;
            _crawlData = crawlData;
            _logger = logger;
            _workerQueueService = workerQueueService;
            _zipHelper = zipHelper;
            _systemData = systemData;
            _memoryCacheService = memoryCacheService;
        }

        public async Task HandleEventAsync(QueueMessage queueMessage)
        {
            var schedule = await _systemData.GetScheduleByIdAsync(queueMessage.Id);
            if (schedule != null)
            {
                try
                {
                    Stopwatch stopWatch = new();
                    stopWatch.Start();
                    switch (schedule.Type)
                    {
                        case 0:
                            await InitialStockAsync();
                            break;
                        case 1:
                            await UpdateCompanyInfoAsync(schedule);
                            break;
                        case 2:
                            await UpdateLeadershipInfoAsync(schedule);
                            break;
                        case 3:
                            await UpdateCapitalAndDividendAsync(schedule);
                            break;
                        case 4:
                            await UpdateFinancialIndicatorAsync(schedule);
                            break;
                        case 5:
                            await UpdateStockPriceHistoryAsync(schedule);
                            break;
                        case 6:
                            await UpdateCorporateActionInfoAsync(schedule);
                            break;
                        case 7:
                            await UpdateStockTransactionAsync(schedule);
                            break;
                        case 8:
                            await UpdateFiinStockEvaluatesAsync(schedule);
                            break;
                        case 9:
                            await UpdateChartPricesAsync(schedule);
                            break;
                        case 10:
                            await UpdateBankInterestRateAsync(schedule);
                            break;
                        case 11:
                            await UpdateStockRecommendationsAsync(schedule);
                            break;
                        case 12:
                            await UpdateVndStockScoreAsync(schedule);
                            break;
                        default:
                            _logger.LogWarning("Worker process schedule id {Id}, type {Type} don't match any function", schedule.Id, schedule.Type);
                            break;
                    }
                    schedule.IsError = false;
                    stopWatch.Stop();
                    _logger.LogInformation("Worker process schedule {Id} type {Type} in {ElapsedMilliseconds} miniseconds.", schedule.Id, schedule.Type, stopWatch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    schedule.IsError = true;
                    _logger.LogError(ex, "Download process schedule id {Id}, type {Type} is error.", schedule.Id, schedule.Type);
                }
                finally
                {
                    await _systemData.UpdateScheduleAsync(schedule);
                }
            }
            else
            {
                _logger.LogError("Worker HandleEventAsync null scheduler Id: {Id}.", queueMessage.Id);
            }
        }

        /// <summary>
        /// download dữ liệu lãi suất ngân hàng
        /// </summary>
        /// <param name="schedule">thông tin lịch</param>
        /// <returns>bool</returns>
        public virtual async Task<bool> UpdateBankInterestRateAsync(Schedule schedule)
        {
            var checkingKey = $"Download-BankInterestRate";
            var bankInterestRates = await _crawlData.DownloadBankInterestRateAsync(schedule.Options["Length"]);
            if (bankInterestRates is null || bankInterestRates.Count < 0)
            {
                _logger.LogWarning("UpdateBankInterestRateAsync => bankInterestRates is null");
                return await _systemData.SetKeyValueAsync(checkingKey, false);
            }

            foreach (var rate in bankInterestRates)
            {
                if (rate?.Data?.Length > 0)
                {
                    var maxItem = rate.Data.OrderByDescending(q => q.InterestRate).FirstOrDefault() ?? throw new Exception("rate.Data is null.");
                    await _systemData.SetKeyValueAsync($"BankInterestRate{(int)(maxItem.Term ?? 3)}", maxItem.InterestRate);
                }
            }
            return await _systemData.SetKeyValueAsync(checkingKey, true);
        }

        /// <summary>
        /// download dữ liệu index
        /// </summary>
        /// <param name="schedule">thông tin lịch</param>
        /// <returns>bool</returns>
        /// <exception cref="Exception">Schedule Null DataKey</exception>
        public virtual async Task<bool> UpdateChartPricesAsync(Schedule schedule)
        {
            var symbol = schedule.DataKey ?? throw new Exception($"Schedule Null DataKey, di: {schedule.Id}, type: {schedule.Type}");
            var checkingKey = $"{symbol}-Download-ChartPrices";
            var configTime = long.Parse(schedule.Options["StartTime"]);
            var ssiChartPrices = await _crawlData.DownloadIndexPricesAsync(symbol, configTime, schedule.Options["ChartType"]);
            if (ssiChartPrices is null || ssiChartPrices.Count < 0)
            {
                _logger.LogWarning("UpdateChartPricesAsync => indexPrices null info for index: {symbol}", symbol);
                return await _systemData.SetKeyValueAsync(checkingKey, false);
            }

            var chartPrices = new List<ChartPrice>();
            foreach (var block in ssiChartPrices)
            {
                if (block?.Time?.Length > 0)
                {
                    for (int i = 0; i < block.Time.Length; i++)
                    {
                        var tradingDate = DateTimeOffset.FromUnixTimeSeconds(block.Time[i]).Date;
                        if (!chartPrices.Any(q => q.TradingDate == tradingDate))
                        {
                            chartPrices.Add(new()
                            {
                                Symbol = schedule.DataKey,
                                TradingDate = tradingDate,
                                ClosePrice = string.IsNullOrEmpty(block.Close[i]) ? 0 : float.Parse(block.Close[i]),
                                OpenPrice = string.IsNullOrEmpty(block.Open[i]) ? 0 : float.Parse(block.Open[i]),
                                HighestPrice = string.IsNullOrEmpty(block.Highest[i]) ? 0 : float.Parse(block.Highest[i]),
                                LowestPrice = string.IsNullOrEmpty(block.Lowest[i]) ? 0 : float.Parse(block.Lowest[i]),
                                TotalMatchVol = string.IsNullOrEmpty(block.Volumes[i]) ? 0 : float.Parse(block.Volumes[i]),
                                Type = "D"
                            });
                        }
                    }
                }
            }

            if (chartPrices.Count <= 0)
            {
                _logger.LogWarning("UpdateChartPricesAsync => indexPrices null info for index: {symbol}", symbol);
                return await _systemData.SetKeyValueAsync(checkingKey, false);
            }

            var check = await _marketData.SaveChartPriceAsync(chartPrices, symbol, schedule.Options["ChartType"]);
            return await _systemData.SetKeyValueAsync(checkingKey, check);
        }

        /// <summary>
        /// Tải và xử lý dữ liệu đánh giá cổ phiếu của fiin
        /// </summary>
        /// <param name="schedule">Thông tin lịch</param>
        /// <returns></returns>
        /// <exception cref="Exception">Can't fiinStock evaluate info.</exception>
        public virtual async Task<bool> UpdateFiinStockEvaluatesAsync(Schedule schedule)
        {
            var symbol = schedule.DataKey ?? throw new Exception($"Schedule Null DataKey, di: {schedule.Id}, type: {schedule.Type}");
            var checkingKey = $"{symbol}-Download-FiinStockEvaluates";
            var fiinStockEvaluate = await _crawlData.DownloadFiinStockEvaluatesAsync(symbol);
            if (fiinStockEvaluate is null || fiinStockEvaluate.Items[0] is null)
            {
                _logger.LogWarning("UpdateFiinStockEvaluatesAsync => fiinStockEvaluate null info for code: {symbol}", symbol);
                return await _systemData.SetKeyValueAsync(checkingKey, false);
            }

            var fiinEvaluate = new FiinEvaluated()
            {
                Symbol = symbol,
                IcbRank = fiinStockEvaluate.Items[0].IcbRank ?? -1,
                IcbTotalRanked = fiinStockEvaluate.Items[0].IcbTotalRanked ?? -1,
                IndexRank = fiinStockEvaluate.Items[0].IndexRank ?? -1,
                IndexTotalRanked = fiinStockEvaluate.Items[0].IndexTotalRanked ?? -1,
                IcbCode = fiinStockEvaluate.Items[0].IcbCode,
                ComGroupCode = fiinStockEvaluate.Items[0].ComGroupCode,
                Growth = fiinStockEvaluate.Items[0].Growth,
                Value = fiinStockEvaluate.Items[0].Value,
                Momentum = fiinStockEvaluate.Items[0].Momentum,
                Vgm = fiinStockEvaluate.Items[0].Vgm,
                ControlStatusCode = fiinStockEvaluate.Items[0].ControlStatusCode ?? -1,
                ControlStatusName = fiinStockEvaluate.Items[0].ControlStatusName
            };
            var check = await _marketData.SaveFiinEvaluateAsync(fiinEvaluate);
            return await _systemData.SetKeyValueAsync(checkingKey, check);
        }

        /// <summary>
        /// Tải và xử lý đánh giá của các công ty chứng khoán
        /// </summary>
        /// <param name="schedule">Thông tin lịch</param>
        /// <returns>bool</returns>
        /// <exception cref="Exception">Can't stock recommendation info</exception>
        public virtual async Task<bool> UpdateStockRecommendationsAsync(Schedule schedule)
        {
            var symbol = schedule.DataKey ?? throw new Exception($"Schedule Null DataKey, di: {schedule.Id}, type: {schedule.Type}");
            var checkingKey = $"{symbol}-Download-StockRecommendations";
            var recommendations = await _crawlData.DownloadStockRecommendationsAsync(symbol);
            if (recommendations is null || recommendations.Data.Length < 0)
            {
                _logger.LogWarning("UpdateStockRecommendationsAsync => recommendations null info for code: {symbol}", symbol);
                return await _systemData.SetKeyValueAsync(checkingKey, false);
            }

            var stockRecommendations = new List<StockRecommendation>();
            foreach (var item in recommendations.Data)
            {
                if (string.IsNullOrEmpty(item.ReportDate) || string.IsNullOrEmpty(item.Analyst))
                {
                    continue;
                }
                var reportDate = DateTime.ParseExact(item.ReportDate, "yyyy-MM-dd", null);
                stockRecommendations.Add(new StockRecommendation()
                {
                    Symbol = symbol,
                    Firm = item.Firm,
                    ReportDate = reportDate,
                    Analyst = item.Analyst,
                    AvgTargetPrice = item.AvgTargetPrice,
                    ReportPrice = item.ReportPrice,
                    Source = item.Source,
                    TargetPrice = item.TargetPrice,
                    Type = item.Type
                });
            }
            var check = await _marketData.SaveStockRecommendationAsync(stockRecommendations);
            return await _systemData.SetKeyValueAsync(checkingKey, check);
        }

        /// <summary>
        /// Bổ sung dữ liệu đánh giá của fiin trading
        /// </summary>
        /// <param name="schedule">Thông tin lịch</param>
        /// <returns>bool</returns>
        /// <exception cref="Exception">Schedule Null DataKey</exception>
        public virtual async Task<bool> UpdateVndStockScoreAsync(Schedule schedule)
        {
            var symbol = schedule.DataKey ?? throw new Exception($"Schedule Null DataKey {schedule.Id}");
            var checkingKey = $"{symbol}-Download-VndStockScore";
            var vndStockScoreResponse = await _crawlData.DownloadVndStockScoringsAsync(symbol);
            if (vndStockScoreResponse is null || vndStockScoreResponse.Data.Length < 0)
            {
                _logger.LogWarning("UpdateVndStockScoreAsync => vndStockScoreResponse null info for code: {symbol}", symbol);
                return await _systemData.SetKeyValueAsync(checkingKey, false);
            }

            var vndStockScores = new List<VndStockScore>();
            foreach (var item in vndStockScoreResponse.Data)
            {
                var fiscalDate = DateTime.ParseExact(item.FiscalDate, "yyyy-MM-dd", null);
                vndStockScores.Add(new VndStockScore()
                {
                    Symbol = symbol,
                    Type = item.Type,
                    FiscalDate = fiscalDate,
                    ModelCode = item.ModelCode,
                    CriteriaCode = item.CriteriaCode,
                    CriteriaType = item.CriteriaType,
                    CriteriaName = item.CriteriaName,
                    Point = item.Point,
                    Locale = item.Locale
                });
            }
            var check = await _marketData.SaveVndStockScoresAsync(vndStockScores);
            return await _systemData.SetKeyValueAsync(checkingKey, check);
        }

        /// <summary>
        /// Bổ sung lịch sử khớp lệnh của cổ phiếu
        /// </summary>
        /// <param name="schedule">Thông tin lịch</param>
        /// <returns></returns>
        public virtual async Task<bool> UpdateStockTransactionAsync(Schedule schedule)
        {
            var symbol = schedule.DataKey ?? throw new Exception($"Schedule Null DataKey, di: {schedule.Id}, type: {schedule.Type}");
            var checkingKey = $"{symbol}-Download-StockTransaction";
            var ssiTransactions = await _crawlData.DownloadTransactionAsync(schedule.Options["SsiStockNo"]);
            if (ssiTransactions is null || ssiTransactions.Data.LeTables.Length < 0)
            {
                _logger.LogWarning("UpdateStockTransactionAsync => ssiTransactions null info for code: {symbol}", symbol);
                return await _systemData.SetKeyValueAsync(checkingKey, false);
            }

            var listTransactionDetails = new List<StockTransactionDetails>();
            foreach (var letableTransaction in ssiTransactions.Data.LeTables)
            {
                var addItem = new StockTransactionDetails()
                {
                    Vol = letableTransaction.Vol * 10,
                    AccumulatedVol = letableTransaction.AccumulatedVol,
                    Price = letableTransaction.Price,
                    RefPrice = letableTransaction.RefPrice,
                    Time = letableTransaction.Time
                };
                listTransactionDetails.Add(addItem);
            }

            StockTransaction stockTransaction = new()
            {
                Symbol = schedule.DataKey,
                TradingDate = Utilities.GetTradingDate(),
                ZipDetails = _zipHelper.ZipByte(JsonSerializer.SerializeToUtf8Bytes(listTransactionDetails))
            };
            var check = await _marketData.SaveStockTransactionAsync(stockTransaction);
            return await _systemData.SetKeyValueAsync(checkingKey, check);
        }

        /// <summary>
        /// Xử lý thông tin hoạt động của doanh nghiệp
        /// </summary>
        /// <param name="schedule">Thông tin lịch</param>
        /// <returns>bool</returns>
        /// <exception cref="Exception">Can't corporate action info.</exception>
        public virtual async Task<bool> UpdateCorporateActionInfoAsync(Schedule schedule)
        {
            var symbol = schedule.DataKey ?? throw new Exception($"Schedule Null DataKey, di: {schedule.Id}, type: {schedule.Type}");
            var checkingKey = $"{symbol}-Download-CorporateAction";
            var size = int.Parse(schedule.Options["CorporateActionCrawlSize"]);
            var ssiCorporateAction = await _crawlData.DownloadCorporateActionAsync(symbol, size);
            if (ssiCorporateAction is null || ssiCorporateAction.Data.CorporateActions.DataList.Length < 0)
            {
                _logger.LogWarning("UpdateCorporateActionInfoAsync => ssiCorporateAction null info for code: {symbol}", symbol);
                return await _systemData.SetKeyValueAsync(checkingKey, false);
            }

            var insertList = new List<CorporateAction>();
            var corporateActions = await _marketData.GetCorporateActionsForCheckDownloadAsync(symbol);
            foreach (var item in ssiCorporateAction.Data.CorporateActions.DataList)
            {
                if (string.IsNullOrEmpty(item.ExrightDate))
                {
                    continue;
                }
                var newLeadership = new CorporateAction()
                {
                    Symbol = symbol,
                    EventName = Utilities.TruncateString(item.EventName, 256),
                    ExrightDate = ParseDateType(item.ExrightDate) ?? DateTime.Now,
                    RecordDate = ParseDateType(item.RecordDate) ?? DateTime.Now,
                    IssueDate = ParseDateType(item.IssueDate) ?? DateTime.Now,
                    EventTitle = Utilities.TruncateString(item.EventTitle, 256),
                    PublicDate = ParseDateType(item.PublicDate) ?? DateTime.Now,
                    Exchange = item.Exchange,
                    EventListCode = Utilities.TruncateString(item.EventListCode, 128),
                    Value = float.Parse(item.Value),
                    Ratio = float.Parse(item.Ratio),
                    Description = _zipHelper.ZipByte(Encoding.UTF8.GetBytes(item.EventDescription)),
                    EventCode = item.EventCode
                };
                if (!corporateActions.Any(q => q.Symbol == newLeadership.Symbol
                && q.ExrightDate == newLeadership.ExrightDate
                && q.EventCode == newLeadership.EventCode))
                {
                    insertList.Add(newLeadership);
                }
            }

            if (size > 10)
            {
                schedule.AddOrUpdateOptions("CorporateActionCrawlSize", "10");
                await _systemData.UpdateScheduleAsync(schedule);
            }

            await _systemData.SetKeyValueAsync(checkingKey, true);
            return await _marketData.InsertCorporateActionAsync(insertList);
        }

        /// <summary>
        /// Xử ý sử giá cổ phiếu
        /// </summary>
        /// <param name="schedule">Thông tin lịch</param>
        /// <returns>bool</returns>
        /// <exception cref="Exception">Schedule Null DataKey.</exception>
        public virtual async Task<bool> UpdateStockPriceHistoryAsync(Schedule schedule)
        {
            var symbol = schedule.DataKey ?? throw new Exception($"Schedule Null DataKey, di: {schedule.Id}, type: {schedule.Type}");
            var checkingKey = $"{symbol}-Download-CorporateAction";
            var size = int.Parse(schedule.Options["StockPricesCrawlSize"]);
            var stockPriceHistory = await _crawlData.DownloadStockPricesAsync(symbol, size);
            if (stockPriceHistory is null || stockPriceHistory.Data.StockPrice.DataList.Length <= 0)
            {
                _logger.LogWarning("UpdateStockPriceHistoryAsync => stockPriceHistory null info for code: {symbol}", symbol);
                return await _systemData.SetKeyValueAsync(checkingKey, false);
            }

            var insertList = new List<StockPrice>();
            var updateList = new List<StockPrice>();
            if (size <= 10)
            {
                foreach (var stockPriceSsi in stockPriceHistory.Data.StockPrice.DataList)
                {
                    var tradingDate = ParseDateType(stockPriceSsi.TradingDate);
                    if (tradingDate is null)
                    {
                        continue;
                    }

                    var updateItem = await _marketData.GetStockPriceAsync(symbol, tradingDate.Value.Date);
                    if (updateItem != null)
                    {
                        StockPriceBindValue(ref updateItem, stockPriceSsi);
                        updateList.Add(updateItem);
                    }
                    else
                    {
                        updateItem = new StockPrice() { Symbol = symbol, TradingDate = tradingDate.Value };
                        StockPriceBindValue(ref updateItem, stockPriceSsi);
                        insertList.Add(updateItem);
                    }
                }
            }
            else
            {
                foreach (var stockPrice in stockPriceHistory.Data.StockPrice.DataList)
                {
                    var tradingDate = ParseDateType(stockPrice.TradingDate);
                    if (tradingDate is null)
                    {
                        continue;
                    }

                    var addItem = new StockPrice() { Symbol = symbol, TradingDate = tradingDate.Value };
                    StockPriceBindValue(ref addItem, stockPrice);
                    insertList.Add(addItem);
                }
                schedule.AddOrUpdateOptions("StockPricesCrawlSize", "10");
                await _systemData.UpdateScheduleAsync(schedule);
            }

            var check = await _marketData.SaveStockPriceAsync(insertList, updateList);
            if (check)
            {
                _memoryCacheService.RemoveByPrefix(Constants.StockPriceCachePrefix);
                _workerQueueService.BroadcastUpdateMemoryTask(new("StockPrice"));
            }
            return await _systemData.SetKeyValueAsync(checkingKey, check);
        }

        /// <summary>
        /// Xử lý thông tin tài chính của doanh nghiệp
        /// </summary>
        /// <param name="schedule">Thông tin lịch</param>
        /// <returns>bool</returns>
        /// <exception cref="Exception">Schedule Null DataKey.</exception>
        public virtual async Task<bool> UpdateFinancialIndicatorAsync(Schedule schedule)
        {
            var symbol = schedule.DataKey ?? throw new Exception($"Schedule Null DataKey, di: {schedule.Id}, type: {schedule.Type}");
            var checkingKey = $"{symbol}-Download-FinancialIndicator";
            var ssiFinancialIndicators = await _crawlData.DownloadFinancialIndicatorAsync(symbol);
            if (ssiFinancialIndicators is null || ssiFinancialIndicators.Data.FinancialIndicator.DataList.Length < 0)
            {
                _logger.LogWarning("UpdateFinancialIndicatorAsync => ssiFinancialIndicators null info for code: {Symbol}", symbol);
                return await _systemData.SetKeyValueAsync(checkingKey, false);
            }

            var insertList = new List<FinancialIndicator>();
            var updateList = new List<FinancialIndicator>();
            var listDbCheck = await _marketData.GetFinancialIndicatorsAsync(symbol);
            foreach (var ssiFinancialIndicator in ssiFinancialIndicators.Data.FinancialIndicator.DataList)
            {
                var reportYear = int.Parse(ssiFinancialIndicator.YearReport);
                var lengthReport = int.Parse(ssiFinancialIndicator.LengthReport);
                var updateItem = listDbCheck.FirstOrDefault(q => q.YearReport == reportYear && q.LengthReport == lengthReport);
                if (updateItem is not null)
                {
                    updateItem.YearReport = reportYear;
                    updateItem.Symbol = symbol;
                    updateItem.LengthReport = lengthReport;
                    updateItem.Revenue = float.Parse(ssiFinancialIndicator.Revenue);
                    updateItem.Profit = float.Parse(ssiFinancialIndicator.Profit);
                    updateItem.Eps = float.Parse(ssiFinancialIndicator.Eps);
                    updateItem.DilutedEps = float.Parse(ssiFinancialIndicator.DilutedEps);
                    updateItem.Pe = float.Parse(ssiFinancialIndicator.Pe);
                    updateItem.DilutedPe = float.Parse(ssiFinancialIndicator.DilutedPe);
                    updateItem.Roe = float.Parse(ssiFinancialIndicator.Roe);
                    updateItem.Roa = float.Parse(ssiFinancialIndicator.Roa);
                    updateItem.Roic = float.Parse(ssiFinancialIndicator.Roic);
                    updateItem.GrossProfitMargin = float.Parse(ssiFinancialIndicator.GrossProfitMargin);
                    updateItem.NetProfitMargin = float.Parse(ssiFinancialIndicator.NetProfitMargin);
                    updateItem.DebtAsset = float.Parse(ssiFinancialIndicator.DebtAsset);
                    updateItem.QuickRatio = float.Parse(ssiFinancialIndicator.QuickRatio);
                    updateItem.CurrentRatio = float.Parse(ssiFinancialIndicator.CurrentRatio);
                    updateItem.Pb = float.Parse(ssiFinancialIndicator.Pb);
                    updateList.Add(updateItem);
                }
                else
                {
                    insertList.Add(new()
                    {
                        YearReport = reportYear,
                        Symbol = symbol,
                        LengthReport = lengthReport,
                        Revenue = float.Parse(ssiFinancialIndicator.Revenue),
                        Profit = float.Parse(ssiFinancialIndicator.Profit),
                        Eps = float.Parse(ssiFinancialIndicator.Eps),
                        DilutedEps = float.Parse(ssiFinancialIndicator.DilutedEps),
                        Pe = float.Parse(ssiFinancialIndicator.Pe),
                        DilutedPe = float.Parse(ssiFinancialIndicator.DilutedPe),
                        Roe = float.Parse(ssiFinancialIndicator.Roe),
                        Roa = float.Parse(ssiFinancialIndicator.Roa),
                        Roic = float.Parse(ssiFinancialIndicator.Roic),
                        GrossProfitMargin = float.Parse(ssiFinancialIndicator.GrossProfitMargin),
                        NetProfitMargin = float.Parse(ssiFinancialIndicator.NetProfitMargin),
                        DebtAsset = float.Parse(ssiFinancialIndicator.DebtAsset),
                        QuickRatio = float.Parse(ssiFinancialIndicator.QuickRatio),
                        CurrentRatio = float.Parse(ssiFinancialIndicator.CurrentRatio),
                        Pb = float.Parse(ssiFinancialIndicator.Pb)
                    });
                }
            }

            var check = await _marketData.SaveFinancialIndicatorAsync(insertList, updateList);
            if (check)
            {
                _memoryCacheService.RemoveByPrefix(Constants.FinancialIndicatorCachePrefix);
                _workerQueueService.BroadcastUpdateMemoryTask(new("FinancialIndicator"));
            }
            return await _systemData.SetKeyValueAsync(checkingKey, check);
        }

        /// <summary>
        /// Xử lý bổ sung thông in vốn, cổ tức, tài sản doanh nghiệp theo mã
        /// </summary>
        /// <param name="schedule">Thông tin lịch</param>
        /// <returns>bool</returns>
        /// <exception cref="Exception">Schedule Null DataKey</exception>
        public virtual async Task<bool> UpdateCapitalAndDividendAsync(Schedule schedule)
        {
            var symbol = schedule.DataKey ?? throw new Exception($"Schedule Null DataKey, di: {schedule.Id}, type: {schedule.Type}");
            var checkingKey = $"{symbol}-Download-CapitalAndDividend";
            var ssiCapitalAndDividend = await _crawlData.DownloadCapitalAndDividendAsync(symbol);
            if (ssiCapitalAndDividend is null || ssiCapitalAndDividend.Data.CapAndDividend.TabcapitalDividendResponse.DataGroup.AssetlistList.Length <= 0)
            {
                _logger.LogWarning("UpdateCapitalAndDividendAsync => ssiCapitalAndDividend null info for code: {symbol}", symbol);
                return await _systemData.SetKeyValueAsync(checkingKey, false);
            }

            var insertList = new List<FinancialGrowth>();
            var updateList = new List<FinancialGrowth>();
            var listDbCheck = await _marketData.GetFinancialGrowthsAsync(symbol);

            if (ssiCapitalAndDividend?.Data?.CapAndDividend?.TabcapitalDividendResponse?.DataGroup?.AssetlistList?.Length > 0)
            {
                foreach (var asset in ssiCapitalAndDividend.Data.CapAndDividend.TabcapitalDividendResponse.DataGroup.AssetlistList)
                {
                    var year = int.Parse(asset.Year);
                    var updateItem = listDbCheck.FirstOrDefault(q => q.Year == year);
                    if (updateItem is null)
                    {
                        insertList.Add(new()
                        {
                            Year = year,
                            Symbol = symbol,
                            Asset = float.Parse(asset.Asset)
                        });
                    }
                    else
                    {
                        updateItem.Asset = float.Parse(asset.Asset);
                        updateList.Add(updateItem);
                    }
                }
            }

            if (ssiCapitalAndDividend?.Data?.CapAndDividend?.TabcapitalDividendResponse?.DataGroup?.CashdividendlistList?.Length > 0)
            {
                foreach (var cashdividend in ssiCapitalAndDividend.Data.CapAndDividend.TabcapitalDividendResponse.DataGroup.CashdividendlistList)
                {
                    var year = int.Parse(cashdividend.Year);
                    var currentAddItem = insertList.FirstOrDefault(q => q.Year == year);
                    if (currentAddItem != null)
                    {
                        currentAddItem.ValuePershare = float.Parse(cashdividend.ValuePershare);
                    }
                    var currentUpdateItem = updateList.FirstOrDefault(q => q.Year == year);
                    if (currentUpdateItem != null)
                    {
                        currentUpdateItem.ValuePershare = float.Parse(cashdividend.ValuePershare);
                    }
                }
            }

            if (ssiCapitalAndDividend?.Data?.CapAndDividend?.TabcapitalDividendResponse?.DataGroup?.OwnercapitallistList?.Length > 0)
            {
                foreach (var ownerCapital in ssiCapitalAndDividend.Data.CapAndDividend.TabcapitalDividendResponse.DataGroup.OwnercapitallistList)
                {
                    var year = int.Parse(ownerCapital.Year);
                    var currentAddItem = insertList.FirstOrDefault(q => q.Year == year);
                    if (currentAddItem != null)
                    {
                        currentAddItem.OwnerCapital = float.Parse(ownerCapital.OwnerCapital);
                    }
                    var currentUpdateItem = updateList.FirstOrDefault(q => q.Year == year);
                    if (currentUpdateItem != null)
                    {
                        currentUpdateItem.OwnerCapital = float.Parse(ownerCapital.OwnerCapital);
                    }
                }
            }

            var check = await _marketData.SaveFinancialGrowthAsync(insertList, updateList);
            return await _systemData.SetKeyValueAsync(checkingKey, check);
        }

        /// <summary>
        /// download và bổ sung thông tin lãn đạo doanh nghiệp
        /// </summary>
        /// <param name="schedule">Thông tin lịch</param>
        /// <returns>bool</returns>
        /// <exception cref="Exception">Schedule Null DataKey</exception>
        public virtual async Task<bool> UpdateLeadershipInfoAsync(Schedule schedule)
        {
            var symbol = schedule.DataKey ?? throw new Exception($"Schedule Null DataKey, di: {schedule.Id}, type: {schedule.Type}");
            var checkingKey = $"{symbol}-Download-LeadershipInfo";
            var ssiLeadership = await _crawlData.DownloadLeadershipAsync(symbol);
            if (ssiLeadership is null || ssiLeadership.Data.Leaderships.Datas.Length < 0)
            {
                _logger.LogWarning("UpdateLeadershipInfoAsync => ssiLeadership null info for code: {symbol}", symbol);
                return await _systemData.SetKeyValueAsync(checkingKey, false);
            }

            var insertList = new List<Leadership>();
            var dbLeaderships = new HashSet<Leadership>(await _marketData.GetLeadershipsAsync(symbol), new LeadershipComparer());

            foreach (var item in ssiLeadership.Data.Leaderships.Datas)
            {
                var newLeadership = new Leadership()
                {
                    Symbol = symbol,
                    FullName = item.FullName,
                    PositionName = item.PositionName,
                    PositionLevel = item.PositionLevel
                };
                if (dbLeaderships.Contains(newLeadership))
                {
                    dbLeaderships.Remove(newLeadership);
                }
                else
                {
                    insertList.Add(newLeadership);
                }
            }

            await _systemData.SetKeyValueAsync(checkingKey, true);
            return await _marketData.SaveLeadershipsAsync(insertList, dbLeaderships.ToList());
        }

        /// <summary>
        /// Tải thông tin doanh nghiệp
        /// </summary>
        /// <param name="schedule">Thông tin lịch</param>
        /// <returns>bool</returns>
        /// <exception cref="Exception">Can't download company info.</exception>
        public virtual async Task<bool> UpdateCompanyInfoAsync(Schedule schedule)
        {
            var symbol = schedule.DataKey ?? throw new Exception($"Schedule Null DataKey, di: {schedule.Id}, type: {schedule.Type}");
            var checkingKey = $"{symbol}-Download-FiinStockEvaluates";
            var ssiCompanyInfo = await _crawlData.DownloadCompanyInfoAsync(symbol);
            if (ssiCompanyInfo is null || ssiCompanyInfo.Data.CompanyStatistics is null)
            {
                _logger.LogWarning("UpdateCompanyInfoAsync => ssiCompanyInfo null info for code: {symbol}", symbol);
                return await _systemData.SetKeyValueAsync(checkingKey, false);
            }

            if (!string.IsNullOrWhiteSpace(ssiCompanyInfo.Data.CompanyProfile.SubsectorCode) && ssiCompanyInfo.Data.CompanyProfile.SubsectorCode != "0")
            {
                var saveIndustry = new Industry()
                {
                    Code = ssiCompanyInfo.Data.CompanyProfile.SubsectorCode,
                    Name = ssiCompanyInfo.Data.CompanyProfile.Subsector
                };
                await _marketData.SaveIndustryAsync(saveIndustry);
            }

            var company = new Company() { Symbol = symbol };
            string companyProfile = ssiCompanyInfo.Data.CompanyProfile.CompanyProfile;
            companyProfile = HttpUtility.HtmlDecode(companyProfile);
            companyProfile = HttpUtility.HtmlDecode(companyProfile);
            companyProfile = companyProfile.Replace("<div style=\"FONT-FAMILY: Arial; FONT-SIZE: 10pt;\">", "", StringComparison.OrdinalIgnoreCase);
            companyProfile = companyProfile.Replace("</div>", "", StringComparison.OrdinalIgnoreCase);
            companyProfile = companyProfile.Replace("<p>", "", StringComparison.OrdinalIgnoreCase);
            companyProfile = companyProfile.Replace("</p>", "", StringComparison.OrdinalIgnoreCase);

            company.SubsectorCode = ssiCompanyInfo.Data.CompanyProfile.SubsectorCode;
            company.IndustryName = ssiCompanyInfo.Data.CompanyProfile.IndustryName;
            company.Supersector = ssiCompanyInfo.Data.CompanyProfile.Supersector;
            company.Sector = ssiCompanyInfo.Data.CompanyProfile.Sector;
            company.Subsector = ssiCompanyInfo.Data.CompanyProfile.Subsector;
            company.CompanyName = ssiCompanyInfo.Data.CompanyProfile.CompanyName;
            company.FoundingDate = ParseDateType(ssiCompanyInfo.Data.CompanyProfile.FoundingDate);
            company.CharterCapital = float.Parse(ssiCompanyInfo.Data.CompanyProfile.CharterCapital);
            company.NumberOfEmployee = int.Parse(ssiCompanyInfo.Data.CompanyProfile.NumberOfEmployee ?? "0");
            company.BankNumberOfBranch = int.Parse(ssiCompanyInfo.Data.CompanyProfile.BankNumberOfBranch ?? "0");
            company.CompanyProfile = _zipHelper.ZipByte(Encoding.UTF8.GetBytes(companyProfile));
            company.ListingDate = ParseDateType(ssiCompanyInfo.Data.CompanyProfile.ListingDate);
            company.Exchange = ssiCompanyInfo.Data.CompanyProfile.Exchange;
            company.FirstPrice = float.Parse(ssiCompanyInfo.Data.CompanyProfile.FirstPrice);
            company.IssueShare = float.Parse(ssiCompanyInfo.Data.CompanyProfile.IssueShare);
            company.ListedValue = float.Parse(ssiCompanyInfo.Data.CompanyProfile.ListedValue);

            company.MarketCap = float.Parse(ssiCompanyInfo.Data.CompanyStatistics.MarketCap);
            company.SharesOutStanding = float.Parse(ssiCompanyInfo.Data.CompanyStatistics.SharesOutStanding);
            company.Bv = float.Parse(ssiCompanyInfo.Data.CompanyStatistics.BV);
            company.Beta = float.Parse(ssiCompanyInfo.Data.CompanyStatistics.BETA);
            company.Eps = float.Parse(ssiCompanyInfo.Data.CompanyStatistics.EPS);
            company.DilutedEps = float.Parse(ssiCompanyInfo.Data.CompanyStatistics.DilutedEps);
            company.Pe = float.Parse(ssiCompanyInfo.Data.CompanyStatistics.PE);
            company.Pb = float.Parse(ssiCompanyInfo.Data.CompanyStatistics.PB);
            company.DividendYield = float.Parse(ssiCompanyInfo.Data.CompanyStatistics.DividendYield);
            company.TotalRevenue = float.Parse(ssiCompanyInfo.Data.CompanyStatistics.TotalRevenue);
            company.Profit = float.Parse(ssiCompanyInfo.Data.CompanyStatistics.Profit);
            company.Asset = float.Parse(ssiCompanyInfo.Data.CompanyStatistics.Asset);
            company.Roe = float.Parse(ssiCompanyInfo.Data.CompanyStatistics.Roe);
            company.Roa = float.Parse(ssiCompanyInfo.Data.CompanyStatistics.Roa);
            company.Npl = float.Parse(ssiCompanyInfo.Data.CompanyStatistics.Npl);
            company.FinanciallEverage = float.Parse(ssiCompanyInfo.Data.CompanyStatistics.FinanciallEverage);
            var check = await _marketData.SaveCompanyAsync(company);
            if (check)
            {
                _workerQueueService.BroadcastUpdateMemoryTask(new("Company"));
            }
            return await _systemData.SetKeyValueAsync(checkingKey, check);
        }

        /// <summary>
        /// Xử lý thêm mới hoặc sửa thông tin chứng khoán hiện có
        /// </summary>
        public virtual async Task InitialStockAsync()
        {
            var ssiAllStock = await _crawlData.DownloadInitialMarketStockAsync();
            if (ssiAllStock is null || ssiAllStock.Data.Length <= 0)
            {
                await _systemData.SetKeyValueAsync("InitialStockDownloadStatus", false);
                _logger.LogWarning("InitialStockAsync => ssiAllStock is null.");
                return;
            }

            var allStocks = await _marketData.GetStockDictionaryAsync();
            var insertSchedules = new List<Schedule>();
            var updateStocks = new List<Stock>();
            var insertStocks = new List<Stock>();

            foreach (var datum in ssiAllStock.Data)
            {
                if (string.IsNullOrEmpty(datum.Type) || (datum.Type != "s" && datum.Type != "i"))
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
                        Symbol = stockCode,
                        Name = datum.Name,
                        FullName = datum.FullName,
                        Exchange = datum.Exchange,
                        Type = datum.Type
                    });
                    if (datum.Type == "s")
                    {
                        insertSchedules.Add(new()
                        {
                            Type = 1,
                            Name = $"Bổ sung thông tin doanh nghiệp theo mã: {stockCode}",
                            DataKey = stockCode,
                            ActiveTime = currentTime.AddMinutes(random.Next(0, 10))
                        });
                        insertSchedules.Add(new()
                        {
                            Type = 2,
                            Name = $"Bổ sung thông tin lãnh đạo mã: {stockCode}",
                            DataKey = stockCode,
                            ActiveTime = currentTime.AddMinutes(random.Next(0, 10))
                        });
                        insertSchedules.Add(new()
                        {
                            Type = 3,
                            Name = $"Bổ sung thông tin vốn, cổ tức, tài sản mã: {stockCode}",
                            DataKey = stockCode,
                            ActiveTime = currentTime.AddMinutes(random.Next(0, 10))
                        });
                        insertSchedules.Add(new()
                        {
                            Type = 4,
                            Name = $"Bổ sung thông tin tài chính doanh nghiệp mã: {stockCode}",
                            DataKey = stockCode,
                            ActiveTime = currentTime.AddMinutes(random.Next(0, 10))
                        });
                        insertSchedules.Add(new()
                        {
                            Type = 5,
                            Name = $"Bổ sung lịch sử giá cổ phiếu theo mã: {stockCode}",
                            DataKey = stockCode,
                            ActiveTime = currentTime.AddMinutes(random.Next(0, 10)),
                            OptionsJson = JsonSerializer.Serialize(new Dictionary<string, string>() { { "StockPricesCrawlSize", "90000" } })
                        });
                        insertSchedules.Add(new()
                        {
                            Type = 6,
                            Name = $"Bổ sung lịch sử sự kiện công ty theo mã: {stockCode}",
                            DataKey = stockCode,
                            ActiveTime = currentTime.AddMinutes(random.Next(0, 10)),
                            OptionsJson = JsonSerializer.Serialize(new Dictionary<string, string>() { { "CorporateActionCrawlSize", "90000" } })
                        });
                        insertSchedules.Add(new()
                        {
                            Type = 7,
                            Name = $"Bổ sung lịch sử gia dịch cho mã: {stockCode}",
                            DataKey = stockCode,
                            ActiveTime = currentTime.AddMinutes(random.Next(0, 10)),
                            OptionsJson = JsonSerializer.Serialize(new Dictionary<string, string>() { { "SsiStockNo", datum.StockNo } })
                        });
                        insertSchedules.Add(new()
                        {
                            Type = 8,
                            Name = $"Bổ sung đánh giá của fiintrade cho mã: {stockCode}",
                            DataKey = stockCode,
                            ActiveTime = currentTime.AddMinutes(random.Next(0, 10))
                        });
                        insertSchedules.Add(new()
                        {
                            Name = $"Tải dữ liệu chart price: {stockCode}",
                            Type = 9,
                            DataKey = stockCode,
                            ActiveTime = currentTime.AddMinutes(random.Next(0, 10)),
                            OptionsJson = JsonSerializer.Serialize(new Dictionary<string, string>()
                            {
                                {"StartTime", _indexStartDownloadTime.ToUnixTimeSeconds().ToString() },
                                {"ChartType", "D" }
                            })
                        });
                        insertSchedules.Add(new()
                        {
                            Type = 11,
                            Name = $"Thu thập khuyến nghị của các công ty chứng khoán cho mã: {stockCode}",
                            DataKey = stockCode,
                            ActiveTime = currentTime.AddMinutes(random.Next(0, 10))
                        });
                        insertSchedules.Add(new()
                        {
                            Type = 12,
                            Name = $"Thu thập đánh giá cổ phiếu của vndirect cho mã: {stockCode}",
                            DataKey = stockCode,
                            ActiveTime = currentTime.AddMinutes(random.Next(0, 10))
                        });


                        insertSchedules.Add(new()
                        {
                            Type = 200,
                            Name = $"Phân tích giá trị doanh nghiệp cho mã: {stockCode}",
                            DataKey = stockCode,
                            ActiveTime = currentTime.AddHours(1).AddMinutes(5)
                        });

                        insertSchedules.Add(new()
                        {
                            Type = 201,
                            Name = $"Phân tích chỉ số kỹ thuật cho mã: {stockCode}",
                            DataKey = stockCode,
                            ActiveTime = currentTime.AddHours(1).AddMinutes(5)
                        });
                        insertSchedules.Add(new()
                        {
                            Type = 202,
                            Name = $"Trading thư nghiệm cho mã: {stockCode}",
                            DataKey = stockCode,
                            ActiveTime = currentTime.AddHours(1).AddMinutes(6)
                        });
                        insertSchedules.Add(new()
                        {
                            Type = 203,
                            Name = $"Tính toán giá dự phóng của các công ty chứng khoán cho mã: {stockCode}",
                            DataKey = stockCode,
                            ActiveTime = currentTime.AddHours(1).AddMinutes(5)
                        });
                        insertSchedules.Add(new()
                        {
                            Type = 208,
                            Name = $"Phân tích các yếu tố vĩ mô tác động đến cổ phiếu: {stockCode}",
                            DataKey = stockCode,
                            ActiveTime = currentTime.AddHours(1).AddMinutes(5)
                        });
                        insertSchedules.Add(new()
                        {
                            Type = 209,
                            Name = $"Đánh giá tăng trưởng doanh nghiệp: {stockCode}",
                            DataKey = stockCode,
                            ActiveTime = currentTime.AddHours(1).AddMinutes(5)
                        });
                        insertSchedules.Add(new()
                        {
                            Type = 210,
                            Name = $"Phân tích đánh giá của fiintrading: {stockCode}",
                            DataKey = stockCode,
                            ActiveTime = currentTime.AddHours(1).AddMinutes(5)
                        });
                        insertSchedules.Add(new()
                        {
                            Type = 211,
                            Name = $"Phân tích đánh giá của vnd: {stockCode}",
                            DataKey = stockCode,
                            ActiveTime = currentTime.AddHours(1).AddMinutes(5)
                        });
                        insertSchedules.Add(new()
                        {
                            Type = 212,
                            Name = $"Tìm kiếm các đặc trưng của cô phiếu cho mã: {stockCode}",
                            DataKey = stockCode,
                            ActiveTime = currentTime.AddHours(1).AddMinutes(5)
                        });
                    }
                    else
                    {
                        insertSchedules.Add(new()
                        {
                            Type = 207,
                            Name = $"Phân tích tâm lý, kỹ thuật cho chỉ số: {stockCode}",
                            DataKey = stockCode,
                            ActiveTime = currentTime.AddHours(1).AddMinutes(4)
                        });
                        insertSchedules.Add(new()
                        {
                            Name = $"Tải dữ liệu chỉ số: {stockCode}",
                            Type = 9,
                            DataKey = stockCode,
                            ActiveTime = currentTime.AddMinutes(random.Next(0, 10)),
                            OptionsJson = JsonSerializer.Serialize(new Dictionary<string, string>()
                            {
                                {"StartTime", _indexStartDownloadTime.ToUnixTimeSeconds().ToString() },
                                {"ChartType", "D" }
                            })
                        });
                    }
                }
            }

            await _marketData.InitialStockAsync(insertStocks, updateStocks);
            await _systemData.InsertScheduleAsync(insertSchedules);
            await _systemData.SetKeyValueAsync("InitialStockDownloadStatus", true);

            if (updateStocks.Count > 0)
            {
                var queueMessage = new QueueMessage("Stocks");
                queueMessage.KeyValues.Add("Symbols", JsonSerializer.Serialize(updateStocks.Select(q => q.Symbol)));
                _workerQueueService.BroadcastUpdateMemoryTask(queueMessage);
            }
        }

        /// <summary>
        /// Binding dữ liệu từ 2 object thông tin chứng khoán
        /// </summary>
        /// <param name="stockPrice">Dữ liệu cần lưu</param>
        /// <param name="stockPriceDl">Dữ liệu gốc</param>
        private static void StockPriceBindValue(ref StockPrice stockPrice, Entities.CrawlObjects.StockPriceSsi stockPriceDl)
        {
            stockPrice.PriceChange = float.Parse(stockPriceDl.PriceChange);
            stockPrice.PerPriceChange = float.Parse(stockPriceDl.PerPriceChange);
            stockPrice.CeilingPrice = float.Parse(stockPriceDl.CeilingPrice);
            stockPrice.FloorPrice = float.Parse(stockPriceDl.FloorPrice);
            stockPrice.RefPrice = float.Parse(stockPriceDl.RefPrice);
            stockPrice.OpenPrice = float.Parse(stockPriceDl.OpenPrice);
            stockPrice.HighestPrice = float.Parse(stockPriceDl.HighestPrice);
            stockPrice.LowestPrice = float.Parse(stockPriceDl.LowestPrice);
            stockPrice.ClosePrice = float.Parse(stockPriceDl.ClosePrice);
            stockPrice.AveragePrice = float.Parse(stockPriceDl.AveragePrice);
            stockPrice.ClosePriceAdjusted = float.Parse(stockPriceDl.ClosePriceAdjusted);
            stockPrice.TotalMatchVol = float.Parse(stockPriceDl.TotalMatchVol);
            stockPrice.TotalMatchVal = float.Parse(stockPriceDl.TotalMatchVal);
            stockPrice.TotalDealVal = float.Parse(stockPriceDl.TotalDealVal);
            stockPrice.TotalDealVol = float.Parse(stockPriceDl.TotalDealVol);
            stockPrice.ForeignBuyVolTotal = float.Parse(stockPriceDl.ForeignBuyVolTotal);
            stockPrice.ForeignCurrentRoom = float.Parse(stockPriceDl.ForeignCurrentRoom);
            stockPrice.ForeignSellVolTotal = float.Parse(stockPriceDl.ForeignSellVolTotal);
            stockPrice.ForeignBuyValTotal = float.Parse(stockPriceDl.ForeignBuyValTotal);
            stockPrice.ForeignSellValTotal = float.Parse(stockPriceDl.ForeignSellValTotal);
            stockPrice.TotalBuyTrade = float.Parse(stockPriceDl.TotalBuyTrade);
            stockPrice.TotalBuyTradeVol = float.Parse(stockPriceDl.TotalBuyTradeVol);
            stockPrice.TotalSellTrade = float.Parse(stockPriceDl.TotalSellTrade);
            stockPrice.TotalSellTradeVol = float.Parse(stockPriceDl.TotalSellTradeVol);
            stockPrice.NetBuySellVol = float.Parse(stockPriceDl.NetBuySellVol);
            stockPrice.NetBuySellVal = float.Parse(stockPriceDl.NetBuySellVal);
        }

        /// <summary>
        /// hàm xử lý chuyển đổi string thành ngày tháng theo định dạng dd/MM/yyyy HH:mm:ss
        /// </summary>
        /// <param name="value">dữ liệu cần chuyển đổi</param>
        /// <returns>DateTime  or Null</returns>
        private DateTime? ParseDateType(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            try
            {
                if (value.Contains('T'))
                {
                    string parseString = value.Replace('T', ' ');
                    var dateTValue = DateTime.ParseExact(parseString, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                    if (dateTValue == DateTime.MinValue)
                    {
                        return null;
                    }
                    return dateTValue;
                }

                var dateValue = DateTime.ParseExact(value, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                if (dateValue == DateTime.MinValue)
                {
                    return null;
                }
                return dateValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ParseDateType error: {value}", value);
                return null;
            }
        }
    }
}
