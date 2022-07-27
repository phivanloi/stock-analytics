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
    /// các nghiệp vụ phân tích dữ liệu
    /// </summary>
    public class AnalyticsService
    {
        private readonly ILogger<AnalyticsService> _logger;
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
        private readonly IAsyncCacheService _asyncCacheService;

        public AnalyticsService(
            IAsyncCacheService asyncCacheService,
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
            ILogger<AnalyticsService> logger)
        {
            _asyncCacheService = asyncCacheService;
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
            var schedule = await _scheduleData.GetByIdAsync(queueMessage.Id);
            if (schedule != null)
            {
                Stopwatch stopWatch = new();
                stopWatch.Start();

                switch (schedule.Type)
                {
                    case 200:
                        _logger.LogInformation("Run company analytics for {DataKey}.", schedule.DataKey);
                        await CompanyValueAnalyticsAsync(schedule.DataKey ?? "");
                        break;

                    case 201:
                        _logger.LogInformation("Run stock price analytics for {DataKey}.", schedule.DataKey);
                        await StockPriceTechnicalAnalyticsAsync(schedule.DataKey ?? "");
                        break;

                    case 202:
                        _logger.LogInformation("Run test trading analytics for {DataKey}.", schedule.DataKey);
                        await TestTradingAnalyticsAsync(schedule.DataKey ?? "");
                        break;

                    case 203:
                        _logger.LogInformation("Run target price analytics => {DataKey}.", schedule.DataKey);
                        await TargetPriceAnalyticsAsync(schedule.DataKey ?? "");
                        break;

                    case 204:
                        _logger.LogInformation("Run industry trend analytics for {Id}.", schedule.Id);
                        await IndustryTrendAnalyticsAsync();
                        break;

                    case 205:
                        _logger.LogInformation("Run update price on corporate action for {Id}.", schedule.Id);
                        await CorporateActionToPriceAsync();
                        break;

                    case 207:
                        _logger.LogInformation("Run market sentiment analytics => {DataKey}.", schedule.DataKey);
                        await MarketSentimentAnalyticsAsync(schedule);
                        break;

                    case 208:
                        _logger.LogInformation("Run macroeconomics analytics => {DataKey}.", schedule.DataKey);
                        await MarketAnalyticsAsync(schedule.DataKey ?? "");
                        break;

                    case 209:
                        _logger.LogInformation("Run company growth analytics => {DataKey}.", schedule.DataKey);
                        await CompanyGrowthAnalyticsAsync(schedule.DataKey ?? "");
                        break;

                    case 210:
                        _logger.LogInformation("Run fiintrading analytics => {DataKey}.", schedule.DataKey);
                        await FiinAnalyticsAsync(schedule.DataKey ?? "");
                        break;

                    case 211:
                        _logger.LogInformation("Run vnd score analytics => {DataKey}.", schedule.DataKey);
                        await VndScoreAnalyticsAsync(schedule.DataKey ?? "");
                        break;

                    case 212:
                        _logger.LogWarning("Chưa viết hàm xử lý type 212 => {DataKey}.", schedule.DataKey);
                        //await VndScoreAnalyticsAsync(schedule.DataKey ?? "");
                        break;

                    default:
                        _logger.LogWarning("Scheduler id {Id}, type: {Type} don't match any function", queueMessage.Id, schedule.Type);
                        stopWatch.Stop();
                        return;
                }

                stopWatch.Stop();
                _logger.LogInformation("Worker analytics {Id} in {ElapsedMilliseconds} miniseconds.", queueMessage.Id, stopWatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogWarning("AnalyticsService HandleEventAsync null scheduler Id: {Id}.", queueMessage.Id);
            }
        }

        /// <summary>
        /// Phân tích giá trị doanh nghiệp
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <returns>bool?</returns>
        public virtual async Task<bool> CompanyValueAnalyticsAsync(string symbol)
        {
            Guard.Against.NullOrEmpty(symbol, nameof(symbol));

            var checkingKey = $"{symbol}-Analytics-CompanyValue";
            var company = await _companyData.FindBySymbolAsync(symbol);
            var stockPrice = await _stockPriceData.GetLastAsync(symbol);
            if (company is null || stockPrice is null)
            {
                _logger.LogWarning("CompanyValueAnalyticsAsync can't find company and last stock price with symbol: {symbol}", symbol);
                return await _keyValueData.SetAsync(checkingKey, false);
            }

            var score = 0;
            var companyValueNotes = new List<AnalyticsNote>();
            var companies = await _companyData.CacheFindAllForAnalyticsAsync();
            if (companies is null)
            {
                _logger.LogWarning("CompanyValueAnalyticsAsync can't find any company same industries with symbol: {symbol}", symbol);
                return await _keyValueData.SetAsync(checkingKey, false);
            }
            var companiesSameIndustries = companies.Where(q => q.SubsectorCode == company.SubsectorCode).ToList();
            var financialIndicators = await _financialIndicatorData.CacheGetBySymbolsAsync(
                company.SubsectorCode,
                companiesSameIndustries.Select(q => q.Symbol).ToArray(), 5);
            if (financialIndicators is null)
            {
                _logger.LogWarning("CompanyValueAnalyticsAsync can't find financial indicatiors same industries with symbol: {symbol}", symbol);
                return await _keyValueData.SetAsync(checkingKey, false);
            }
            var fiinEvaluates = await _fiinEvaluatedData.FindAsync(symbol);
            var vndScores = await _vndStockScoreData.FindAllAsync(symbol);
            var lastQuarterlyFinancialIndicator = financialIndicators
                .Where(q => q.Symbol == symbol)
                .OrderByDescending(q => q.YearReport)
                .ThenByDescending(q => q.LengthReport)
                .FirstOrDefault(q => q.LengthReport != 5);
            var lastYearlyFinancialIndicator = financialIndicators
                .Where(q => q.Symbol == symbol)
                .OrderByDescending(q => q.YearReport)
                .ThenByDescending(q => q.LengthReport)
                .FirstOrDefault(q => q.LengthReport == 5);
            var lastYearlyFinancialIndicatorSameIndustries = financialIndicators
                .Where(q => companiesSameIndustries.Any(c => c.Symbol == q.Symbol) && q.LengthReport == 5)
                .GroupBy(q => q.Symbol)
                .Select(q => q.OrderByDescending(c => c.YearReport).First())
                .ToList();
            var lastQuarterlyFinancialIndicatorSameIndustries = financialIndicators
                .Where(q => companiesSameIndustries.Any(c => c.Symbol == q.Symbol))
                .GroupBy(q => q.Symbol)
                .Select(q => q.OrderByDescending(c => c.YearReport).ThenByDescending(q => q.LengthReport).First())
                .ToList();

            var bankInterestRate6 = await _keyValueData.GetAsync(Constants.BankInterestRate6Key);
            var bankInterestRate12 = await _keyValueData.GetAsync(Constants.BankInterestRate12Key);
            var dividendYield = company.DividendYield;
            if (dividendYield <= 0)
            {
                var financialGrowth = await _financialGrowthData.GetLastAsync(symbol);
                if (financialGrowth is not null && financialGrowth.ValuePershare > 0 && stockPrice.ClosePriceAdjusted > 0)
                {
                    dividendYield = financialGrowth.ValuePershare / stockPrice.ClosePriceAdjusted;
                }
            }

            score += CompanyValueAnalyticsService.FoundingDateCheck(companyValueNotes, company);
            score += CompanyValueAnalyticsService.RevenueCheck(companyValueNotes, company, financialIndicators);
            score += CompanyValueAnalyticsService.ProfitCheck(companyValueNotes, company, financialIndicators);
            score += CompanyValueAnalyticsService.EpsCheck(companyValueNotes, company, companiesSameIndustries);
            score += CompanyValueAnalyticsService.PeCheck(companyValueNotes, company, companiesSameIndustries);
            score += CompanyValueAnalyticsService.RoeCheck(companyValueNotes, company, companiesSameIndustries);
            score += CompanyValueAnalyticsService.RoaCheck(companyValueNotes, company, companiesSameIndustries);
            score += CompanyValueAnalyticsService.EpLastQuarterlyCheck(companyValueNotes, lastQuarterlyFinancialIndicator, stockPrice.ClosePriceAdjusted, bankInterestRate6?.GetValue<float>() ?? 6.8f);
            score += CompanyValueAnalyticsService.EpLastYearlyCheck(companyValueNotes, lastYearlyFinancialIndicator, stockPrice.ClosePriceAdjusted, bankInterestRate12?.GetValue<float>() ?? 6.8f);
            score += CompanyValueAnalyticsService.MarketCapCheck(companyValueNotes, company, companiesSameIndustries, companies.Average(q => q.MarketCap));
            score += CompanyValueAnalyticsService.DividendDivCheck(companyValueNotes, bankInterestRate12?.GetValue<float>() ?? 6.8f, dividendYield);
            score += CompanyValueAnalyticsService.PbCheck(companyValueNotes, company, companiesSameIndustries, companies);
            score += CompanyValueAnalyticsService.GrossProfitMarginCheck(companyValueNotes, lastQuarterlyFinancialIndicator, bankInterestRate12?.GetValue<float>() ?? 6.8f);
            score += CompanyValueAnalyticsService.DebtAssetCheck(companyValueNotes, lastYearlyFinancialIndicator, lastYearlyFinancialIndicatorSameIndustries);
            score += CompanyValueAnalyticsService.DebtEquityCheck(companyValueNotes, lastYearlyFinancialIndicator, lastYearlyFinancialIndicatorSameIndustries);
            score += CompanyValueAnalyticsService.CurrentRatioCheck(companyValueNotes, lastQuarterlyFinancialIndicator, lastQuarterlyFinancialIndicatorSameIndustries);
            score += CompanyValueAnalyticsService.CharterCapitalCheck(companyValueNotes, company, companiesSameIndustries);
            score += CompanyValueAnalyticsService.FiinValueCheck(companyValueNotes, fiinEvaluates);
            if (company.SubsectorCode != "8355")//Không phải là ngân hàng thì đánh giá của vnd có giá trị
            {
                score += CompanyValueAnalyticsService.VndValueCheck(companyValueNotes, vndScores, symbol);
            }

            var saveZipData = _zipHelper.ZipByte(JsonSerializer.SerializeToUtf8Bytes(companyValueNotes));
            var check = await _analyticsResultData.SaveCompanyValueScoreAsync(symbol, score, saveZipData);
            return await _keyValueData.SetAsync(checkingKey, check);
        }

        /// <summary>
        /// Phân tích tăng trưởng giá trị doanh nghiệp
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <returns></returns>
        public virtual async Task<bool> CompanyGrowthAnalyticsAsync(string symbol)
        {
            Guard.Against.NullOrEmpty(symbol, nameof(symbol));

            var checkingKey = $"{symbol}-Analytics-CompanyGrowth";
            var company = await _companyData.FindBySymbolAsync(symbol);
            var stockPrice = await _stockPriceData.GetLastAsync(symbol);
            if (company is null || stockPrice is null)
            {
                _logger.LogWarning("CompanyGrowthAnalyticsAsync can't find company and last stock price with symbol: {symbol}", symbol);
                return await _keyValueData.SetAsync(checkingKey, false);
            }

            var score = 0;
            var companyGrowthNotes = new List<AnalyticsNote>();
            var companies = await _companyData.CacheFindAllForAnalyticsAsync();
            if (companies is null)
            {
                _logger.LogWarning("CompanyValueAnalyticsAsync can't find any company same industries with symbol: {symbol}", symbol);
                return await _keyValueData.SetAsync(checkingKey, false);
            }
            var companiesSameIndustries = companies.Where(q => q.SubsectorCode == company.SubsectorCode).ToList();
            var financialIndicators = await _financialIndicatorData.CacheGetBySymbolsAsync(
                company.SubsectorCode,
                companiesSameIndustries.Select(q => q.Symbol).ToArray(), 5);
            if (financialIndicators is null)
            {
                _logger.LogWarning("CompanyValueAnalyticsAsync can't find financial indicatiors same industries with symbol: {symbol}", symbol);
                return await _keyValueData.SetAsync(checkingKey, false);
            }
            var financialGrowths = await _financialGrowthData.FindAllAsync(company.Symbol);
            var fiinEvaluates = await _fiinEvaluatedData.FindAsync(symbol);
            var vndScores = await _vndStockScoreData.FindAllAsync(symbol);

            score += CompanyGrowthAnalyticsService.YearlyRevenueGrowthCheck(companyGrowthNotes, financialIndicators);
            score += CompanyGrowthAnalyticsService.QuarterlyRevenueGrowthCheck(companyGrowthNotes, financialIndicators);
            score += CompanyGrowthAnalyticsService.YearlyProfitGrowthCheck(companyGrowthNotes, financialIndicators);
            score += CompanyGrowthAnalyticsService.QuarterlyProfitGrowthCheck(companyGrowthNotes, financialIndicators);
            score += CompanyGrowthAnalyticsService.AssetCheck(companyGrowthNotes, financialGrowths);
            score += CompanyGrowthAnalyticsService.OwnerCapitalCheck(companyGrowthNotes, financialGrowths);
            score += CompanyGrowthAnalyticsService.DividendCheck(companyGrowthNotes, financialGrowths);
            score += CompanyGrowthAnalyticsService.YearlyEpsGrowthCheck(companyGrowthNotes, financialIndicators);
            score += CompanyGrowthAnalyticsService.QuarterlyEpsGrowthCheck(companyGrowthNotes, financialIndicators);
            score += CompanyGrowthAnalyticsService.FiinGrowthCheck(companyGrowthNotes, fiinEvaluates);
            if (company.SubsectorCode != "8355")//Không phải là ngân hàng thì đánh giá của vnd có giá trị
            {
                score += CompanyGrowthAnalyticsService.VndGrowthCheck(companyGrowthNotes, vndScores);
            }

            var saveZipData = _zipHelper.ZipByte(JsonSerializer.SerializeToUtf8Bytes(companyGrowthNotes));
            var check = await _analyticsResultData.SaveCompanyGrowthScoreAsync(symbol, score, saveZipData);
            return await _keyValueData.SetAsync(checkingKey, check);
        }

        /// <summary>
        /// Phân tích các chỉ báo, yếu tố kỹ thuật
        /// </summary>
        /// <param name="symbol">mã chứng khoán</param>
        /// <returns>bool</returns>
        public virtual async Task<bool> StockPriceTechnicalAnalyticsAsync(string symbol)
        {
            Guard.Against.NullOrEmpty(symbol, nameof(symbol));
            var checkingKey = $"{symbol}-Analytics-StockPrice";
            var chartPrices = await _chartPriceData.CacheFindAllAsync(symbol, "D");
            if (chartPrices is null || chartPrices.Count <= 0)
            {
                var stockPrices = await _stockPriceData.FindAllForTradingAsync(symbol);
                if (stockPrices is not null && stockPrices.Count > 0)
                {
                    chartPrices = stockPrices.Select(q => q.ToChartPrice()).ToList();
                }
            }
            var stock = await _stockData.FindBySymbolAsync(symbol);
            if (stock is null || chartPrices is null || chartPrices.Count <= 0)
            {
                _logger.LogWarning("StockPriceTechnicalAnalyticsAsync can't find last stock price, chart price and stock with symbol: {symbol}", symbol);
                return await _keyValueData.SetAsync(checkingKey, false);
            }
            var fiinEvaluate = await _fiinEvaluatedData.FindAsync(symbol);
            var quotes = chartPrices.Select(q => q.ToQuote()).ToList();

            var score = 0;
            var stockNotes = new List<AnalyticsNote>();
            var (_, StochRsiScore) = StockTechnicalAnalytics.StochRsiAnalytics(stockNotes, quotes);

            score += StochRsiScore;
            score += StockTechnicalAnalytics.FiinCheck(stockNotes, fiinEvaluate);

            var saveZipData = _zipHelper.ZipByte(JsonSerializer.SerializeToUtf8Bytes(stockNotes));
            var check = await _analyticsResultData.SaveStockScoreAsync(symbol, score, saveZipData);
            return await _keyValueData.SetAsync(checkingKey, check);
        }

        /// <summary>
        /// Phân tích đánh giá cổ phiếu của fiintrading
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <returns></returns>
        public virtual async Task<bool> FiinAnalyticsAsync(string symbol)
        {
            var score = 0;
            var type = 0;
            var notes = new List<AnalyticsNote>();
            var checkingKey = $"{symbol}-Analytics-FiinAnalytics";
            var fiinEvaluate = await _fiinEvaluatedData.FindAsync(symbol);
            var stockPrice = await _stockPriceData.GetLastAsync(symbol);
            if (fiinEvaluate is null || stockPrice is null)
            {
                _logger.LogWarning("FiinAnalyticsAsync can't find fiin evaluate with symbol: {symbol}", symbol);
                return await _keyValueData.SetAsync(checkingKey, false);
            }

            if (fiinEvaluate.ControlStatusCode >= 0)
            {
                score -= 5;
                notes.Add($"Fiin báo trạng thái giao dịch cổ phiếu {fiinEvaluate.ControlStatusName}. ", -2, -2);
            }
            else
            {
                switch (fiinEvaluate.Vgm)
                {
                    case "A":
                        score += 2;
                        type = 2;
                        break;
                    case "B":
                        score++;
                        type = 1;
                        break;
                    case "C":
                        break;
                    case "D":
                        score--;
                        type = -1;
                        break;
                    case "E":
                        score -= 2;
                        type = -1;
                        break;
                    case "F":
                        score -= 2;
                        type = -2;
                        break;
                }
                notes.Add($"Fiin đánh giá giá trị của cổ phiếu điểm {fiinEvaluate.Vgm}, ", -2, type);
            }
            var saveZipData = _zipHelper.ZipByte(JsonSerializer.SerializeToUtf8Bytes(notes));
            var check = await _analyticsResultData.SaveFiinScoreAsync(symbol, score, saveZipData);
            return await _keyValueData.SetAsync(checkingKey, check);
        }

        /// <summary>
        /// Phân tích đánh giá của vnd
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <returns></returns>
        public virtual async Task<bool> VndScoreAnalyticsAsync(string symbol)
        {
            var score = 0;
            var type = 0;
            var notes = new List<AnalyticsNote>();
            var checkingKey = $"{symbol}-Analytics-VndScore";
            var vndScores = await _vndStockScoreData.FindAllAsync(symbol);
            var company = await _companyData.FindBySymbolAsync(symbol);
            var stockPrice = await _stockPriceData.GetLastAsync(symbol);
            if (vndScores is null || stockPrice is null)
            {
                _logger.LogWarning("VndScoreAnalyticsAsync can't find vnd score with symbol: {symbol}", symbol);
                return await _keyValueData.SetAsync(checkingKey, false);
            }

            if (company is null || company.SubsectorCode == "8355")
            {
                notes.Add("Không xử lý phân tích của vnd.", 0, 0);
            }
            else
            {
                if (vndScores.Count <= 0)
                {
                    _logger.LogWarning("VndScoreAnalyticsAsync can't find vnd scores with symbol: {symbol}", symbol);
                    notes.Add("Không tim thấy dữ liệu phân tích của vnd.", -2, -2);
                    score = -2;
                }
                else
                {
                    var avgScore = vndScores.Where(q => q.CriteriaCode != "105000").Average(q => q.Point);
                    if (avgScore >= 8)
                    {
                        score += 2;
                        type = 2;
                    }
                    else if (avgScore >= 6)
                    {
                        score++;
                        type++;
                    }
                    else if (avgScore <= 4)
                    {
                        score--;
                        type--;
                    }
                    else if (avgScore <= 2)
                    {
                        score -= 2;
                        type = -2;
                    }
                    notes.Add($"Vnd trung bình đánh giá các tiêu chí tốc độ tăng trưởng, năng lực sinh lời là {avgScore:0.0}", score, type);
                }
            }

            var saveZipData = _zipHelper.ZipByte(JsonSerializer.SerializeToUtf8Bytes(notes));
            var check = await _analyticsResultData.SaveVndScoreAsync(symbol, score, saveZipData);
            return await _keyValueData.SetAsync(checkingKey, check);
        }

        /// <summary>
        /// Phân tích đánh giá của vnd
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <returns></returns>
        public virtual async Task<bool> TargetPriceAnalyticsAsync(string symbol)
        {
            var targetPrice = 0f;
            var notes = new List<AnalyticsNote>();
            var checkingKey = $"{symbol}-Analytics-TargetPrice";
            var lastStockPrice = await _stockPriceData.GetLastAsync(symbol);
            if (lastStockPrice is null)
            {
                _logger.LogWarning("TargetPriceAnalyticsAsync can't get last stock price to check: {symbol}", symbol);
                return await _keyValueData.SetAsync(checkingKey, false);
            }
            var stockRecommendations = await _stockRecommendationData.GetTopReportInSixMonthAsync(symbol, 5);
            if (stockRecommendations.Count <= 0)
            {
                notes.Add($"Cổ phiếu {symbol} không có dự phóng giá của công ty chứng khoán nào trong vòng 6 tháng lại đây.", 0, -1);
            }
            else
            {
                targetPrice = stockRecommendations.Average(q => q.TargetPrice);
                foreach (var stockRecommendation in stockRecommendations)
                {
                    notes.Add($"Báo cáo viên {stockRecommendation.Analyst} của CTCK {stockRecommendation.Firm} dự phóng giá {stockRecommendation.TargetPrice:0,0} ngày {stockRecommendation.ReportDate:dd/MM/yyyy}.", 0, lastStockPrice.ClosePriceAdjusted > stockRecommendation.TargetPrice ? 1 : -1);
                }
            }

            var saveZipData = _zipHelper.ZipByte(JsonSerializer.SerializeToUtf8Bytes(notes));
            var check = await _analyticsResultData.SaveTargetPriceAsync(symbol, targetPrice, saveZipData);
            return await _keyValueData.SetAsync(checkingKey, check);
        }

        /// <summary>
        /// Thực hiện cho giao dịch thử nghiệm trên bộ dũ liệu lịch sử
        /// </summary>
        /// <param name="symbol">Mã cổ phiêu</param>
        /// <returns></returns>
        public virtual async Task<bool> TestTradingAnalyticsAsync(string symbol)
        {
            var checkingKey = $"{symbol}-Analytics-TestTrading";
            var stock = await _stockData.FindBySymbolAsync(symbol);
            var chartPrices = await _chartPriceData.CacheFindAllAsync(symbol, "D");
            if (stock is null || chartPrices is null || chartPrices.Count <= 2)
            {
                _logger.LogWarning("TestTradingAnalyticsAsync => stock is null or chartPrices is null for {symbol}", symbol);
                return await _keyValueData.SetAsync(checkingKey, false);
            }

            chartPrices = chartPrices.OrderBy(q => q.TradingDate).ToList();
            var chartTrading = chartPrices.Where(q => q.TradingDate >= Constants.StartTime).OrderBy(q => q.TradingDate).ToList();
            if (chartTrading.Count <= 0)
            {
                _logger.LogWarning("TestTradingAnalyticsAsync => tradingHistories is null for {symbol}", symbol);
                return await _keyValueData.SetAsync(checkingKey, false);
            }

            #region Buy and wait
            var startPrice = chartTrading[0].ClosePrice;
            var endPrice = chartTrading[^1].ClosePrice;
            var noteInvestment = $"Mua và nắm giữ {chartTrading.Count} phiên từ ngày {chartTrading[0].TradingDate:dd/MM/yyyy}: Giá đóng cửa đầu kỳ {startPrice:0,0} giá đóng cửa cuối kỳ {endPrice:0,0} lợi nhuận {endPrice.GetPercent(startPrice):0.00}%.";
            var buyAndWaitCase = new TradingCase();
            buyAndWaitCase.AddNote(startPrice > endPrice ? -1 : startPrice < endPrice ? 1 : 0, noteInvestment);
            var buyAndWaitResult = new TradingResult()
            {
                Symbol = symbol,
                Principle = 3,
                IsBuy = false,
                IsSell = false,
                BuyPrice = chartTrading[^1].ClosePrice,
                SellPrice = chartTrading[^1].ClosePrice,
                FixedCapital = 100000000,
                Profit = (100000000 / startPrice) * endPrice,
                TotalTax = 0,
                AssetPosition = "100% C",
                LoseNumber = startPrice <= endPrice ? 1 : 0,
                WinNumber = startPrice > endPrice ? 1 : 0,
                TradingNotes = _zipHelper.ZipByte(JsonSerializer.SerializeToUtf8Bytes(buyAndWaitCase)),
            };
            await _tradingResultData.SaveTestTradingResultAsync(buyAndWaitResult);
            #endregion

            #region Ngắn hạn
            var shortTrading = new SmaTrading(chartPrices, 6, 23);
            var tradingHistory = chartPrices.Where(q => q.TradingDate < Constants.StartTime).OrderBy(q => q.TradingDate).ToList();
            var shortCase = shortTrading.Trading(chartTrading, tradingHistory, stock.Exchange);
            var shortResult = new TradingResult()
            {
                Symbol = symbol,
                Principle = 0,
                IsBuy = shortCase.IsBuy,
                IsSell = shortCase.IsSell,
                BuyPrice = shortCase.BuyPrice,
                SellPrice = shortCase.SellPrice,
                FixedCapital = shortCase.FixedCapital,
                Profit = shortCase.Profit(chartTrading[^1].ClosePrice),
                TotalTax = shortCase.TotalTax,
                AssetPosition = shortCase.AssetPosition,
                LoseNumber = shortCase.LoseNumber,
                WinNumber = shortCase.WinNumber,
                TradingNotes = _zipHelper.ZipByte(JsonSerializer.SerializeToUtf8Bytes(shortCase)),
            };
            await _tradingResultData.SaveTestTradingResultAsync(shortResult);
            #endregion

            #region Trung hạn
            var midTrading = new SmaTrading(chartPrices, 12, 36);
            tradingHistory = chartPrices.Where(q => q.TradingDate < Constants.StartTime).OrderBy(q => q.TradingDate).ToList();
            var midCase = midTrading.Trading(chartTrading, tradingHistory, stock.Exchange);
            var midResult = new TradingResult()
            {
                Symbol = symbol,
                Principle = 1,
                IsBuy = midCase.IsBuy,
                IsSell = midCase.IsSell,
                BuyPrice = midCase.BuyPrice,
                SellPrice = midCase.SellPrice,
                FixedCapital = midCase.FixedCapital,
                Profit = midCase.Profit(chartTrading[^1].ClosePrice),
                TotalTax = midCase.TotalTax,
                AssetPosition = midCase.AssetPosition,
                LoseNumber = midCase.LoseNumber,
                WinNumber = midCase.WinNumber,
                TradingNotes = _zipHelper.ZipByte(JsonSerializer.SerializeToUtf8Bytes(midCase)),
            };
            await _tradingResultData.SaveTestTradingResultAsync(midResult);
            #endregion

            #region Thử nghiệm
            var experimentTrading = new EmaTrading(chartPrices);
            tradingHistory = chartPrices.Where(q => q.TradingDate < Constants.StartTime).OrderBy(q => q.TradingDate).ToList();
            var experimentCase = experimentTrading.Trading(chartTrading, tradingHistory, stock.Exchange);
            var smaPSarResult = new TradingResult()
            {
                Symbol = symbol,
                Principle = 2,
                IsBuy = experimentCase.IsBuy,
                IsSell = experimentCase.IsSell,
                BuyPrice = experimentCase.BuyPrice,
                SellPrice = experimentCase.SellPrice,
                FixedCapital = experimentCase.FixedCapital,
                Profit = experimentCase.Profit(chartTrading[^1].ClosePrice),
                TotalTax = experimentCase.TotalTax,
                AssetPosition = experimentCase.AssetPosition,
                LoseNumber = experimentCase.LoseNumber,
                WinNumber = experimentCase.WinNumber,
                TradingNotes = _zipHelper.ZipByte(JsonSerializer.SerializeToUtf8Bytes(experimentCase)),
            };
            await _tradingResultData.SaveTestTradingResultAsync(smaPSarResult);
            #endregion            

            return await _keyValueData.SetAsync(checkingKey, true);
        }

        /// <summary>
        /// Phân tích thị trường
        /// </summary>
        /// <param name="schedule">Thông tin lịch làm việc</param>
        /// <returns></returns>
        public virtual async Task MarketSentimentAnalyticsAsync(Schedule schedule)
        {
            Guard.Against.NullOrEmpty(schedule.DataKey, nameof(schedule.DataKey));
            var key = $"{schedule.DataKey}-Analytics-MarketSentiment";
            var indexPrices = await _chartPriceData.FindAllAsync(schedule.DataKey, "D", DateTime.Now.Date.AddDays(-512));
            if (indexPrices.Count < 5)
            {
                _logger.LogWarning("Chỉ số {DataKey} không được phân tích tâm lý do không đủ dữ liệu để phân tích.", schedule.DataKey);
                await _keyValueData.SetAsync(key, -1);
                return;
            }
            var indicatorSet = BaseTrading.BuildIndicatorSet(indexPrices);

            var score = 0;
            var marketSentimentNotes = new List<AnalyticsNote>();
            score += MarketAnalyticsService.IndexValueTrend(marketSentimentNotes, indexPrices, indicatorSet);

            await _keyValueData.SetAsync(key, score);
        }

        /// <summary>
        /// Phân tích dòng tiền theo ngành
        /// </summary>
        /// <param name="workerMessageQueueService">Message queue service</param>
        /// <returns></returns>
        public virtual async Task IndustryTrendAnalyticsAsync()
        {
            var industries = await _industryData.FindAllAsync();
            var oneChange = new List<float>();
            var fiveChange = new List<float>();
            var tenChange = new List<float>();
            var thirtyChange = new List<float>();
            foreach (var industry in industries)
            {
                var stocks = await _companyData.FindAllAsync(industry.Code);
                foreach (var stock in stocks)
                {
                    var stockPrices = await _stockPriceData.GetForIndustryTrendAnalyticsAsync(stock.Symbol, 35);
                    var chartPrices = await _chartPriceData.CacheFindAllAsync(stock.Symbol);
                    if ((chartPrices is null || chartPrices.Count <= 0) && stockPrices is not null && stockPrices.Count > 0)
                    {
                        chartPrices = stockPrices.Select(q => q.ToChartPrice()).ToList();
                    }
                    if (chartPrices is null || chartPrices.Count <= 0)
                    {
                        continue;
                    }
                    if (chartPrices[0].TotalMatchVol > 10000)
                    {
                        if (chartPrices.Count > 2)
                        {
                            oneChange.Add(chartPrices[0].ClosePrice.GetPercent(chartPrices[1].ClosePrice));
                        }
                        else
                        {
                            oneChange.Add(0);
                        }
                        if (chartPrices.Count > 5)
                        {
                            fiveChange.Add(chartPrices[0].ClosePrice.GetPercent(chartPrices[4].ClosePrice));
                        }
                        else
                        {
                            fiveChange.Add(0);
                        }
                        if (chartPrices.Count > 10)
                        {
                            tenChange.Add(chartPrices[0].ClosePrice.GetPercent(chartPrices[9].ClosePrice));
                        }
                        else
                        {
                            tenChange.Add(0);
                        }
                        if (chartPrices.Count > 30)
                        {
                            thirtyChange.Add(chartPrices[0].ClosePrice.GetPercent(chartPrices[29].ClosePrice));
                        }
                        else
                        {
                            thirtyChange.Add(0);
                        }
                    }
                }
                var score = 0;
                if (oneChange.Count > 1)
                {
                    score += oneChange.Count > 0 ? (int)oneChange.Average() * 10 : 0;
                    score += fiveChange.Count > 0 ? (int)fiveChange.Average() * 5 : 0;
                    score += tenChange.Count > 0 ? (int)tenChange.Average() : 3;
                    score += thirtyChange.Count > 0 ? (int)(thirtyChange.Average() * 1) : 0;
                }

                industry.AutoRank = score;
                await _industryData.UpdateAsync(industry);
            }
            _workerQueueService.BroadcastUpdateMemoryTask(new QueueMessage("Industries"));
        }

        /// <summary>
        /// Xử lý index lại dữ liệu lịch sử giá khi ngày giao dịch không hưởng quyền xuất hiện
        /// </summary>
        /// <param name="workerMessageQueueService">Message queue service</param>
        /// <returns></returns>
        public virtual async Task CorporateActionToPriceAsync()
        {
            var exrightCorporateActions = await _corporateActionData.GetTradingByExrightDateAsync();
            foreach (var corporateActions in exrightCorporateActions)
            {
                var schedule = await _scheduleData.FindAsync(5, corporateActions.Symbol);
                if (schedule is not null)
                {
                    await _stockPriceData.DeleteBySymbolAsync(corporateActions.Symbol);
                    schedule.OptionsJson = JsonSerializer.Serialize(new Dictionary<string, string>() { { "StockPricesCrawlSize", "90000" } });
                    schedule.ActiveTime = DateTime.Now;
                    var updateResult = await _scheduleData.UpdateAsync(schedule);
                    if (updateResult)
                    {
                        _workerQueueService.BroadcastUpdateMemoryTask(new QueueMessage("Schedule"));
                    }
                }
            }
        }

        /// <summary>
        /// Phân tích các yếu tố vĩ mô tác động đến giá cổ phiếu
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <returns></returns>
        public virtual async Task<bool> MarketAnalyticsAsync(string symbol)
        {
            var stockPrice = await _stockPriceData.GetLastAsync(symbol);
            var company = await _companyData.FindBySymbolAsync(symbol);
            var checkingKey = $"{symbol}-Analytics-Macroeconomics";
            if (stockPrice is null || company is null)
            {
                _logger.LogWarning("MacroeconomicsAnalyticsAsync can't find last stock price and company with symbol: {symbol}", symbol);
                return await _keyValueData.SetAsync(checkingKey, false);
            }

            var indexCode = Utilities.GetLeadIndexByExchange(company.Exchange ?? "VNINDEX");
            var marketSentimentIndices = await _keyValueData.GetAsync($"{indexCode}-Analytics-MarketSentiment");
            var industry = await _industryData.GetByCodeAsync(company.SubsectorCode);
            var score = 0;
            var analyticsNotes = new List<AnalyticsNote>();
            score += MarketAnalyticsService.MarketSentimentTrend(analyticsNotes, marketSentimentIndices?.GetValue<float>() ?? 0);
            score += MarketAnalyticsService.IndustryTrend(analyticsNotes, industry);

            var saveZipData = _zipHelper.ZipByte(JsonSerializer.SerializeToUtf8Bytes(analyticsNotes));
            var check = await _analyticsResultData.SaveMarketScoreAsync(symbol, score, saveZipData);
            return await _keyValueData.SetAsync(checkingKey, check);
        }

        /// <summary>
        /// Tìm kiếm đặc trưng của cố phiếu
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <returns></returns>
        public virtual async Task<(TradingCase? Case, StockFeature? Feature)> FindStockFeatureAsync(string symbol)
        {
            DateTime fromDate = new(2010, 1, 1);
            DateTime toDate = new(2023, 1, 1);
            var chartPrices = await _chartPriceData.CacheFindAllAsync(symbol, "D") ?? throw new Exception("ChartPrices is null");
            var stock = await _stockData.FindBySymbolAsync(symbol);
            if (chartPrices is null || chartPrices.Count < 50 || stock is null)
            {
                return (null, null);
            }
            var cacheKey = $"TOCKFEATURE-{symbol}";
            var stockFeature = await _asyncCacheService.GetByKeyAsync<StockFeature>(cacheKey);
            if (stockFeature is null)
            {
                stockFeature = new StockFeature();
            }
            chartPrices = chartPrices.OrderBy(q => q.TradingDate).ToList();
            var chartTrading = chartPrices.Where(q => q.TradingDate >= fromDate && q.TradingDate < toDate).OrderBy(q => q.TradingDate).ToList();
            var tradingHistory = chartPrices.Where(q => q.TradingDate < fromDate).OrderBy(q => q.TradingDate).ToList();
            var winCase = 0;
            var loseCase = 0;
            var tradingCase = new TradingCase();

            var testCaseCount = 0;
            var totalCase = 50 * 50;

            for (int i = 1; i < 50; i++)
            {
                for (int j = 1; j < 50; j++)
                {
                    testCaseCount++;
                    if (i >= j)
                    {
                        continue;
                    }
                    Console.Write($"\r{testCaseCount}/{totalCase} cases.");
                    var trader = new SmaTrading(chartPrices, i, j);
                    tradingHistory = chartPrices.Where(q => q.TradingDate < fromDate).OrderBy(q => q.TradingDate).ToList();
                    var findResult = trader.Trading(chartTrading, tradingHistory, stock.Exchange);
                    if (findResult.Profit(chartTrading[^1].ClosePrice) > tradingCase.FixedCapital)
                    {
                        winCase++;
                    }
                    else
                    {
                        loseCase++;
                    }
                    if (tradingCase.Profit(chartTrading[^1].ClosePrice) < findResult.Profit(chartTrading[^1].ClosePrice))
                    {
                        stockFeature.FastSma = i;
                        stockFeature.SlowSma = j;
                        tradingCase = findResult;
                    }
                }
            }
            stockFeature.SmaWin = winCase;
            stockFeature.SmaLose = loseCase;
            await _asyncCacheService.SetValueAsync(cacheKey, stockFeature);
            return (tradingCase, stockFeature);
        }
    }
}
