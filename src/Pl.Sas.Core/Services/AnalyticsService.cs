using Microsoft.Extensions.Logging;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Core.Trading;
using System.Diagnostics;
using System.Text.Json;

namespace Pl.Sas.Core.Services
{
    public class AnalyticsService
    {
        private readonly IScheduleData _scheduleData;
        private readonly ICompanyData _companyData;
        private readonly IFinancialIndicatorData _financialIndicatorData;
        private readonly IFinancialGrowthData _financialGrowthData;
        private readonly ILogger<AnalyticsService> _logger;
        private readonly IIndustryData _industryData;
        private readonly IStockData _stockData;
        private readonly IStockPriceData _stockPriceData;
        private readonly ICorporateActionData _corporateActionData;
        private readonly IAnalyticsResultData _analyticsResultData;
        private readonly IZipHelper _zipHelper;
        private readonly ITradingResultData _tradingResultData;
        private readonly IOperationRetry _operationRetry;
        private readonly IStockTransactionData _stockTransactionData;
        private readonly IStockFeatureData _stockFeatureData;
        private readonly IEconomicIndexData _economicIndexData;
        private readonly IMemoryCacheService _memoryCacheService;
        private readonly IVndSignalData _vndSignalData;
        private readonly IVndStockScoreData _vndStockScoreData;
        private readonly IFiinEvaluateData _fiinEvaluateData;
        private readonly IStockRecommendationData _stockRecommendationData;

        public AnalyticsService(
            IStockRecommendationData stockRecommendationData,
            IFiinEvaluateData fiinEvaluateData,
            IVndStockScoreData vndStockScoreData,
            IVndSignalData vndSignalData,
            IMemoryCacheService memoryCacheService,
            IEconomicIndexData commonEconomicIndexData,
            IStockFeatureData stockFeatureData,
            IStockTransactionData stockTransactionData,
            IOperationRetry operationRetry,
            ITradingResultData tradingResultData,
            IZipHelper zipHelper,
            IAnalyticsResultData analyticsResultData,
            ICorporateActionData corporateActionData,
            IStockPriceData stockPriceData,
            IStockData stockData,
            IFinancialGrowthData financialGrowthData,
            IFinancialIndicatorData financialIndicatorData,
            ICompanyData companyData,
            IIndustryData industryData,
            ILogger<AnalyticsService> logger,
            IScheduleData scheduleData)
        {
            _stockRecommendationData = stockRecommendationData;
            _vndSignalData = vndSignalData;
            _vndStockScoreData = vndStockScoreData;
            _fiinEvaluateData = fiinEvaluateData;
            _operationRetry = operationRetry;
            _zipHelper = zipHelper;
            _analyticsResultData = analyticsResultData;
            _industryData = industryData;
            _companyData = companyData;
            _financialIndicatorData = financialIndicatorData;
            _financialGrowthData = financialGrowthData;
            _scheduleData = scheduleData;
            _logger = logger;
            _stockData = stockData;
            _stockPriceData = stockPriceData;
            _corporateActionData = corporateActionData;
            _tradingResultData = tradingResultData;
            _stockTransactionData = stockTransactionData;
            _stockFeatureData = stockFeatureData;
            _economicIndexData = commonEconomicIndexData;
            _memoryCacheService = memoryCacheService;
        }

