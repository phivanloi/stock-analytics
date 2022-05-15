using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Pl.Sas.Core;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;

namespace Pl.Sas.Infrastructure.Market
{
    /// <summary>
    /// Lớp xử lỹ liệu của market
    /// </summary>
    public class MarketData : IMarketData
    {
        private readonly MarketDbContext _marketDbContext;
        private readonly IMemoryCacheService _memoryCacheService;

        public MarketData(
            IMemoryCacheService memoryCacheService,
            MarketDbContext marketDbContext)
        {
            _marketDbContext = marketDbContext;
            _memoryCacheService = memoryCacheService;
        }

        #region Company
        public virtual async Task<Company?> GetCompanyAsync(string symbol)
        {
            Guard.Against.NullOrEmpty(symbol, nameof(symbol));
            return await _marketDbContext.Companies.FirstOrDefaultAsync(c => c.Symbol == symbol);
        }

        public virtual async Task<bool> SaveCompanyAsync(Company company)
        {
            Guard.Against.Null(company, nameof(company));
            var checkItem = _marketDbContext.Companies.FirstOrDefault(q => q.Symbol == company.Symbol);
            if (checkItem is null)
            {
                _marketDbContext.Companies.Add(company);
            }
            else
            {
                checkItem.SubsectorCode = company.SubsectorCode;
                checkItem.IndustryName = company.IndustryName;
                checkItem.Supersector = company.Supersector;
                checkItem.Sector = company.Sector;
                checkItem.Subsector = company.Subsector;
                checkItem.FoundingDate = company.FoundingDate;
                checkItem.ListingDate = company.ListingDate;
                checkItem.CharterCapital = company.CharterCapital;
                checkItem.NumberOfEmployee = company.NumberOfEmployee;
                checkItem.BankNumberOfBranch = company.BankNumberOfBranch;
                checkItem.CompanyProfile = company.CompanyProfile;
                checkItem.Exchange = company.Exchange;
                checkItem.FirstPrice = company.FirstPrice;
                checkItem.IssueShare = company.IssueShare;
                checkItem.ListedValue = company.ListedValue;
                checkItem.CompanyName = company.CompanyName;
                checkItem.MarketCap = company.MarketCap;
                checkItem.SharesOutStanding = company.SharesOutStanding;
                checkItem.Bv = company.Bv;
                checkItem.Beta = company.Beta;
                checkItem.Eps = company.Eps;
                checkItem.DilutedEps = company.DilutedEps;
                checkItem.Pe = company.Pe;
                checkItem.Pb = company.Pb;
                checkItem.DividendYield = company.DividendYield;
                checkItem.TotalRevenue = company.TotalRevenue;
                checkItem.Profit = company.Profit;
                checkItem.Asset = company.Asset;
                checkItem.Roe = company.Roe;
                checkItem.Roa = company.Roa;
                checkItem.Npl = company.Npl;
                checkItem.FinanciallEverage = company.FinanciallEverage;
                checkItem.UpdatedTime = DateTime.Now;
            }
            return await _marketDbContext.SaveChangesAsync() > 0;
        }

