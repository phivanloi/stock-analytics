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
    }
}
