using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
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
    /// các nghiệp vụ được xử lý bởi worker
    /// </summary>
    public class WorkerService
    {
        private readonly IMarketData _marketData;
        private readonly ICrawlData _crawlData;
        private readonly IAnalyticsData _analyticsData;
        private readonly ILogger<WorkerService> _logger;
        private readonly IZipHelper _zipHelper;

        public WorkerService(
            IZipHelper zipHelper,
            IAnalyticsData analyticsData,
            ILogger<WorkerService> logger,
            ICrawlData crawlData,
            IMarketData marketData)
        {
            _marketData = marketData;
            _crawlData = crawlData;
            _logger = logger;
            _analyticsData = analyticsData;
            _zipHelper = zipHelper;
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

                    case 10:
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
            var insertStockTrackings = new List<StockTracking>();
            foreach (var datum in ssiAllStock.Data)
            {
                if (string.IsNullOrEmpty(datum.Type) || datum.Type != "s" || datum.Type != "i")
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
                    insertStocks.Add(new(stockCode)
                    {
                        Name = datum.Name,
                        FullName = datum.FullName,
                        Exchange = datum.Exchange,
                        Type = datum.Type
                    });
                    if (datum.Type == "s")
                    {
                        insertSchedules.Add(new()
                        {
                            Name = $"Tải và phân tích mã chứng khoán: {stockCode}",
                            Type = 10,
                            DataKey = stockCode,
                            ActiveTime = currentTime.AddMinutes(random.Next(0, 10)),
                            OptionsJson = JsonSerializer.Serialize(new Dictionary<string, string>()
                            {
                                {"CorporateActionCrawlSize","10000" },
                                {"StockPricesCrawlSize","10000" }
                            })
                        });
                        insertStockTrackings.Add(new(stockCode));
                    }
                    else
                    {
                        insertSchedules.Add(new()
                        {
                            Name = $"Tải dữ liệu chỉ số: {stockCode}",
                            Type = 11,
                            DataKey = stockCode,
                            ActiveTime = currentTime.AddMinutes(random.Next(0, 10)),
                            OptionsJson = JsonSerializer.Serialize(new Dictionary<string, string>()
                            {
                                {"DateCrawlStart","10000" }
                            })
                        });
                    }
                }
            }

            await _marketData.InitialStockAsync(insertStocks, updateStocks);
            await _marketData.InsertScheduleAsync(insertSchedules);
            await _analyticsData.InsertStockTrackingAsync(insertStockTrackings);

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
            Guard.Against.NullOrEmpty(schedule.DataKey, nameof(schedule.DataKey));
            var stockTracking = new StockTracking(schedule.DataKey);
            try
            {
                await UpdateCompanyInfoAsync(stockTracking);//xử lý thông tin công ty
                await UpdateLeadershipInfoAsync(stockTracking);//xử lý thông tin lãnh đạo của công ty
                await UpdateCapitalAndDividendAsync(stockTracking);//Xử lý vốn vả cổ thức
                await UpdateFinancialIndicatorAsync(stockTracking);//xử lý chỉ số tài chính của công ty
                await UpdateCorporateActionInfoAsync(stockTracking, int.Parse(schedule.Options["CorporateActionCrawlSize"]));//xử lý hoạt động của công ty

            }
            finally
            {
                await _analyticsData.UpdateStockTrackingAsync(stockTracking);
                if (schedule.Options["CorporateActionCrawlSize"] == "10000")
                {
                    await _marketData.UpdateKeyOptionScheduleAsync(schedule, "CorporateActionCrawlSize", "10");
                }
                if (schedule.Options["StockPricesCrawlSize"] == "10000")
                {
                    await _marketData.UpdateKeyOptionScheduleAsync(schedule, "StockPricesCrawlSize", "10");
                }
            }

            var queueMessage = new QueueMessage("UpdatedStock");
            queueMessage.KeyValues.Add("Symbol", schedule.DataKey);
            return queueMessage;
        }

        #region Stock download

        public virtual async Task UpdateStockPriceHistoryAsync(StockTracking stockTracking, int size)
        {
            stockTracking.DownloadStatus = "Tải và xử lý dữ liệu lịch sử giá chứng khoán.";
            stockTracking.DownloadDate = DateTime.Now;

            var stockPriceHistory = await _crawlData.DownloadStockPricesAsync(stockTracking.Symbol, size) ?? throw new Exception("Can't stock prices info.");
            if (stockPriceHistory.Data.StockPrice.DataList.Length <= 0)
            {
                _logger.LogWarning("UpdateStockPriceHistoryAsync null histories for: {Symbol}.", stockTracking.Symbol);
                return;
            }

            var insertList = new List<StockPrice>();
            var updateList = new List<StockPrice>();
            if (size <= 10)
            {
                var tradingDate = ParseDateType(stockPriceSsi.TradingDate);
                if (tradingDate is null)
                {
                    return;
                }
                var datePath = Utilities.GetTradingDatePath(tradingDate);
                var updateItem = await _stockPriceData.GetByDayAsync(schedule.DataKey, datePath);
                if (updateItem != null)
                {
                    StockPriceBindValue(ref updateItem, stockPriceSsi);
                    await _stockPriceData.UpdateAsync(updateItem);
                }
                else
                {
                    var addItem = new StockPrice()
                    {
                        Symbol = schedule.DataKey,
                        TradingDate = tradingDate.Value,
                        DatePath = datePath
                    };
                    StockPriceBindValue(ref addItem, stockPriceSsi);
                    await _stockPriceData.InsertAsync(addItem);
                }
                await UpdateStockPriceFromApiAsync(schedule, stockPriceHistory.Data.StockPrice.DataList[0]);
            }
            else
            {
                var datePathHashSet = await _stockPriceData.GetAllDatePathBySymbol(schedule.DataKey);
                var insertStockPrices = new List<StockPrice>();
                foreach (var stockPrice in stockPriceHistory.Data.StockPrice.DataList)
                {
                    var tradingDate = ParseDateType(stockPrice.TradingDate);
                    if (tradingDate is null)
                    {
                        continue;
                    }

                    var datePath = Utilities.GetTradingDatePath(tradingDate);
                    if (!datePathHashSet.Contains(datePath))
                    {
                        var addItem = new StockPrice()
                        {
                            Symbol = schedule.DataKey,
                            TradingDate = tradingDate.Value,
                            DatePath = datePath
                        };
                        StockPriceBindValue(ref addItem, stockPrice);
                        insertStockPrices.Add(addItem);
                    }
                }
                await _operationRetry.Retry(() => _stockPriceData.BulkInserAsync(insertStockPrices), 10, TimeSpan.FromMilliseconds(100));
                await UpdateIsNextTimeUpdate(schedule);
            }

            var updateMemoryMessage = new QueueMessage() { Id = "StockPrices" };
            updateMemoryMessage.KeyValues.Add("Symbol", schedule.DataKey);
            _processorMessageQueueService.BroadcastUpdateMemoryTask(updateMemoryMessage);
        }

        /// <summary>
        /// Xử lý thông tin tài chính của doanh nghiệp
        /// </summary>
        /// <param name="stockTracking">Thông tin trạng thái kiểm tra download dữ liệu của cổ phiếu</param>
        /// <returns>bool</returns>
        /// <exception cref="Exception">Can't corporate action info.</exception>
        public virtual async Task<bool> UpdateFinancialIndicatorAsync(StockTracking stockTracking)
        {
            stockTracking.DownloadStatus = "Tải và xử lý dữ liệu tài chính của công ty.";
            stockTracking.DownloadDate = DateTime.Now;

            var ssiFinancialIndicators = await _crawlData.DownloadFinancialIndicatorAsync(stockTracking.Symbol) ?? throw new Exception("Can't financial indicator info.");
            var insertList = new List<FinancialIndicator>();
            var updateList = new List<FinancialIndicator>();
            var listDbCheck = await _marketData.GetFinancialIndicatorsAsync(stockTracking.Symbol);
            foreach (var ssiFinancialIndicator in ssiFinancialIndicators.Data.FinancialIndicator.DataList)
            {
                var reportYear = int.Parse(ssiFinancialIndicator.YearReport);
                var lengthReport = int.Parse(ssiFinancialIndicator.LengthReport);
                var updateItem = listDbCheck.FirstOrDefault(q => q.YearReport == reportYear && q.LengthReport == lengthReport);
                if (updateItem is not null)
                {
                    updateItem.YearReport = reportYear;
                    updateItem.Symbol = stockTracking.Symbol;
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
                        Symbol = stockTracking.Symbol,
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
            return await _marketData.SaveFinancialIndicatorAsync(insertList, updateList);
        }

        /// <summary>
        /// Xử lý thông tin hoạt động của doanh nghiệp
        /// </summary>
        /// <param name="stockTracking">Thông tin trạng thái kiểm tra download dữ liệu của cổ phiếu</param>
        /// <returns>bool</returns>
        /// <exception cref="Exception">Can't corporate action info.</exception>
        public virtual async Task<bool> UpdateCorporateActionInfoAsync(StockTracking stockTracking, int size)
        {
            stockTracking.DownloadStatus = "Tải và xử lý dữ liệu sư kiện của công ty.";
            stockTracking.DownloadDate = DateTime.Now;

            var ssiCorporateAction = await _crawlData.DownloadCorporateActionAsync(stockTracking.Symbol, size) ?? throw new Exception("Can't corporate action info.");
            var insertList = new List<CorporateAction>();
            var corporateActions = await _marketData.GetCorporateActionsAsync(stockTracking.Symbol);
            foreach (var item in ssiCorporateAction.Data.CorporateActions.DataList)
            {
                if (string.IsNullOrEmpty(item.ExrightDate))
                {
                    continue;
                }
                var newLeadership = new CorporateAction()
                {
                    Symbol = stockTracking.Symbol,
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
            return await _marketData.InsertCorporateActionAsync(insertList);
        }

        /// <summary>
        /// download và bổ sung thông tin lãn đạo doanh nghiệp
        /// </summary>
        /// <param name="stockTracking">Thông tin trạng thái kiểm tra download dữ liệu của cổ phiếu</param>
        /// <returns></returns>
        /// <exception cref="Exception">Can't download leadership info.</exception>
        public virtual async Task<bool> UpdateLeadershipInfoAsync(StockTracking stockTracking)
        {
            stockTracking.DownloadStatus = "Tải và xử lý danh sách lãnh đạo cho công ty.";
            stockTracking.DownloadDate = DateTime.Now;

            var ssiLeadership = await _crawlData.DownloadLeadershipAsync(stockTracking.Symbol) ?? throw new Exception("Can't download leadership info.");

            var insertList = new List<Leadership>();
            var dbLeaderships = new HashSet<Leadership>(await _marketData.GetLeadershipsAsync(stockTracking.Symbol), new LeadershipComparer());
            foreach (var item in ssiLeadership.Data.Leaderships.Datas)
            {
                var newLeadership = new Leadership()
                {
                    Symbol = stockTracking.Symbol,
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
            return await _marketData.SaveLeadershipsAsync(insertList, dbLeaderships.ToList());
        }

        /// <summary>
        /// Tải thông tin doanh nghiệp
        /// </summary>
        /// <param name="stockTracking">Thông tin trạng thái kiểm tra download dữ liệu của cổ phiếu</param>
        /// <returns>bool</returns>
        /// <exception cref="Exception">Can't download company info.</exception>
        public virtual async Task<bool> UpdateCompanyInfoAsync(StockTracking stockTracking)
        {
            stockTracking.DownloadStatus = "Tải và xử lý thông tin công ty.";
            stockTracking.DownloadDate = DateTime.Now;

            var ssiCompanyInfo = await _crawlData.DownloadCompanyInfoAsync(stockTracking.Symbol) ?? throw new Exception("Can't download company info.");
            if (!string.IsNullOrWhiteSpace(ssiCompanyInfo.Data.CompanyProfile.SubsectorCode) && ssiCompanyInfo.Data.CompanyProfile.SubsectorCode != "0")
            {
                var saveIndustry = new Industry(ssiCompanyInfo.Data.CompanyProfile.SubsectorCode, ssiCompanyInfo.Data.CompanyProfile.Subsector);
                await _marketData.SaveIndustryAsync(saveIndustry);
            }

            var company = new Company(stockTracking.Symbol);
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
            return await _marketData.SaveCompanyAsync(company);
        }

        /// <summary>
        /// Xử lý bổ sung thông in vốn, cổ tức, tài sản doanh nghiệp theo mã
        /// </summary>
        /// <param name="stockTracking">Thông tin trạng thái kiểm tra download dữ liệu của cổ phiếu</param>
        /// <returns></returns>
        public virtual async Task<bool> UpdateCapitalAndDividendAsync(StockTracking stockTracking)
        {
            stockTracking.DownloadStatus = "Tải và xử lý thông tin vốn và cổ tức.";
            stockTracking.DownloadDate = DateTime.Now;

            var ssiCapitalAndDividend = await _crawlData.DownloadCapitalAndDividendAsync(stockTracking.Symbol) ?? throw new Exception("Can't download capital, dividend and asset info.");
            var insertList = new List<FinancialGrowth>();
            var updateList = new List<FinancialGrowth>();
            var listDbCheck = await _marketData.GetFinancialGrowthsAsync(stockTracking.Symbol);

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
                            Symbol = stockTracking.Symbol,
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

            return await _marketData.SaveFinancialGrowthAsync(insertList, updateList);
        }
        #endregion

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
