using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Core.Trading;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Web;

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

        public AnalyticsService(
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
        }

        public async Task HandleEventAsync(QueueMessage queueMessage)
        {
            Stopwatch stopWatch = new();
            stopWatch.Start();

            switch (queueMessage.Id)
            {
                case "CompanyValueAnalytics":
                    await CompanyValueAnalyticsAsync(queueMessage.KeyValues["Symbol"]);
                    break;

                case "CompanyGrowthAnalytics":
                    await CompanyGrowthAnalyticsAsync(queueMessage.KeyValues["Symbol"]);
                    break;

                case "StockPriceAnalytics":
                    await StockPriceAnalyticsAsync(queueMessage.KeyValues["Symbol"]);
                    break;

                case "VndScoreAnalytics":
                    await VndScoreAnalyticsAsync(queueMessage.KeyValues["Symbol"]);
                    break;

                case "FiinAnalytics":
                    await VndScoreAnalyticsAsync(queueMessage.KeyValues["Symbol"]);
                    break;

                case "TradingAnalytics":
                    await VndScoreAnalyticsAsync(queueMessage.KeyValues["Symbol"]);
                    break;

                case "MarketSentimentAnalytics":
                    await MarketSentimentAnalyticsAsync(queueMessage.KeyValues["Symbol"]);
                    break;

                case "IndustryTrendAnalytics":
                    await IndustryTrendAnalyticsAsync(queueMessage.KeyValues["Symbol"]);
                    break;

                case "CorporateActionToPrice":
                    await CorporateActionToPriceAsync(queueMessage.KeyValues["Symbol"]);
                    break;

                default:
                    _logger.LogWarning("Worker analytics id {Id} don't match any function", queueMessage.Id);
                    break;
            }

            stopWatch.Stop();
            _logger.LogInformation("Worker analytics {Id} in {ElapsedMilliseconds} miniseconds.", queueMessage.Id, stopWatch.ElapsedMilliseconds);
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
            var check = await _analyticsData.SaveCompanyValueScoreAsync(symbol, stockPrice.TradingDate, score, saveZipData);
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
            var check = await _analyticsData.SaveCompanyGrowthScoreAsync(symbol, stockPrice.TradingDate, score, saveZipData);
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
            var check = await _analyticsData.SaveStockScoreAsync(symbol, stockPrices[0].TradingDate, score, saveZipData);
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
            var check = await _analyticsData.SaveFiinScoreAsync(symbol, stockPrice.TradingDate, score, saveZipData);
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
            var check = await _analyticsData.SaveVndScoreAsync(symbol, stockPrice.TradingDate, score, saveZipData);
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
            if (stockPrice is null)
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
            var check = await _analyticsData.SaveTargetPriceAsync(symbol, stockPrice.TradingDate, targetPrice, saveZipData);
            return await _systemData.SetKeyValueAsync(checkingKey, check);
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
                var type = tradingCase.Value.FixedCapital < tradingCase.Value.TradingTestResult ? 1 : tradingCase.Value.FixedCapital == tradingCase.Value.TradingTestResult ? 0 : -1;
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
                    Profit = tradingCase.Value.TradingTestResult,
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
        /// Phân tích dòng tiền theo ngành
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
    }
}