        public virtual async Task HandleAnalyticsEventAsync(QueueMessage queueMessage, IWorkerMessageQueueService workerMessageQueueService)
        {
            var schedule = await _operationRetry.Retry(() => _scheduleData.GetByIdAsync(queueMessage.Id), 5, TimeSpan.FromMilliseconds(100));
            if (schedule != null)
            {
                Stopwatch stopWatch = new();
                stopWatch.Start();

                switch (schedule.Type)
                {
                    case 200:
                        _logger.LogInformation("Run company analytics for {DataKey}.", schedule.DataKey);
                        await CompanyValueAnalyticsAsync(schedule.DataKey);
                        break;

                    case 201:
                        _logger.LogInformation("Run stock price analytics for {DataKey}.", schedule.DataKey);
                        await StockPriceAnalyticsAsync(schedule.DataKey);
                        break;

                    case 202:
                        _logger.LogInformation("Run test trading analytics for {DataKey}.", schedule.DataKey);
                        await TestTradingAnalyticsAsync(schedule.DataKey);
                        break;

                    case 203:
                        _logger.LogInformation("Run target price analytics => {DataKey}.", schedule.DataKey);
                        await TargetPriceAnalyticsAsync(schedule.DataKey);
                        break;

                    case 204:
                        _logger.LogInformation("Run industry trend analytics for {Id}.", schedule.Id);
                        await IndustryTrendAnalyticsAsync(workerMessageQueueService);
                        break;

                    case 205:
                        _logger.LogInformation("Run update price on corporate action for {Id}.", schedule.Id);
                        await CorporateActionToPriceAsync(workerMessageQueueService);
                        break;

                    case 207:
                        _logger.LogInformation("Run market sentiment analytics => {DataKey}.", schedule.DataKey);
                        await MarketSentimentAnalyticsAsync(schedule, workerMessageQueueService);
                        break;

                    case 208:
                        _logger.LogInformation("Run macroeconomics analytics => {DataKey}.", schedule.DataKey);
                        await MacroeconomicsAnalyticsAsync(schedule, workerMessageQueueService);
                        break;

                    case 209:
                        _logger.LogInformation("Run company growth analytics => {DataKey}.", schedule.DataKey);
                        await CompanyGrowthAnalyticsAsync(schedule.DataKey);
                        break;

                    case 210:
                        _logger.LogInformation("Run fiintrading analytics => {DataKey}.", schedule.DataKey);
                        await FiinAnalyticsAsync(schedule.DataKey);
                        break;

                    case 211:
                        _logger.LogInformation("Run vnd score analytics => {DataKey}.", schedule.DataKey);
                        await VndScoreAnalyticsAsync(schedule.DataKey);
                        break;

                    default:
                        _logger.LogWarning("Scheduler id {Id}, type: {Type} don't match any function", queueMessage.Id, schedule.Type);
                        stopWatch.Stop();
                        return;
                }

                stopWatch.Stop();
                _logger.LogDebug("Process analytics {Id} type {Type} in {ElapsedMilliseconds} miniseconds.", schedule.Id, schedule.Type, stopWatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogWarning("HandleAnalyticsEventAsync null scheduler Id: {Id}.", queueMessage.Id);
            }
        }

        /// <summary>
        /// Phân tích các yếu tố vĩ mô tác động đến giá cổ phiếu
        /// </summary>
        /// <param name="workerMessageQueueService">message queue service</param>
        /// <param name="schedule">Thông tin lịch làm việc</param>
        /// <returns></returns>
        public virtual async Task MacroeconomicsAnalyticsAsync(Schedule schedule, IWorkerMessageQueueService workerMessageQueueService)
        {
            var company = await _companyData.GetByCodeAsync(schedule.DataKey);
            if (company is null)
            {
                _logger.LogWarning("MacroeconomicsAnalyticsAsync can't find company with symbol: {DataKey}", schedule.DataKey);
                return;
            }

            var indexCode = Utilities.GetLeadIndexByExchange(company.Exchange);
            //var warIndices = await _economicIndexData.FindAllAsync(2, Constants.WarIndexKey, null);
            var diseaseIndices = await _economicIndexData.FindAllAsync(2, Constants.DiseaseIndexKey, null);
            //var disastersIndices = await _economicIndexData.FindAllAsync(2, Constants.DisastersIndeKey, null);
            var institutionsIndices = await _economicIndexData.FindAllAsync(2, Constants.InstitutionsPolicyIndexKey, null);
            var marketSentimentIndices = await _economicIndexData.FindAllAsync(10, $"{Constants.MarketSentimentIndexKey}-{indexCode}", null);
            var industry = await _industryData.GetByCodeAsync(company.SubsectorCode);
            var score = 0;
            var analyticsNotes = new List<AnalyticsMessage>();
            //score += MacroeconomicsAnalyticsService.WarIndexTrend(analyticsNotes, company, warIndices.ToList());
            score += MacroeconomicsAnalyticsService.DiseaseIndexTrend(analyticsNotes, company, diseaseIndices.ToList());
            score += MacroeconomicsAnalyticsService.MarketSentimentTrend(analyticsNotes, marketSentimentIndices.ToList());
            //score += MacroeconomicsAnalyticsService.DisastersIndexTrend(analyticsNotes, company, disastersIndices.ToList());
            score += MacroeconomicsAnalyticsService.InstitutionsIndexTrend(analyticsNotes, company, institutionsIndices.ToList());
            //score += MacroeconomicsAnalyticsService.MarketSentimentTrend(analyticsNotes, marketSentimentIndices.ToList());
            score += MacroeconomicsAnalyticsService.IndustryTrend(analyticsNotes, industry);

            var saveZipData = _zipHelper.ZipByte(JsonSerializer.SerializeToUtf8Bytes(analyticsNotes));
            var saveResult = await _operationRetry.Retry(() =>
                _analyticsResultData.SaveMacroeconomicsScoreAsync(schedule.DataKey, Utilities.GetTradingDatePath(), score, saveZipData), 10, TimeSpan.FromMilliseconds(100));
            if (!saveResult)
            {
                _logger.LogWarning("MacroeconomicsAnalyticsAsync can't save result for symbol: {DataKey}", schedule.DataKey);
            }
        }

        /// <summary>
        /// Phân tích giá trị doanh nghiệp
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <returns></returns>
        public virtual async Task CompanyValueAnalyticsAsync(string symbol)
        {
            var company = await _companyData.GetByCodeAsync(symbol);
            if (company is null)
            {
                _logger.LogWarning("CompanyValueAnalyticsAsync can't find company with symbol: {symbol}", symbol);
                return;
            }

            var score = 0;
            var companyValueNotes = new List<AnalyticsMessage>();

            var tradingPath = Utilities.GetTradingDatePath();
            var companies = await _companyData.FindAllForAnalyticsAsync();
            var companiesSameIndustries = companies.Where(q => q.SubsectorCode == company.SubsectorCode).ToList();
            var financialIndicators = await CacheGetFinancialIndicatorByIndustriesAsync(company.SubsectorCode, companiesSameIndustries, 5);
            var fiinEvaluates = await _fiinEvaluateData.FindAsync(symbol, tradingPath);
            var vndScores = await _vndStockScoreData.FindAllAsync(symbol, tradingPath);
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
                .Select(q => q.OrderByDescending(c => c.YearReport).FirstOrDefault())
                .Where(q => q is not null)
                .ToList();
            var lastQuarterlyFinancialIndicatorSameIndustries = financialIndicators
                .Where(q => companiesSameIndustries.Any(c => c.Symbol == q.Symbol))
                .GroupBy(q => q.Symbol)
                .Select(q => q.OrderByDescending(c => c.YearReport).ThenByDescending(q => q.LengthReport).FirstOrDefault())
                .Where(q => q is not null)
                .ToList();


            var stockPrice = await _stockPriceData.GetLastAsync(symbol);
            if (stockPrice is null)
            {
                _logger.LogWarning("CompanyValueAnalyticsAsync can't find last stock price with symbol: {symbol}", symbol);
                return;
            }
            var industry = await _industryData.GetByCodeAsync(company.SubsectorCode);
            var bankInterestRate6 = await _economicIndexData.GetByKeyAsync(Constants.BankInterestRate6Key, tradingPath);
            var bankInterestRate12 = await _economicIndexData.GetByKeyAsync(Constants.BankInterestRate12Key, tradingPath);
            if (stockPrice?.DatePath != tradingPath || !(bankInterestRate6?.Value > 0) || !(bankInterestRate12?.Value > 0))
            {
                _logger.LogWarning("CompanyValueAnalyticsAsync abnormal data detection with symbol: {symbol}", symbol);
            }
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
            score += CompanyValueAnalyticsService.EpLastQuarterlyCheck(companyValueNotes, lastQuarterlyFinancialIndicator, stockPrice.ClosePriceAdjusted, bankInterestRate6?.Value ?? 6.8f);
            score += CompanyValueAnalyticsService.EpLastYearlyCheck(companyValueNotes, lastYearlyFinancialIndicator, stockPrice.ClosePriceAdjusted, bankInterestRate12?.Value ?? 6.8f);
            score += CompanyValueAnalyticsService.MarketCapCheck(companyValueNotes, company, companiesSameIndustries, companies.Average(q => q.MarketCap));
            score += CompanyValueAnalyticsService.DividendDivCheck(companyValueNotes, bankInterestRate12?.Value ?? 6.8f, dividendYield);
            score += CompanyValueAnalyticsService.PbCheck(companyValueNotes, company, companiesSameIndustries, companies);
            score += CompanyValueAnalyticsService.GrossProfitMarginCheck(companyValueNotes, lastQuarterlyFinancialIndicator, bankInterestRate12?.Value ?? 6.8f);
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
            var saveResult = await _operationRetry.Retry(() =>
                _analyticsResultData.SaveCompanyValueScoreAsync(symbol, tradingPath, score, saveZipData), 10, TimeSpan.FromMilliseconds(100));
            if (!saveResult)
            {
                _logger.LogWarning("MacroeconomicsAnalyticsAsync can't save result for symbol: {symbol}", symbol);
            }
        }

        /// <summary>
        /// Phân tích tăng trưởng giá trị doanh nghiệp
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <returns></returns>
        public virtual async Task CompanyGrowthAnalyticsAsync(string symbol)
        {
            var company = await _companyData.GetByCodeAsync(symbol);
            if (company is null)
            {
                _logger.LogWarning("CompanyGrowthAnalyticsAsync can't find company with symbol: {symbol}", symbol);
                return;
            }

            var score = 0;
            var companyGrowthNotes = new List<AnalyticsMessage>();

            var tradingPath = Utilities.GetTradingDatePath();
            var companies = await _companyData.FindAllForAnalyticsAsync();
            var companiesSameIndustries = companies.Where(q => q.SubsectorCode == company.SubsectorCode).ToList();
            var financialIndicators = await CacheGetFinancialIndicatorByIndustriesAsync(company.SubsectorCode, companiesSameIndustries, 5);
            var financialGrowths = (await _financialGrowthData.FindAllAsync(company.Symbol)).ToList();
            var fiinEvaluate = await _fiinEvaluateData.FindAsync(symbol, tradingPath);
            var vndScores = await _vndStockScoreData.FindAllAsync(symbol, tradingPath);

            var industry = await _industryData.GetByCodeAsync(company.SubsectorCode);
            var stockPrice = await _stockPriceData.GetLastAsync(symbol);
            if (stockPrice?.DatePath != tradingPath)
            {
                _logger.LogWarning("CompanyGrowthAnalyticsAsync abnormal data detection with symbol: {symbol}", symbol);
            }

            score += CompanyGrowthAnalyticsService.YearlyRevenueGrowthCheck(companyGrowthNotes, company, financialIndicators);
            score += CompanyGrowthAnalyticsService.QuarterlyRevenueGrowthCheck(companyGrowthNotes, company, financialIndicators);
            score += CompanyGrowthAnalyticsService.YearlyProfitGrowthCheck(companyGrowthNotes, company, financialIndicators);
            score += CompanyGrowthAnalyticsService.QuarterlyProfitGrowthCheck(companyGrowthNotes, company, financialIndicators);
            score += CompanyGrowthAnalyticsService.AssetCheck(companyGrowthNotes, financialGrowths);
            score += CompanyGrowthAnalyticsService.OwnerCapitalCheck(companyGrowthNotes, financialGrowths);
            score += CompanyGrowthAnalyticsService.DividendCheck(companyGrowthNotes, financialGrowths);
            score += CompanyGrowthAnalyticsService.YearlyEpsGrowthCheck(companyGrowthNotes, company, financialIndicators);
            score += CompanyGrowthAnalyticsService.QuarterlyEpsGrowthCheck(companyGrowthNotes, company, financialIndicators);
            score += CompanyGrowthAnalyticsService.FiinGrowthCheck(companyGrowthNotes, fiinEvaluate);
            if (company.SubsectorCode != "8355")
            {
                score += CompanyGrowthAnalyticsService.VndGrowthCheck(companyGrowthNotes, vndScores);
            }

            var saveZipData = _zipHelper.ZipByte(JsonSerializer.SerializeToUtf8Bytes(companyGrowthNotes));
            var saveResult = await _operationRetry.Retry(() =>
            _analyticsResultData.SaveCompanyGrowthScoreAsync(symbol, tradingPath, score, saveZipData), 10, TimeSpan.FromMilliseconds(100));
            if (!saveResult)
            {
                _logger.LogWarning("CompanyGrowthAnalyticsAsync can't save result for symbol: {symbol}", symbol);
            }
        }

        /// <summary>
        /// Phân tích tăng trưởng của thị giá, phân tích kỹ thuật
        /// </summary>
        /// <param name="symbol">Mã cổ phiêu</param>
        /// <returns></returns>
        public virtual async Task StockPriceAnalyticsAsync(string symbol)
        {
            var company = await _companyData.GetByCodeAsync(symbol);
            if (company is null)
            {
                _logger.LogWarning("StockPriceAnalyticsAsync => Can't find company or stock or stockPrice is not major 2 with symbol: {symbol}", symbol);
                return;
            }
            var score = 0;
            var stockNotes = new List<AnalyticsMessage>();
            var tradingPath = Utilities.GetTradingDatePath();

            var stock = await _operationRetry.Retry(() => _stockData.GetByCodeAsync(symbol), 10, TimeSpan.FromMilliseconds(100));
            var stockPrices = await _operationRetry.Retry(() =>
                _stockPriceData.FindAllAsync(company.Symbol, 99999, Constants.StartTime), 10, TimeSpan.FromMilliseconds(100));
            if (stockPrices?.Count < 2)
            {
                _logger.LogWarning("StockPriceAnalyticsAsync => Can't get any stock prices to ananytics symbol {symbol}.", symbol);
                return;
            }
            if (stockPrices[0].DatePath != tradingPath)
            {
                _logger.LogWarning("StockPriceAnalyticsAsync => abnormal data detection with symbol: {symbol}", symbol);
                score = -1;
            }

            var tradingHistories = BaseTrading.ConvertStockPricesToStockPriceAdj(stockPrices.Where(q => q.ClosePrice > 0 && q.ClosePriceAdjusted > 0).OrderByDescending(q => q.TradingDate));
            var exchangeFluctuationsRate = BaseTrading.GetExchangeFluctuationsRate(stock.Exchange);
            var indicatorSet = BaseTrading.BuildIndicatorSet(tradingHistories);
            var vndSignalShort = await _vndSignalData.FindAsync(symbol, "cipShort");
            var vndSignalLong = await _vndSignalData.FindAsync(symbol, "cipLong");
            var fiinEvaluate = await _fiinEvaluateData.FindAsync(symbol, tradingPath);

            score += StockAnalyticsService.LastTradingAnalytics(stockNotes, tradingHistories, exchangeFluctuationsRate);
            //score += StockAnalyticsService.StochasticTrend(stockNotes, indicatorSet);
            //score += StockAnalyticsService.EmaTrend(stockNotes, indicatorSet, tradingHistories[0]);
            //score += StockAnalyticsService.PriceTrend(stockNotes, tradingHistories);
            score += StockAnalyticsService.MatchVolCheck(stockNotes, stockPrices.Take(5).ToList());
            //score += StockAnalyticsService.MatchVolCheck(stockNotes, stockPrices.Take(30).ToList());
            //score += StockAnalyticsService.MatchVolTrend(stockNotes, stockPrices);
            score += StockAnalyticsService.ForeignPurchasingPowerTrend(stockNotes, stockPrices.Take(5).ToList());
            //score += StockAnalyticsService.TraderTrend(stockNotes, stockPrices);
            score += StockAnalyticsService.VndCheck(stockNotes, vndSignalShort, vndSignalLong);
            score += StockAnalyticsService.FiinCheck(stockNotes, fiinEvaluate);

            var saveZipData = _zipHelper.ZipByte(JsonSerializer.SerializeToUtf8Bytes(stockNotes));
            var saveResult = await _operationRetry.Retry(() =>
            _analyticsResultData.SaveStockScoreAsync(symbol, tradingPath, score, saveZipData), 10, TimeSpan.FromMilliseconds(100));
            if (!saveResult)
            {
                _logger.LogWarning("StockPriceAnalyticsAsync can't save result for symbol: {symbol}", symbol);
            }
        }

        /// <summary>
        /// Phân tích đánh giá cổ phiếu của fiintrading
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <returns></returns>
        public virtual async Task FiinAnalyticsAsync(string symbol)
        {
            var score = 0;
            var type = 0;
            var notes = new List<AnalyticsMessage>();
            var tradingPath = Utilities.GetTradingDatePath();
            var fiinEvaluate = await _fiinEvaluateData.FindAsync(symbol, tradingPath);
            if (fiinEvaluate is null)
            {
                _logger.LogWarning("FiinAnalyticsAsync can't find fiin evaluate with symbol: {symbol}", symbol);
                notes.Add("Không tim thấy dữ liệu phân tích của fiintrading.", -2, -2);
                score = -2;
            }
            else
            {
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
            }
            var saveZipData = _zipHelper.ZipByte(JsonSerializer.SerializeToUtf8Bytes(notes));
            var saveResult = await _operationRetry.Retry(() =>
            _analyticsResultData.SaveFiinScoreAsync(symbol, tradingPath, score, saveZipData), 10, TimeSpan.FromMilliseconds(100));
            if (!saveResult)
            {
                _logger.LogWarning("FiinAnalyticsAsync can't save result for symbol: {symbol}", symbol);
            }
        }

        /// <summary>
        /// Phân tích đánh giá của vnd
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <returns></returns>
        public virtual async Task VndScoreAnalyticsAsync(string symbol)
        {
            var score = 0;
            var type = 0;
            var notes = new List<AnalyticsMessage>();
            var tradingPath = Utilities.GetTradingDatePath();
            var vndScores = await _vndStockScoreData.FindAllAsync(symbol, tradingPath);
            var company = await _companyData.GetByCodeAsync(symbol);
            if (company is null || company.SubsectorCode == "8355")
            {
                notes.Add("Không xử lý phân tích của vnd.", 0, 0);
            }
            else
            {
                if (vndScores?.Count <= 0)
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
            var saveResult = await _operationRetry.Retry(() =>
            _analyticsResultData.SaveVndScoreAsync(symbol, tradingPath, score, saveZipData), 10, TimeSpan.FromMilliseconds(100));
            if (!saveResult)
            {
                _logger.LogWarning("VndScoreAnalyticsAsync can't save result for symbol: {symbol}", symbol);
            }
        }

        /// <summary>
        /// Phân tích đánh giá của vnd
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <returns></returns>
        public virtual async Task TargetPriceAnalyticsAsync(string symbol)
        {
            var targetPrice = 0M;
            var notes = new List<AnalyticsMessage>();
            var tradingPath = Utilities.GetTradingDatePath();
            var lastStockPrice = await _stockPriceData.GetLastAsync(symbol);
            if (lastStockPrice is null)
            {
                _logger.LogWarning("TargetPriceAnalyticsAsync can't get last stock price to check.");
                return;
            }
            var stockRecommendations = await _stockRecommendationData.GetTopReportInSixMonthAsync(symbol, 5);
            if (stockRecommendations?.Count <= 0)
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
            var saveResult = await _operationRetry.Retry(() =>
            _analyticsResultData.SaveTargetPriceAsync(symbol, tradingPath, targetPrice, saveZipData), 10, TimeSpan.FromMilliseconds(100));
            if (!saveResult)
            {
                _logger.LogWarning("CompanyGrowthAnalyticsAsync can't save result for symbol: {symbol}", symbol);
            }
        }

        /// <summary>
        /// Thực hiện cho giao dịch thử nghiệm trên bộ dũ liệu lịch sử
        /// </summary>
        /// <param name="symbol">Mã cổ phiêu</param>
        /// <returns></returns>
        public virtual async Task TestTradingAnalyticsAsync(string symbol)
        {
            var stockNotes = new List<AnalyticsMessage>();
            var tradingPath = Utilities.GetTradingDatePath();
            var stock = await _operationRetry.Retry(() => _stockData.GetByCodeAsync(symbol), 10, TimeSpan.FromMilliseconds(100));
            var stockPrices = await _operationRetry.Retry(() => _stockPriceData.FindAllAsync(symbol, 99999, Constants.StartTime), 10, TimeSpan.FromMilliseconds(100));
            if (stock is null || stockPrices?.Count <= 2)
            {
                _logger.LogWarning("TestTradingAnalyticsAsync => stockPrices is null or stock is null for {symbol}", symbol);
                return;
            }
            var tradingHistories = BaseTrading.ConvertStockPricesToStockPriceAdj(stockPrices.Where(q => q.ClosePrice > 0 && q.ClosePriceAdjusted > 0).OrderBy(q => q.TradingDate));
            var exchangeFluctuationsRate = BaseTrading.GetExchangeFluctuationsRate(stock.Exchange);
            var indicatorSet = BaseTrading.BuildIndicatorSet(tradingHistories.OrderByDescending(q => q.TradingDate).ToList());
            var isBuy = false;
            var isSell = false;

            #region Buy and wait

            var startPrice = tradingHistories[0].ClosePrice;
            var endPrice = tradingHistories[^1].ClosePrice;
            var noteInvestment = $"Mua và nắm giữ {tradingHistories.Count} phiên từ ngày {tradingHistories[0].TradingDate:dd/MM/yyyy}: Giá đóng cửa đầu kỳ {startPrice:0,0} giá đóng cửa cuối kỳ {endPrice:0,0} lợi nhuận {endPrice.GetPercent(startPrice):0.00}%.";
            stockNotes.Add(noteInvestment, 0, startPrice > endPrice ? -1 : startPrice < endPrice ? 1 : 0, null);
            await _operationRetry.Retry(() => _tradingResultData.SaveTestTradingResultAsync(new()
            {
                Symbol = symbol,
                Principle = 0,
                IsBuy = false,
                BuyPrice = tradingHistories[^1].ClosePrice,
                IsSell = false,
                SellPrice = tradingHistories[^1].ClosePrice,
                Capital = 10000000,
                Profit = (10000000 / startPrice) * endPrice,
                TotalTax = 0,
                ZipExplainNotes = _zipHelper.ZipByte(JsonSerializer.SerializeToUtf8Bytes(new List<KeyValuePair<int, string>>() { new(startPrice > endPrice ? -1 : startPrice < endPrice ? 1 : 0, noteInvestment) })),
                ProfitPercent = endPrice.GetPercent(startPrice)
            }), 10, TimeSpan.FromMilliseconds(100));

            #endregion Buy and wait

            var tradingCases = AnalyticsBuildTradingCases();
            foreach (var tradingCase in tradingCases)
            {
                (isBuy, isSell) = EmaStochTrading.Trading(tradingCase.Value, tradingHistories, indicatorSet, true);
                var note = $"Đầu tư {Utilities.GetPrincipleName(tradingCase.Key).ToLower()} {tradingHistories.Count} phiên từ ngày {tradingHistories[0].TradingDate:dd/MM/yyyy}, {tradingCase.Value.ResultString()} xem chi tiết tại tab \"Lợi nhuận và đầu tư TN\".";
                var optimalBuyPrice = EmaStochTrading.CalculateOptimalBuyPrice(tradingHistories[^1]);
                var optimalSellPrice = EmaStochTrading.CalculateOptimalSellPriceOnLoss(tradingHistories[^1]);
                var type = tradingCase.Value.FixedCapital < tradingCase.Value.Profit ? 1 : tradingCase.Value.FixedCapital == tradingCase.Value.Profit ? 0 : -1;
                stockNotes.Add(note, 0, type, null);
                await _operationRetry.Retry(() => _tradingResultData.SaveTestTradingResultAsync(new()
                {
                    Symbol = symbol,
                    Principle = tradingCase.Key,
                    IsBuy = isBuy,
                    BuyPrice = optimalBuyPrice,
                    IsSell = isSell,
                    SellPrice = optimalSellPrice,
                    Capital = tradingCase.Value.FixedCapital,
                    Profit = tradingCase.Value.Profit,
                    TotalTax = tradingCase.Value.TotalTax,
                    ZipExplainNotes = _zipHelper.ZipByte(JsonSerializer.SerializeToUtf8Bytes(tradingCase.Value.ExplainNotes)),
                    ProfitPercent = tradingCase.Value.ProfitPercent
                }), 10, TimeSpan.FromMilliseconds(100));
            }
        }

        /// <summary>
        /// Phân tích trạng thái thị trường
        /// </summary>
        /// <param name="workerMessageQueueService">message queue service</param>
        /// <param name="schedule">Thông tin lịch làm việc</param>
        /// <returns></returns>
        public virtual async Task MarketSentimentAnalyticsAsync(Schedule schedule, IWorkerMessageQueueService workerMessageQueueService)
        {
            foreach (var index in Constants.ShareIndex)
            {
                var stockPrices = await _stockPriceData.GetForIMarketSentimentAnalyticsAsync(index, 512);
                if (stockPrices?.Count < 5)
                {
                    _logger.LogWarning("Chỉ số {index} không được phân tích tâm lý do không đủ dữ liệu để phân tích.", index);
                    continue;
                }
                var stockPricesAdj = BaseTrading.ConvertStockPricesToStockPriceAdj(stockPrices);
                var indicatorSet = BaseTrading.BuildIndicatorSet(stockPricesAdj);

                var score = 0;
                var marketSentimentNotes = new List<AnalyticsMessage>();
                score += MarketAnalyticsService.IndexValueTrend(marketSentimentNotes, stockPrices, indicatorSet);
                score += MarketAnalyticsService.MatchVolTrend(marketSentimentNotes, stockPrices);
                var key = $"{Constants.MarketSentimentIndexKey}-{index}";
                var saveItem = new EconomicIndex()
                {
                    Key = key,
                    Value = score,
                    Description = $"Phân tích trạng thái thị trường {index}."
                };
                var checkSave = await _economicIndexData.SaveAsync(saveItem);
                if (checkSave)
                {
                    var updateMemoryMessage = new QueueMessage() { Id = "MarketSentimentIndex" };
                    updateMemoryMessage.KeyValues.Add("Key", key);
                    workerMessageQueueService.BroadcastUpdateMemoryTask(updateMemoryMessage);
                }
                else
                {
                    _logger.LogWarning("MarketSentimentAnalyticsAsync can't SaveAsync for: {Id}, Key: {key}", schedule.Id, key);
                }
            }
        }

        /// <summary>
        /// Phân tích biến động ngành
        /// </summary>
        /// <param name="workerMessageQueueService">Message queue service</param>
        /// <returns></returns>
        public virtual async Task IndustryTrendAnalyticsAsync(IWorkerMessageQueueService workerMessageQueueService)
        {
            var industries = await _industryData.FindAllAsync();
            var oneChange = new List<decimal>();
            var threeChange = new List<decimal>();
            var fiveChange = new List<decimal>();
            var tenChange = new List<decimal>();
            var twentyChange = new List<decimal>();
            var thirtyChange = new List<decimal>();
            foreach (var industry in industries)
            {
                var stocks = await _companyData.FindAllAsync(industry.Code);
                foreach (var stock in stocks)
                {
                    var stockPrices = await _operationRetry.Retry(() => _stockPriceData.GetForIndustryTrendAnalyticsAsync(stock.Symbol, 35), 10, TimeSpan.FromMilliseconds(100));
                    if (stockPrices.Count <= 0)
                    {
                        continue;
                    }
                    if (stockPrices[0].TotalMatchVal > 10000000000)
                    {
                        if (stockPrices.Count > 2)
                        {
                            oneChange.Add(stockPrices[0].ClosePriceAdjusted.GetPercent(stockPrices[1].ClosePriceAdjusted));
                        }
                        else
                        {
                            oneChange.Add(0);
                        }
                        if (stockPrices.Count > 3)
                        {
                            threeChange.Add(stockPrices[0].ClosePriceAdjusted.GetPercent(stockPrices[2].ClosePriceAdjusted));
                        }
                        else
                        {
                            threeChange.Add(0);
                        }
                        if (stockPrices.Count > 5)
                        {
                            fiveChange.Add(stockPrices[0].ClosePriceAdjusted.GetPercent(stockPrices[4].ClosePriceAdjusted));
                        }
                        else
                        {
                            fiveChange.Add(0);
                        }
                        if (stockPrices.Count > 10)
                        {
                            tenChange.Add(stockPrices[0].ClosePriceAdjusted.GetPercent(stockPrices[9].ClosePriceAdjusted));
                        }
                        else
                        {
                            tenChange.Add(0);
                        }
                        if (stockPrices.Count > 20)
                        {
                            twentyChange.Add(stockPrices[0].ClosePriceAdjusted.GetPercent(stockPrices[19].ClosePriceAdjusted));
                        }
                        else
                        {
                            twentyChange.Add(0);
                        }
                        if (stockPrices.Count > 30)
                        {
                            thirtyChange.Add(stockPrices[0].ClosePriceAdjusted.GetPercent(stockPrices[29].ClosePriceAdjusted));
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
                    score += threeChange.Count > 0 ? (int)threeChange.Average() * 6 : 0;
                    score += fiveChange.Count > 0 ? (int)fiveChange.Average() * 5 : 0;
                    score += tenChange.Count > 0 ? (int)tenChange.Average() : 3;
                    score += twentyChange.Count > 0 ? (int)(twentyChange.Average() * 2) : 0;
                    score += thirtyChange.Count > 0 ? (int)(thirtyChange.Average() * 1) : 0;
                }

                industry.AutoRank = score;
                await _industryData.UpdateAsync(industry);
                oneChange.Clear();
                threeChange.Clear();
                fiveChange.Clear();
                tenChange.Clear();
                twentyChange.Clear();
                thirtyChange.Clear();
            }
            var sendUpdateMemory = new QueueMessage() { Id = "Industries" };
            workerMessageQueueService.BroadcastUpdateMemoryTask(sendUpdateMemory);
        }

        /// <summary>
        /// Xử lý index lại dữ liệu lịch sử giá khi ngày giao dịch không hưởng quyền xuất hiện
        /// </summary>
        /// <param name="workerMessageQueueService">Message queue service</param>
        /// <returns></returns>
        public virtual async Task CorporateActionToPriceAsync(IWorkerMessageQueueService workerMessageQueueService)
        {
            var exrightCorporateActions = await _corporateActionData.GetTradingByExrightDateAsync();
            foreach (var corporateActions in exrightCorporateActions)
            {
                var schedule = await _scheduleData.FindAsync(5, corporateActions.Symbol);
                if (schedule is not null)
                {
                    await _stockPriceData.DeleteBySymbolAsync(corporateActions.Symbol);
                    schedule.OptionsJson = JsonSerializer.Serialize(new Dictionary<string, string>());
                    var updateResult = await _scheduleData.UpdateAsync(schedule);
                    if (updateResult)
                    {
                        var sendUpdateMemory = new QueueMessage() { Id = "UpdatedSchedule" };
                        sendUpdateMemory.KeyValues.Add("SchedulerId", schedule.Id);
                        workerMessageQueueService.BroadcastUpdateMemoryTask(sendUpdateMemory);
                    }
                }
            }
        }

        /// <summary>
        /// Xây dựng các trường hợp trading cho phân tích
        /// </summary>
        /// <param name="fixedCapital">Số tiền vốn ban đầu</param>
        /// <returns></returns>
        public static Dictionary<int, TradingCase> AnalyticsBuildTradingCases(decimal fixedCapital = 10000000M)
        {
            var testCases = new Dictionary<int, TradingCase>()
            {
                {1, new()
                    {
                        TotalTax = 0,
                        ExplainNotes = new(),
                        Profit = 0,
                        FixedCapital = fixedCapital,
                        FirstEmaSell = 2,
                        SecondEmaSell = 60,
                        FirstEmaBuy = 2,
                        SecondEmaBuy = 60
                    }
                },
                {2, new()
                    {
                        TotalTax = 0,
                        ExplainNotes = new(),
                        Profit = 0,
                        FixedCapital = fixedCapital,
                        FirstEmaSell = 2,
                        SecondEmaSell = 36,
                        FirstEmaBuy = 2,
                        SecondEmaBuy = 36
                    }
                },
                {3, new()
                    {
                        TotalTax = 0,
                        ExplainNotes = new(),
                        Profit = 0,
                        FixedCapital = fixedCapital,
                        FirstEmaSell = 2,
                        SecondEmaSell = 5,
                        FirstEmaBuy = 2,
                        SecondEmaBuy = 5
                    }
                },
                {4, new()
                    {
                        TotalTax = 0,
                        ExplainNotes = new(),
                        Profit = 0,
                        FixedCapital = fixedCapital,
                        FirstEmaSell = 1,
                        SecondEmaSell = 3,
                        FirstEmaBuy = 1,
                        SecondEmaBuy = 3
                    }
                }
            };
            return testCases;
        }

        #region Private Method

        /// <summary>
        /// Lấy báo cáo tài chính 5 năm các công ty cùng một ngành
        /// </summary>
        /// <param name="industryCode">Mã ngành</param>
        /// <param name="companies">Danh sách các công ty cần lấy báo cáo tài chính</param>
        /// <param name="yearRanger">Độ dài báo cáo khoảng 5 năm</param>
        /// <returns>List FinancialIndicatorReports</returns>
        private async Task<List<FinancialIndicator>> CacheGetFinancialIndicatorByIndustriesAsync(string industryCode, List<Company> companies, int yearRanger = 5)
        {
            GuardClauses.NullOrEmpty(companies, nameof(companies));
            var cacheKey = CacheKeyService.GetFinancialIndicatorByIndustriesCacheKey(industryCode);
            return await _memoryCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                var result = new List<FinancialIndicator>();
                foreach (var company in companies)
                {
                    var financialIndicators = (await _financialIndicatorData.FindAllAsync(company.Symbol))
                        .Where(q => q.YearReport >= DateTime.Now.Year - yearRanger)
                        .OrderBy(q => q.YearReport)
                        .ThenBy(q => q.LengthReport)
                        .ToList();
                    result.AddRange(financialIndicators);
                }
                return result;
            }, 300);
        }

        #endregion Private Method
    }
}