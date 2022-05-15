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
        private readonly IMarketData _marketData;
        private readonly IAnalyticsData _analyticsData;
        private readonly ILogger<AnalyticsService> _logger;
        private readonly IZipHelper _zipHelper;
        private readonly ISystemData _systemData;
        private readonly IWorkerQueueService _workerQueueService;

        public AnalyticsService(
            IWorkerQueueService workerQueueService,
            ISystemData systemData,
            IZipHelper zipHelper,
            IAnalyticsData analyticsData,
            ILogger<AnalyticsService> logger,
            IMarketData marketData)
        {
            _marketData = marketData;
            _logger = logger;
            _analyticsData = analyticsData;
            _zipHelper = zipHelper;
            _systemData = systemData;
            _workerQueueService = workerQueueService;
        }

        public async Task HandleEventAsync(QueueMessage queueMessage)
        {
            var schedule = await _systemData.GetScheduleByIdAsync(queueMessage.Id);
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
                        await StockPriceAnalyticsAsync(schedule.DataKey ?? "");
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
                        await MarketSentimentAnalyticsAsync();
                        break;

                    case 208:
                        _logger.LogInformation("Run macroeconomics analytics => {DataKey}.", schedule.DataKey);
                        await MacroeconomicsAnalyticsAsync(schedule.DataKey ?? "");
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
            var company = await _marketData.GetCompanyAsync(symbol);
            var stockPrice = await _marketData.GetLastStockPriceAsync(symbol);
            if (company is null || stockPrice is null)
            {
                _logger.LogWarning("CompanyValueAnalyticsAsync can't find company and last stock price with symbol: {symbol}", symbol);
                return await _systemData.SetKeyValueAsync(checkingKey, false);
            }

            var score = 0;
            var companyValueNotes = new List<AnalyticsNote>();

            var companies = await _marketData.CacheGetCompaniesAsync();
            var companiesSameIndustries = companies.Where(q => q.SubsectorCode == company.SubsectorCode).ToList();
            var financialIndicators = await _marketData.CacheGetFinancialIndicatorByIndustriesAsync(company.SubsectorCode, companiesSameIndustries, 5);
            var fiinEvaluates = await _marketData.GetFiinEvaluatedAsync(symbol);
            var vndScores = await _marketData.GetVndStockScoreAsync(symbol);
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

            var bankInterestRate6 = await _systemData.GetKeyValueAsync(Constants.BankInterestRate6Key);
            var bankInterestRate12 = await _systemData.GetKeyValueAsync(Constants.BankInterestRate12Key);
            var dividendYield = company.DividendYield;
            if (dividendYield <= 0)
            {
                var financialGrowth = await _marketData.GetLastFinancialGrowthAsync(symbol);
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
            var check = await _analyticsData.SaveCompanyValueScoreAsync(symbol, score, saveZipData);
            return await _systemData.SetKeyValueAsync(checkingKey, check);
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
            var company = await _marketData.GetCompanyAsync(symbol);
            var stockPrice = await _marketData.GetLastStockPriceAsync(symbol);
            if (company is null || stockPrice is null)
            {
                _logger.LogWarning("CompanyGrowthAnalyticsAsync can't find company and last stock price with symbol: {symbol}", symbol);
                return await _systemData.SetKeyValueAsync(checkingKey, false);
            }

            var score = 0;
            var companyGrowthNotes = new List<AnalyticsNote>();

            var companies = await _marketData.CacheGetCompaniesAsync();
            var companiesSameIndustries = companies.Where(q => q.SubsectorCode == company.SubsectorCode).ToList();
            var financialIndicators = await _marketData.CacheGetFinancialIndicatorByIndustriesAsync(company.SubsectorCode, companiesSameIndustries, 5);
            var financialGrowths = await _marketData.GetFinancialGrowthsAsync(company.Symbol);
            var fiinEvaluates = await _marketData.GetFiinEvaluatedAsync(symbol);
            var vndScores = await _marketData.GetVndStockScoreAsync(symbol);

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
            var check = await _analyticsData.SaveCompanyGrowthScoreAsync(symbol, score, saveZipData);
            return await _systemData.SetKeyValueAsync(checkingKey, check);
        }

        /// <summary>
        /// Phân tích tăng trưởng của thị giá, phân tích kỹ thuật
        /// </summary>
        /// <param name="symbol">Mã cổ phiêu</param>
        /// <returns></returns>
        public virtual async Task<bool> StockPriceAnalyticsAsync(string symbol)
        {
            Guard.Against.NullOrEmpty(symbol, nameof(symbol));
            var checkingKey = $"{symbol}-Analytics-StockPrice";
            var stockPrices = await _marketData.GetAnalyticsTopStockPriceAsync(symbol, 10000);
            var stock = await _marketData.GetStockByCode(symbol);
            if (stock is null || stockPrices.Count <= 0)
            {
                _logger.LogWarning("StockPriceAnalyticsAsync can't find last stock price with symbol: {symbol}", symbol);
                return await _systemData.SetKeyValueAsync(checkingKey, false);
            }

            var score = 0;
            var stockNotes = new List<AnalyticsNote>();
            var exchangeFluctuationsRate = BaseTrading.GetExchangeFluctuationsRate(stock.Exchange);
            var fiinEvaluate = await _marketData.GetFiinEvaluatedAsync(symbol);

            score += StockAnalyticsService.LastTradingAnalytics(stockNotes, stockPrices, exchangeFluctuationsRate);
            //score += StockAnalyticsService.StochasticTrend(stockNotes, indicatorSet);
            //score += StockAnalyticsService.EmaTrend(stockNotes, indicatorSet, tradingHistories[0]);
            //score += StockAnalyticsService.PriceTrend(stockNotes, tradingHistories);
            score += StockAnalyticsService.MatchVolCheck(stockNotes, stockPrices.Take(5).ToList());
            //score += StockAnalyticsService.MatchVolCheck(stockNotes, stockPrices.Take(30).ToList());
            //score += StockAnalyticsService.MatchVolTrend(stockNotes, stockPrices);
            score += StockAnalyticsService.ForeignPurchasingPowerTrend(stockNotes, stockPrices.Take(5).ToList());
            //score += StockAnalyticsService.TraderTrend(stockNotes, stockPrices);
            score += StockAnalyticsService.FiinCheck(stockNotes, fiinEvaluate);

            var saveZipData = _zipHelper.ZipByte(JsonSerializer.SerializeToUtf8Bytes(stockNotes));
            var check = await _analyticsData.SaveStockScoreAsync(symbol, score, saveZipData);
            return await _systemData.SetKeyValueAsync(checkingKey, check);
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
            var fiinEvaluate = await _marketData.GetFiinEvaluatedAsync(symbol);
            var stockPrice = await _marketData.GetLastStockPriceAsync(symbol);
            if (fiinEvaluate is null || stockPrice is null)
            {
                _logger.LogWarning("FiinAnalyticsAsync can't find fiin evaluate with symbol: {symbol}", symbol);
                return await _systemData.SetKeyValueAsync(checkingKey, false);
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
            var check = await _analyticsData.SaveFiinScoreAsync(symbol, score, saveZipData);
            return await _systemData.SetKeyValueAsync(checkingKey, check);
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
            var vndScores = await _marketData.GetVndStockScoreAsync(symbol);
            var company = await _marketData.GetCompanyAsync(symbol);
            var stockPrice = await _marketData.GetLastStockPriceAsync(symbol);
            if (vndScores is null || stockPrice is null)
            {
                _logger.LogWarning("VndScoreAnalyticsAsync can't find vnd score with symbol: {symbol}", symbol);
                return await _systemData.SetKeyValueAsync(checkingKey, false);
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
            var check = await _analyticsData.SaveVndScoreAsync(symbol, score, saveZipData);
            return await _systemData.SetKeyValueAsync(checkingKey, check);
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
            var lastStockPrice = await _marketData.GetLastStockPriceAsync(symbol);
            var stockPrice = await _marketData.GetLastStockPriceAsync(symbol);
            if (stockPrice is null || lastStockPrice is null)
            {
                _logger.LogWarning("TargetPriceAnalyticsAsync can't get last stock price to check: {symbol}", symbol);
                return await _systemData.SetKeyValueAsync(checkingKey, false);
            }
            var stockRecommendations = await _marketData.GetTopStockRecommendationInSixMonthAsync(symbol, 5);
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
            var check = await _analyticsData.SaveTargetPriceAsync(symbol, targetPrice, saveZipData);
            return await _systemData.SetKeyValueAsync(checkingKey, check);
        }

        /// <summary>
        /// Thực hiện cho giao dịch thử nghiệm trên bộ dũ liệu lịch sử
        /// </summary>
        /// <param name="symbol">Mã cổ phiêu</param>
        /// <returns></returns>
        public virtual async Task<bool> TestTradingAnalyticsAsync(string symbol)
        {
            var stockNotes = new List<AnalyticsNote>();
            var checkingKey = $"{symbol}-Analytics-TestTrading";
            var stock = await _marketData.GetStockByCode(symbol);
            var stockPrices = (await _marketData.GetAnalyticsTopStockPriceAsync(symbol, 90000)).OrderBy(q => q.TradingDate).ToList();
            if (stock is null || stockPrices.Count <= 2)
            {
                _logger.LogWarning("TestTradingAnalyticsAsync => stockPrices is null or stock is null for {symbol}", symbol);
                return await _systemData.SetKeyValueAsync(checkingKey, false);
            }
            var exchangeFluctuationsRate = BaseTrading.GetExchangeFluctuationsRate(stock.Exchange);
            var indicatorSet = BaseTrading.BuildIndicatorSet(stockPrices.OrderByDescending(q => q.TradingDate).ToList());
            var isBuy = false;
            var isSell = false;

            #region Buy and wait

            var startPrice = stockPrices[0].ClosePrice;
            var endPrice = stockPrices[^1].ClosePrice;
            var noteInvestment = $"Mua và nắm giữ {stockPrices.Count} phiên từ ngày {stockPrices[0].TradingDate:dd/MM/yyyy}: Giá đóng cửa đầu kỳ {startPrice:0,0} giá đóng cửa cuối kỳ {endPrice:0,0} lợi nhuận {endPrice.GetPercent(startPrice):0.00}%.";
            stockNotes.Add(noteInvestment, 0, startPrice > endPrice ? -1 : startPrice < endPrice ? 1 : 0, null);
            await _analyticsData.SaveTestTradingResultAsync(new()
            {
                Symbol = symbol,
                Principle = 0,
                IsBuy = false,
                BuyPrice = stockPrices[^1].ClosePrice,
                IsSell = false,
                SellPrice = stockPrices[^1].ClosePrice,
                Capital = 10000000,
                Profit = (10000000 / startPrice) * endPrice,
                TotalTax = 0,
                TradingNotes = _zipHelper.ZipByte(JsonSerializer.SerializeToUtf8Bytes(new List<KeyValuePair<int, string>>() { new(startPrice > endPrice ? -1 : startPrice < endPrice ? 1 : 0, noteInvestment) })),
            });

            #endregion
            var tradingCases = AnalyticsBuildTradingCases();
            foreach (var tradingCase in tradingCases)
            {
                (isBuy, isSell) = EmaStochTrading.Trading(tradingCase.Value, stockPrices, indicatorSet, true);
                var note = $"Đầu tư {Utilities.GetPrincipleName(tradingCase.Key).ToLower()} {stockPrices.Count} phiên từ ngày {stockPrices[0].TradingDate:dd/MM/yyyy}, {tradingCase.Value.ToString} xem chi tiết tại tab \"Lợi nhuận và đầu tư TN\".";
                var optimalBuyPrice = EmaStochTrading.CalculateOptimalBuyPrice(stockPrices[^1]);
                var optimalSellPrice = EmaStochTrading.CalculateOptimalSellPriceOnLoss(stockPrices[^1]);
                var type = tradingCase.Value.FixedCapital < tradingCase.Value.TradingTestResult ? 1 : tradingCase.Value.FixedCapital == tradingCase.Value.TradingTestResult ? 0 : -1;
                stockNotes.Add(note, 0, type, null);
                await _analyticsData.SaveTestTradingResultAsync(new()
                {
                    Symbol = symbol,
                    Principle = tradingCase.Key,
                    IsBuy = isBuy,
                    BuyPrice = optimalBuyPrice,
                    IsSell = isSell,
                    SellPrice = optimalSellPrice,
                    Capital = tradingCase.Value.FixedCapital,
                    Profit = tradingCase.Value.TradingTestResult,
                    TotalTax = tradingCase.Value.TotalTax,
                    TradingNotes = _zipHelper.ZipByte(JsonSerializer.SerializeToUtf8Bytes(tradingCase.Value.ExplainNotes)),
                });
            }
            return await _systemData.SetKeyValueAsync(checkingKey, true);
        }

        /// <summary>
        /// Phân tích trạng thái thị trường
        /// </summary>
        /// <param name="workerMessageQueueService">message queue service</param>
        /// <param name="schedule">Thông tin lịch làm việc</param>
        /// <returns></returns>
        public virtual async Task MarketSentimentAnalyticsAsync()
        {
            var indexs = await _marketData.GetStockByType("i");
            foreach (var index in indexs)
            {
                var indexPrices = await _marketData.GetAnalyticsTopIndexPriceAsync(index.Symbol, 512);
                if (indexPrices.Count < 5)
                {
                    _logger.LogWarning("Chỉ số {index} không được phân tích tâm lý do không đủ dữ liệu để phân tích.", index);
                    continue;
                }
                var indicatorSet = BaseTrading.BuildIndicatorSet(indexPrices);

                var score = 0;
                var marketSentimentNotes = new List<AnalyticsNote>();
                score += MarketAnalyticsService.IndexValueTrend(marketSentimentNotes, indexPrices, indicatorSet);
                var key = $"{index.Symbol}-Analytics-MarketSentiment";
                await _systemData.SetKeyValueAsync(key, score);
            }
        }

        /// <summary>
        /// Phân tích dòng tiền theo ngành
        /// </summary>
        /// <param name="workerMessageQueueService">Message queue service</param>
        /// <returns></returns>
        public virtual async Task IndustryTrendAnalyticsAsync()
        {
            var industries = await _marketData.GetIndustriesAsync();
            var oneChange = new List<float>();
            var threeChange = new List<float>();
            var fiveChange = new List<float>();
            var tenChange = new List<float>();
            var twentyChange = new List<float>();
            var thirtyChange = new List<float>();
            foreach (var industry in industries)
            {
                var notes = new List<AnalyticsNote>();
                var companies = await _marketData.CacheGetCompaniesAsync(industry.Code);
                foreach (var company in companies)
                {
                    var stockPrices = await _marketData.GetAnalyticsTopStockPriceAsync(company.Symbol, 35);
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
                var score = 0f;
                if (oneChange.Count > 1)
                {
                    score += oneChange.Count > 0 ? oneChange.Average() * 10 : 0;
                    score += threeChange.Count > 0 ? threeChange.Average() * 6 : 0;
                    score += fiveChange.Count > 0 ? fiveChange.Average() * 5 : 0;
                    score += tenChange.Count > 0 ? tenChange.Average() : 3;
                    score += twentyChange.Count > 0 ? twentyChange.Average() * 2 : 0;
                    score += thirtyChange.Count > 0 ? thirtyChange.Average() * 1 : 0;
                }

                var saveZipData = _zipHelper.ZipByte(JsonSerializer.SerializeToUtf8Bytes(notes));
                await _analyticsData.SaveIndustryScoreAsync(industry.Code, score, saveZipData);
                oneChange.Clear();
                threeChange.Clear();
                fiveChange.Clear();
                tenChange.Clear();
                twentyChange.Clear();
                thirtyChange.Clear();
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
            var exrightCorporateActions = await _marketData.GetCorporateActionTradingByExrightDateAsync();
            foreach (var corporateActions in exrightCorporateActions)
            {
                var schedule = await _systemData.GetScheduleAsync(5, corporateActions.Symbol);
                if (schedule is not null)
                {
                    await _marketData.DeleteStockPrices(corporateActions.Symbol);
                    schedule.OptionsJson = JsonSerializer.Serialize(new Dictionary<string, string>() { { "StockPricesCrawlSize", "90000" } });
                    schedule.ActiveTime = DateTime.Now;
                    var updateResult = await _systemData.UpdateScheduleAsync(schedule);
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
        /// <param name="workerMessageQueueService">message queue service</param>
        /// <param name="schedule">Thông tin lịch làm việc</param>
        /// <returns></returns>
        public virtual async Task<bool> MacroeconomicsAnalyticsAsync(string symbol)
        {
            var stockPrice = await _marketData.GetLastStockPriceAsync(symbol);
            var company = await _marketData.GetCompanyAsync(symbol);
            var checkingKey = $"{symbol}-Analytics-Macroeconomics";
            if (stockPrice is null || company is null)
            {
                _logger.LogWarning("MacroeconomicsAnalyticsAsync can't find last stock price and company with symbol: {symbol}", symbol);
                return await _systemData.SetKeyValueAsync(checkingKey, false);
            }

            var indexCode = Utilities.GetLeadIndexByExchange(company.Exchange ?? "VNINDEX");
            var marketSentimentIndices = await _systemData.GetKeyValueAsync($"{indexCode}-Analytics-MarketSentiment");
            var industry = await _analyticsData.GetIndustryAnalyticsAsync(company.SubsectorCode);
            var score = 0;
            var analyticsNotes = new List<AnalyticsNote>();
            score += MarketAnalyticsService.MarketSentimentTrend(analyticsNotes, marketSentimentIndices?.GetValue<float>() ?? 0);
            score += MarketAnalyticsService.IndustryTrend(analyticsNotes, industry);

            var saveZipData = _zipHelper.ZipByte(JsonSerializer.SerializeToUtf8Bytes(analyticsNotes));
            var check = await _analyticsData.SaveMacroeconomicsScoreAsync(symbol, score, saveZipData);
            return await _systemData.SetKeyValueAsync(checkingKey, check);
        }

        /// <summary>
        /// Xây dựng các trường hợp trading cho phân tích
        /// </summary>
        /// <param name="fixedCapital">Số tiền vốn ban đầu</param>
        /// <returns></returns>
        public static Dictionary<int, TradingCaseV3> AnalyticsBuildTradingCases(float fixedCapital = 10000000)
        {
            var testCases = new Dictionary<int, TradingCaseV3>()
            {
                {1, new()
                    {
                        TotalTax = 0,
                        ExplainNotes = new(),
                        TradingTestResult = 0,
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
                        TradingTestResult = 0,
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
                        TradingTestResult = 0,
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
                        TradingTestResult = 0,
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
    }
}
