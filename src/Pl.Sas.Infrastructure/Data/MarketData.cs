using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Pl.Sas.Core;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using System.Text.Json;

namespace Pl.Sas.Infrastructure.Data
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

        public virtual async Task<StockPrice?> GeStockPriceAsync(string symbol, DateTime tradingDate)
        {
            return await _marketDbContext.StockPrices.FirstOrDefaultAsync(s => s.Symbol == symbol && s.TradingDate == tradingDate);
        }

        public virtual async Task<bool> InsertScheduleAsync(List<Schedule> schedules)
        {
            if (schedules.Count > 0)
            {
                _marketDbContext.Schedules.AddRange(schedules);
                return await _marketDbContext.SaveChangesAsync() > 0;
            }
            return false;
        }

        public virtual async Task<bool> UpdateKeyOptionScheduleAsync(Schedule schedule, string key, string value)
        {
            var options = schedule.Options;
            if (options.ContainsKey("key"))
            {
                options[key] = value;
            }
            else
            {
                options.Add(key, value);
            }
            schedule.OptionsJson = JsonSerializer.Serialize(options);
            return await _marketDbContext.SaveChangesAsync() > 0;
        }

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
            return await _marketDbContext.Stocks.ToDictionaryAsync(s => s.Symbol, s => s);
        }

        public virtual async Task<Schedule?> GetScheduleByIdAsync(string id)
        {
            return await _marketDbContext.Schedules.FirstOrDefaultAsync(s => s.Id == id);
        }

        public virtual async Task<bool> UpdateScheduleAsync(Schedule schedule)
        {
            schedule.UpdatedTime = DateTime.Now;
            return await _marketDbContext.SaveChangesAsync() > 0;
        }

        public virtual async Task<Schedule?> CacheGetScheduleByIdAsync(string id)
        {
            var cacheKey = $"{Constants.ScheduleCachePrefix}-CGSBIA{id}";
            return await _memoryCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                var item = await _marketDbContext.Schedules.FirstOrDefaultAsync(s => s.Id == id);
                if (item != null)
                {
                    _marketDbContext.Entry(item).State = EntityState.Detached;
                }
                return item;
            }, Constants.DefaultCacheTime * 60 * 24);
        }

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

        public virtual async Task<List<CorporateAction>> GetCorporateActionsAsync(string symbol)
        {
            return await _marketDbContext.CorporateActions.Where(q => q.Symbol == symbol).ToListAsync();
        }

        public virtual async Task<bool> InsertCorporateActionAsync(List<CorporateAction> insertItems)
        {
            if (insertItems.Count > 0)
            {
                _marketDbContext.CorporateActions.AddRange(insertItems);
            }
            return await _marketDbContext.SaveChangesAsync() > 0;
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
    }
}
