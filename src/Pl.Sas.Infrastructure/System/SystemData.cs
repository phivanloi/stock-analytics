using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Pl.Sas.Core;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using System.Text.Json;

namespace Pl.Sas.Infrastructure.System
{
    /// <summary>
    /// Lớp xử lỹ liệu của các thành phần hệ thống
    /// </summary>
    public class SystemData : ISystemData
    {
        private readonly SystemDbContext _systemDbContext;
        private readonly IMemoryCacheService _memoryCacheService;

        public SystemData(
            IMemoryCacheService memoryCacheService,
            SystemDbContext systemDbContext)
        {
            _systemDbContext = systemDbContext;
            _memoryCacheService = memoryCacheService;
        }

        public virtual async Task<KeyValue?> GetKeyValueAsync(string key)
        {
            Guard.Against.NullOrEmpty(key, nameof(key));
            return await _systemDbContext.KeyValues.FirstOrDefaultAsync(x => x.Key == key);
        }

        public virtual async Task<bool> SetKeyValueAsync<T>(string key, T value)
        {
            var updateItem = _systemDbContext.KeyValues.FirstOrDefault(x => x.Key == key);
            if (updateItem == null)
            {
                _systemDbContext.KeyValues.Add(new() { Key = key, Value = JsonSerializer.Serialize(value) });
            }
            else
            {
                updateItem.Value = JsonSerializer.Serialize(value);
                updateItem.UpdatedTime = DateTime.Now;
            }
            return await _systemDbContext.SaveChangesAsync() > 0;
        }

        public virtual async Task<Schedule?> GetScheduleAsync(int type, string dataKey)
        {
            return await _systemDbContext.Schedules.FirstOrDefaultAsync(s => s.Type == type && s.DataKey == dataKey);
        }

        public virtual async Task<Schedule?> GetScheduleByIdAsync(string id)
        {
            return await _systemDbContext.Schedules.FirstOrDefaultAsync(s => s.Id == id);
        }

        public virtual async Task<bool> UpdateScheduleAsync(Schedule schedule)
        {
            schedule.UpdatedTime = DateTime.Now;
            return await _systemDbContext.SaveChangesAsync() > 0;
        }

        public virtual async Task<Schedule?> CacheGetScheduleByIdAsync(string id)
        {
            var cacheKey = $"{Constants.ScheduleCachePrefix}-CGSBIA{id}";
            return await _memoryCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                var item = await _systemDbContext.Schedules.FirstOrDefaultAsync(s => s.Id == id);
                if (item != null)
                {
                    _systemDbContext.Entry(item).State = EntityState.Detached;
                }
                return item;
            }, Constants.DefaultCacheTime * 60 * 24);
        }

        public virtual async Task<bool> InsertScheduleAsync(List<Schedule> schedules)
        {
            if (schedules.Count > 0)
            {
                _systemDbContext.Schedules.AddRange(schedules);
                return await _systemDbContext.SaveChangesAsync() > 0;
            }
            return false;
        }

        public virtual async Task<bool> UtilityUpdateAsync(int type, string symbol)
        {
            var query = _systemDbContext.Schedules.Where(q => q.Type == type);
            if (!string.IsNullOrEmpty(symbol))
            {
                query = query.Where(q => q.DataKey == symbol);
            }
            query.ToList().ForEach(q => q.ActiveTime = DateTime.Now);
            return await _systemDbContext.SaveChangesAsync() > 0;
        }
    }
}
