using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Pl.Sas.Core;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;

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

        public virtual async Task<bool> InsertScheduleAsync(List<Schedule> schedules)
        {
            if (schedules.Count > 0)
            {
                _marketDbContext.Schedules.AddRange(schedules);
                return await _marketDbContext.SaveChangesAsync() > 0;
            }
            return false;
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
                    _marketDbContext.Stocks.Attach(item);
                }
            }
            return await _marketDbContext.SaveChangesAsync() > 0;
        }

        public virtual async Task<Dictionary<string, Stock>> GetStockDictionaryAsync()
        {
            return await _marketDbContext.Stocks.AsNoTracking().ToDictionaryAsync(s => s.Symbol, s => s);
        }

        public virtual async Task<Schedule?> GetScheduleByIdAsync(string id)
        {
            return await _marketDbContext.Schedules.FirstOrDefaultAsync(s => s.Id == id);
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
    }
}