        public virtual async Task<List<Company>> CacheGetCompaniesAsync(string? industryCode = null)
        {
            var cacheKey = $"{Constants.CompanyCachePrefix}-CGC{industryCode}";
            return await _memoryCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                var query = _marketDbContext.Companies.AsNoTracking();
                if (!string.IsNullOrEmpty(industryCode))
                {
                    query = query.Where(q => q.SubsectorCode == industryCode);
                }
                return await query.ToListAsync();
            }, Constants.DefaultCacheTime * 60 * 24);
        }

        public virtual async Task<Company?> CacheGetCompanyByCodeAsync(string symbol)
        {
            var cacheKey = $"{Constants.CompanyCachePrefix}-SY{symbol}";
            return await _memoryCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _marketDbContext.Companies.AsNoTracking().FirstOrDefaultAsync(q => q.Symbol == symbol);
            }, Constants.DefaultCacheTime * 60 * 24);
        }
        #endregion

        #region FinancialIndicator
        public virtual async Task<List<FinancialIndicator>> CacheGetFinancialIndicatorByIndustriesAsync(string industryCode, List<Company> companies, int yearRanger = 5)
        {
            Guard.Against.NullOrEmpty(companies, nameof(companies));
            var cacheKey = $"{Constants.FinancialIndicatorCachePrefix}-INC{industryCode}-YR{yearRanger}";
            return await _memoryCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                var result = new List<FinancialIndicator>();
                foreach (var company in companies)
                {
                    var financialIndicators = await _marketDbContext.FinancialIndicators
                        .Where(q => q.Symbol == company.Symbol && q.YearReport >= DateTime.Now.Year - yearRanger)
                        .OrderBy(q => q.YearReport)
                        .ThenBy(q => q.LengthReport)
                        .ToListAsync();
                    result.AddRange(financialIndicators);
                }
                return result;
            }, Constants.DefaultCacheTime * 60 * 24);
        }

        public virtual async Task<List<FinancialIndicator>> GetFinancialIndicatorsAsync(string symbol)
        {
            return await _marketDbContext.FinancialIndicators.Where(q => q.Symbol == symbol).ToListAsync();
        }

        public virtual async Task<bool> SaveFinancialIndicatorAsync(List<FinancialIndicator> insertItems, List<FinancialIndicator> updateItems)
        {
            if (insertItems.Count > 0)
            {
                _marketDbContext.FinancialIndicators.AddRange(insertItems);
            }
            if (updateItems.Count > 0)
            {
                foreach (var item in updateItems)
                {
                    item.UpdatedTime = DateTime.Now;
                }
            }
            return await _marketDbContext.SaveChangesAsync() > 0;
        }
        #endregion

        #region FiinEvaluated
        public virtual async Task<FiinEvaluated?> GetFiinEvaluatedAsync(string symbol)
        {
            Guard.Against.NullOrEmpty(symbol, nameof(symbol));
            return await _marketDbContext.FiinEvaluates.FirstOrDefaultAsync(c => c.Symbol == symbol);
        }

        public virtual async Task<bool> SaveFiinEvaluateAsync(FiinEvaluated fiinEvaluate)
        {
            Guard.Against.Null(fiinEvaluate, nameof(fiinEvaluate));
            var updateItem = _marketDbContext.FiinEvaluates.FirstOrDefault(q => q.Symbol == fiinEvaluate.Symbol);
            if (updateItem != null)
            {
                updateItem.IcbRank = fiinEvaluate.IcbRank;
                updateItem.IcbTotalRanked = fiinEvaluate.IcbTotalRanked;
                updateItem.IndexRank = fiinEvaluate.IndexRank;
                updateItem.IndexTotalRanked = fiinEvaluate.IndexTotalRanked;
                updateItem.IcbCode = fiinEvaluate.IcbCode;
                updateItem.ComGroupCode = fiinEvaluate.ComGroupCode;
                updateItem.Growth = fiinEvaluate.Growth;
                updateItem.Value = fiinEvaluate.Value;
                updateItem.Momentum = fiinEvaluate.Momentum;
                updateItem.Vgm = fiinEvaluate.Vgm;
                updateItem.ControlStatusCode = fiinEvaluate.ControlStatusCode;
                updateItem.ControlStatusName = fiinEvaluate.ControlStatusName;
                updateItem.UpdatedTime = DateTime.Now;
            }
            else
            {
                _marketDbContext.FiinEvaluates.Add(fiinEvaluate);
            }
            return await _marketDbContext.SaveChangesAsync() > 0;
        }
        #endregion

        #region VndStockScore
        public virtual async Task<List<VndStockScore>> GetVndStockScoreAsync(string symbol)
        {
            Guard.Against.NullOrEmpty(symbol, nameof(symbol));
            return await _marketDbContext.VndStockScores.Where(c => c.Symbol == symbol).ToListAsync();
        }

        public virtual async Task<bool> SaveVndStockScoresAsync(List<VndStockScore> vndStockScores)
        {
            Guard.Against.Null(vndStockScores, nameof(vndStockScores));
            foreach (var vndStockScore in vndStockScores)
            {
                var updateItem = _marketDbContext.VndStockScores.FirstOrDefault(q => q.Symbol == vndStockScore.Symbol && q.CriteriaCode == vndStockScore.CriteriaCode);
                if (updateItem != null)
                {
                    updateItem.Type = vndStockScore.Type;
                    updateItem.FiscalDate = vndStockScore.FiscalDate;
                    updateItem.ModelCode = vndStockScore.ModelCode;
                    updateItem.CriteriaCode = vndStockScore.CriteriaCode;
                    updateItem.CriteriaType = vndStockScore.CriteriaType;
                    updateItem.CriteriaName = vndStockScore.CriteriaName;
                    updateItem.Point = vndStockScore.Point;
                    updateItem.Locale = vndStockScore.Locale;
                    updateItem.UpdatedTime = DateTime.Now;
                }
                else
                {
                    _marketDbContext.VndStockScores.Add(vndStockScore);
                }
            }
            return await _marketDbContext.SaveChangesAsync() > 0;
        }
        #endregion

        #region ChartPrice
        public virtual async Task<bool> SaveChartPriceAsync(List<ChartPrice> chartPrices, string symbol, string type = "D")
        {
            if (chartPrices.Count > 0)
            {
                _marketDbContext.Database.ExecuteSqlRaw($"DELETE ChartPrices WHERE Symbol = '{symbol}' AND [Type] = '{type}'");
                _marketDbContext.ChartPrices.AddRange(chartPrices);
            }
            return await _marketDbContext.SaveChangesAsync() > 0;
        }

        public virtual async Task<List<ChartPrice>> GetChartPricesAsync(string symbol, string type = "D", DateTime? fromDate = null, DateTime? toDate = null)
        {
            return await _marketDbContext.ChartPrices.Where(s =>
                s.Symbol == symbol
                && s.Type == type
                && (!fromDate.HasValue || s.TradingDate > fromDate)
                && (!toDate.HasValue || s.TradingDate <= toDate)).ToListAsync();
        }
        #endregion

        #region StockPrice
        public virtual async Task<bool> SaveStockPriceAsync(List<StockPrice> insertItems, List<StockPrice> updateItems)
        {
            if (insertItems.Count > 0)
            {
                _marketDbContext.StockPrices.AddRange(insertItems);
            }
            if (updateItems.Count > 0)
            {
                foreach (var item in updateItems)
                {
                    item.UpdatedTime = DateTime.Now;
                }
            }
            return await _marketDbContext.SaveChangesAsync() > 0;
        }

        public virtual async Task<StockPrice?> GetStockPriceAsync(string symbol, DateTime tradingDate)
        {
            return await _marketDbContext.StockPrices.FirstOrDefaultAsync(s => s.Symbol == symbol && s.TradingDate == tradingDate);
        }

        public virtual async Task<List<StockPrice>> GetTopStockPriceAsync(string symbol, int top)
        {
            return await _marketDbContext.StockPrices.OrderByDescending(q => q.TradingDate).Where(s => s.Symbol == symbol).Take(top).ToListAsync();
        }

        public virtual async Task<List<StockPrice>> CacheGetAllStockPricesAsync(string symbol)
        {
            var cacheKey = $"{Constants.StockPriceCachePrefix}-SM{symbol}";
            return await _memoryCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _marketDbContext.StockPrices.AsNoTracking().OrderByDescending(q => q.TradingDate).Where(s => s.Symbol == symbol).Take(100000).ToListAsync();
            }, Constants.DefaultCacheTime * 60 * 24);
        }

        public virtual async Task<List<StockPrice>> GetAnalyticsTopStockPriceAsync(string symbol, int top)
        {
            return (await _marketDbContext.StockPrices.Where(s => s.Symbol == symbol && s.ClosePrice > 0 && s.ClosePriceAdjusted > 0)
                .OrderByDescending(q => q.TradingDate)
                .Take(top).ToListAsync()).Select(q =>
            {
                var changePercent = (q.ClosePrice - q.ClosePriceAdjusted) / q.ClosePrice;
                return new StockPrice()
                {
                    Symbol = q.Symbol,
                    TradingDate = q.TradingDate,
                    OpenPrice = q.OpenPrice - (q.OpenPrice * changePercent),
                    HighestPrice = q.HighestPrice - (q.HighestPrice * changePercent),
                    LowestPrice = q.LowestPrice - (q.LowestPrice * changePercent),
                    ClosePrice = q.ClosePriceAdjusted,
                    TotalMatchVol = q.TotalMatchVol
                };
            }).ToList();
        }

        public virtual async Task<List<StockPrice>> GetForTradingAsync(string symbol, DateTime fromDate, DateTime? toDate = null)
        {
            return (await _marketDbContext.StockPrices.Where(s =>
                s.Symbol == symbol
                && s.ClosePrice > 0
                && s.ClosePriceAdjusted > 0
                && s.TradingDate >= fromDate
                && (!toDate.HasValue || s.TradingDate <= toDate))
                .OrderByDescending(q => q.TradingDate)
                .ToListAsync()).Select(q =>
                {
                    var changePercent = (q.ClosePrice - q.ClosePriceAdjusted) / q.ClosePrice;
                    return new StockPrice()
                    {
                        Symbol = q.Symbol,
                        TradingDate = q.TradingDate,
                        OpenPrice = q.OpenPrice - (q.OpenPrice * changePercent),
                        HighestPrice = q.HighestPrice - (q.HighestPrice * changePercent),
                        LowestPrice = q.LowestPrice - (q.LowestPrice * changePercent),
                        ClosePrice = q.ClosePriceAdjusted,
                        TotalMatchVol = q.TotalMatchVol
                    };
                }).ToList();
        }

        public virtual async Task<List<StockPrice>> GetAnalyticsTopIndexPriceAsync(string index, int top)
        {
            return (await _marketDbContext.StockPrices.Where(s => s.Symbol == index && s.ClosePrice > 0 && s.ClosePriceAdjusted > 0)
                .OrderByDescending(q => q.TradingDate)
                .Take(top).ToListAsync());
        }

        public virtual async Task<StockPrice?> GetLastStockPriceAsync(string symbol)
        {
            Guard.Against.NullOrEmpty(symbol, nameof(symbol));
            return await _marketDbContext.StockPrices.OrderByDescending(q => q.TradingDate).FirstOrDefaultAsync(s => s.Symbol == symbol);
        }

        public virtual async Task<bool> DeleteStockPrices(string symbol)
        {
            Guard.Against.NullOrEmpty(symbol, nameof(symbol));
            return await _marketDbContext.Database.ExecuteSqlRawAsync($"DELETE StockPrices WHERE Symbol = '{symbol}'") > 0;
        }

        public virtual async Task<List<StockPrice>> GetForStockViewAsync(string symbol, int numberItem = 10000)
        {
            return await _marketDbContext.StockPrices.AsNoTracking().OrderByDescending(q => q.TradingDate).Where(s => s.Symbol == symbol).Take(numberItem).Select(q => new StockPrice()
            {
                Symbol = q.Symbol,
                TradingDate = q.TradingDate,
                ClosePrice = q.ClosePrice,
                ClosePriceAdjusted = q.ClosePriceAdjusted,
                HighestPrice = q.HighestPrice,
                LowestPrice = q.LowestPrice,
                TotalMatchVol = q.TotalMatchVol,
                ForeignBuyVolTotal = q.ForeignBuyVolTotal,
                ForeignSellVolTotal = q.ForeignSellVolTotal,
                TotalBuyTrade = q.TotalBuyTrade,
                TotalSellTrade = q.TotalSellTrade,
            }).ToListAsync();
        }
        #endregion

        #region StockRecommendation
        public virtual async Task<bool> SaveStockRecommendationAsync(List<StockRecommendation> stockRecommendations)
        {
            Guard.Against.Null(stockRecommendations, nameof(stockRecommendations));
            foreach (var stockRecommendation in stockRecommendations)
            {
                var updateItem = _marketDbContext.StockRecommendations.FirstOrDefault(q => q.Symbol == stockRecommendation.Symbol && q.ReportDate == stockRecommendation.ReportDate && q.Analyst == stockRecommendation.Analyst);
                if (updateItem != null)
                {
                    updateItem.Firm = stockRecommendation.Firm;
                    updateItem.Type = stockRecommendation.Type;
                    updateItem.ReportDate = stockRecommendation.ReportDate;
                    updateItem.Source = stockRecommendation.Source;
                    updateItem.Analyst = stockRecommendation.Analyst;
                    updateItem.ReportPrice = stockRecommendation.ReportPrice;
                    updateItem.TargetPrice = stockRecommendation.TargetPrice;
                    updateItem.AvgTargetPrice = stockRecommendation.AvgTargetPrice;
                    updateItem.UpdatedTime = DateTime.Now;
                }
                else
                {
                    _marketDbContext.StockRecommendations.Add(stockRecommendation);
                }
            }
            return await _marketDbContext.SaveChangesAsync() > 0;
        }
        public virtual async Task<List<StockRecommendation>> GetTopStockRecommendationInSixMonthAsync(string symbol, int top)
        {
            Guard.Against.NullOrEmpty(symbol, nameof(symbol));
            var endDate = DateTime.Now.AddMonths(-6);
            return await _marketDbContext.StockRecommendations.Where(q => q.ReportDate >= endDate && q.Symbol == symbol).OrderByDescending(q => q.ReportDate).Take(top).ToListAsync();
        }
        #endregion

        #region Stock
        public virtual async Task<bool> InitialStockAsync(List<Stock> insertItems, List<Stock> updateItems)
        {
            if (insertItems.Count > 0)
            {
                _marketDbContext.Stocks.AddRange(insertItems);
            }
            if (updateItems.Count > 0)
            {
                foreach (var item in updateItems)
                {
                    item.UpdatedTime = DateTime.Now;
                }
            }
            return await _marketDbContext.SaveChangesAsync() > 0;
        }

        public virtual async Task<Dictionary<string, Stock>> GetStockDictionaryAsync()
        {
            return await _marketDbContext.Stocks.AsNoTracking().ToDictionaryAsync(s => s.Symbol, s => s);
        }

        public virtual async Task<Stock?> GetStockByCode(string symbol)
        {
            return await _marketDbContext.Stocks.FirstOrDefaultAsync(q => q.Symbol == symbol);
        }

        public virtual async Task<List<Stock>> GetStockByType(string type)
        {
            return await _marketDbContext.Stocks.Where(q => q.Type == type).ToListAsync();
        }

        public virtual async Task<Stock?> CacheGetStockByCodeAsync(string symbol)
        {
            var cacheKey = $"{Constants.StockCachePrefix}-SM{symbol}";
            return await _memoryCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _marketDbContext.Stocks.AsNoTracking().FirstOrDefaultAsync(q => q.Symbol == symbol);
            }, Constants.DefaultCacheTime * 60 * 24);
        }

        public virtual async Task<List<string>> CacheGetExchangesAsync()
        {
            var cacheKey = $"{Constants.StockCachePrefix}-CGEA";
            return await _memoryCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _marketDbContext.Stocks.Where(q => q.Type == "i").Select(q => q.Exchange).Distinct().ToListAsync();
            }, Constants.DefaultCacheTime * 60 * 24);
        }
        #endregion

        #region Industry
        public virtual async Task<bool> SaveIndustryAsync(Industry industry)
        {
            Guard.Against.Null(industry, nameof(industry));
            var checkItem = _marketDbContext.Industries.FirstOrDefault(q => q.Code == industry.Code);
            if (checkItem is null)
            {
                _marketDbContext.Industries.Add(industry);
            }
            else
            {
                checkItem.Name = industry.Name;
                checkItem.UpdatedTime = DateTime.Now;
            }
            return await _marketDbContext.SaveChangesAsync() > 0;
        }

        public virtual async Task<List<Industry>> GetIndustriesAsync()
        {
            return await _marketDbContext.Industries.ToListAsync();
        }
        #endregion

        #region Leadership
        public virtual async Task<List<Leadership>> GetLeadershipsAsync(string symbol)
        {
            return await _marketDbContext.Leaderships.Where(q => q.Symbol == symbol).ToListAsync();
        }

        public virtual async Task<bool> SaveLeadershipsAsync(List<Leadership> insertItems, List<Leadership> deleteItems)
        {
            if (insertItems.Count > 0)
            {
                _marketDbContext.Leaderships.AddRange(insertItems);
            }
            if (deleteItems.Count > 0)
            {
                _marketDbContext.Leaderships.RemoveRange(deleteItems);
            }
            return await _marketDbContext.SaveChangesAsync() > 0;
        }
        #endregion

        #region FinancialGrowth
        public virtual async Task<FinancialGrowth?> GetLastFinancialGrowthAsync(string symbol)
        {
            return await _marketDbContext.FinancialGrowths.OrderByDescending(q => q.Year).FirstOrDefaultAsync(q => q.Symbol == symbol);
        }

        public virtual async Task<List<FinancialGrowth>> GetFinancialGrowthsAsync(string symbol)
        {
            return await _marketDbContext.FinancialGrowths.Where(q => q.Symbol == symbol).ToListAsync();
        }

        public virtual async Task<bool> SaveFinancialGrowthAsync(List<FinancialGrowth> insertItems, List<FinancialGrowth> updateItems)
        {
            if (insertItems.Count > 0)
            {
                _marketDbContext.FinancialGrowths.AddRange(insertItems);
            }
            if (updateItems.Count > 0)
            {
                foreach (var item in updateItems)
                {
                    item.UpdatedTime = DateTime.Now;
                }
            }
            return await _marketDbContext.SaveChangesAsync() > 0;
        }
        #endregion

        #region CorporateAction
        public virtual async Task<List<CorporateAction>> GetCorporateActionsForCheckDownloadAsync(string symbol)
        {
            return await _marketDbContext.CorporateActions.Where(q => q.Symbol == symbol).Select(q => new CorporateAction()
            {
                Symbol = q.Symbol,
                EventCode = q.EventCode,
                ExrightDate = q.ExrightDate
            }).ToListAsync();
        }

        public virtual async Task<List<CorporateAction>> GetCorporateActionsAsync(string? symbol = null, string? exchange = null, string[]? eventCode = null)
        {
            var query = _marketDbContext.CorporateActions.Where(q => q.ExrightDate >= DateTime.Now.Date);
            if (!string.IsNullOrEmpty(exchange))
            {
                query = query.Where(q => q.Exchange == exchange);
            }
            if (!string.IsNullOrEmpty(symbol))
            {
                query = query.Where(q => q.Symbol == symbol);
            }
            if (eventCode is not null && eventCode.Length > 0)
            {
                query = query.Where(q => eventCode.Contains(q.EventCode));
            }
            return await query.ToListAsync();
        }

        public virtual async Task<List<CorporateAction>> GetCorporateActionTradingByExrightDateAsync()
        {
            var tradingDate = DateTime.Now.Date;
            var eventCode = new string[] { "DIV", "ISS" };
            return await _marketDbContext.CorporateActions.Where(q => q.ExrightDate.Date == tradingDate && eventCode.Contains(q.EventCode)).Select(q => new CorporateAction()
            {
                Symbol = q.Symbol,
                EventCode = q.EventCode,
                ExrightDate = q.ExrightDate
            }).ToListAsync();
        }

        public virtual async Task<bool> InsertCorporateActionAsync(List<CorporateAction> insertItems)
        {
            if (insertItems.Count > 0)
            {
                _marketDbContext.CorporateActions.AddRange(insertItems);
            }
            return await _marketDbContext.SaveChangesAsync() > 0;
        }
        #endregion

        #region StockTransaction
        public virtual async Task<bool> SaveStockTransactionAsync(StockTransaction stockTransaction)
        {
            Guard.Against.Null(stockTransaction, nameof(stockTransaction));
            var updateItem = _marketDbContext.StockTransactions.FirstOrDefault(q => q.Symbol == stockTransaction.Symbol && q.TradingDate == stockTransaction.TradingDate);
            if (updateItem != null)
            {
                updateItem.Symbol = stockTransaction.Symbol;
                updateItem.TradingDate = stockTransaction.TradingDate;
                updateItem.ZipDetails = stockTransaction.ZipDetails;
                updateItem.UpdatedTime = DateTime.Now;
            }
            else
            {
                _marketDbContext.StockTransactions.Add(stockTransaction);
            }
            return await _marketDbContext.SaveChangesAsync() > 0;
        }
        #endregion
    }
}
