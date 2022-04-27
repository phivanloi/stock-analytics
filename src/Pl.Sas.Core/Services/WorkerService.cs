using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using System.Diagnostics;
using System.Globalization;
using System.Text;
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
                            ActiveTime = currentTime.AddMinutes(random.Next(0, 60))
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
                            ActiveTime = currentTime.AddMinutes(random.Next(0, 60))
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

            }
            finally
            {
                await _analyticsData.UpdateStockTrackingAsync(stockTracking);
            }

            var queueMessage = new QueueMessage("UpdatedStock");
            queueMessage.KeyValues.Add("Symbol", schedule.DataKey);
            return queueMessage;
        }

        #region Stock download
        /// <summary>
        /// download và bổ sung thông tin lãn đạo doanh nghiệp
        /// </summary>
        /// <param name="stockTracking">Thông tin trạng thái kiểm tra download dữ liệu của cổ phiếu</param>
        /// <returns></returns>
        /// <exception cref="Exception">Can't download leadership info.</exception>
        public virtual async Task<bool> UpdateLeadershipInfoAsync(StockTracking stockTracking)
        {
            stockTracking.DownloadStatus = "Tải và xử danh sách lãnh đạo cho công ty.";
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
