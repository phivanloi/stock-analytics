using Ardalis.GuardClauses;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using Pl.Sas.Core.Interfaces;
using StackExchange.Redis;
using System.Net;
using System.Text.Json;

namespace Pl.Sas.Infrastructure.Caching
{
    public class AsyncDistributedCacheService : IAsyncCacheService
    {
        #region Properties And Constructor

        private readonly IZipHelper _zipHelper;

        private readonly IDistributedCache _distributedCache;

        private readonly RedisCacheOptions _redisCacheOptions;

        public AsyncDistributedCacheService(
            IZipHelper zipHelper,
            IDistributedCache distributedCache,
            IOptions<RedisCacheOptions> option)
        {
            Guard.Against.Null(option, nameof(option));

            _distributedCache = distributedCache;
            _redisCacheOptions = option.Value;
            _zipHelper = zipHelper;
        }

        #endregion
        #region Get Method

        public virtual async Task<TItem?> GetByKeyAsync<TItem>(string key)
        {
            byte[] cacheData = await _distributedCache.GetAsync(key);
            if (cacheData != null)
            {
                return JsonSerializer.Deserialize<TItem>(_zipHelper.UnZipByte(cacheData));
            }
            return default;
        }

        public virtual async Task<TItem?> GetOrCreateAsync<TItem>(string key, Func<Task<TItem>> factory)
        {
            var cacheData = await GetByKeyAsync<TItem>(key);
            if (!EqualityComparer<TItem>.Default.Equals(cacheData, default))
            {
                return cacheData;
            }
            TItem functionData = await factory();
            await SetValueAsync(key, functionData);
            return functionData;
        }

        public virtual async Task<TItem?> GetOrCreateAsync<TItem>(string key, Func<Task<TItem>> factory, int time)
        {
            Guard.Against.Null(factory, nameof(factory));

            var cacheData = await GetByKeyAsync<TItem>(key);
            if (!EqualityComparer<TItem>.Default.Equals(cacheData, default))
            {
                return cacheData;
            }
            TItem functionData = await factory();
            await SetValueAsync(key, functionData, time);
            return functionData;
        }

        public virtual async Task<DateTime> GetTime()
        {
            using ConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(_redisCacheOptions.Configuration);
            var endPoints = connectionMultiplexer.GetEndPoints();
            if (endPoints is not null && endPoints.Length > 0)
            {
                return await connectionMultiplexer.GetServer(endPoints[0]).TimeAsync();
            }
            throw new Exception("Can't get redis endPoint to get server time.");
        }

        #endregion
        #region Set Method

        public virtual async Task<TItem> SetValueAsync<TItem>(string key, TItem value)
        {
            await _distributedCache.SetAsync(key, _zipHelper.ZipByte(JsonSerializer.SerializeToUtf8Bytes(value)));
            return value;
        }

        public virtual async Task<TItem> SetValueAsync<TItem>(string key, TItem value, int time)
        {
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(time));
            await _distributedCache.SetAsync(key, _zipHelper.ZipByte(JsonSerializer.SerializeToUtf8Bytes(value)), options);
            return value;
        }

        #endregion
        #region Refresh Method
        public virtual async Task RefreshAsync(string key)
        {
            await _distributedCache.RefreshAsync(key);
        }
        #endregion
        #region Remove Method

        public virtual async Task RemoveAsync(string key)
        {
            await _distributedCache.RemoveAsync(key);
        }

        public virtual async Task RemoveByPrefixAsync(string pattern)
        {
            using ConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(_redisCacheOptions.Configuration);
            foreach (EndPoint endPoints in connectionMultiplexer.GetEndPoints())
            {
                IServer server = connectionMultiplexer.GetServer(endPoints);
                IDatabase db = connectionMultiplexer.GetDatabase();
                IEnumerable<RedisKey> keys = server.Keys(database: db.Database, pattern: $"{_redisCacheOptions.InstanceName}{pattern}*");
                await db.KeyDeleteAsync(keys.ToArray());
            }
        }

        public virtual async Task ClearAsync()
        {
            using ConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(_redisCacheOptions.Configuration);
            foreach (EndPoint endPoints in connectionMultiplexer.GetEndPoints())
            {
                IServer server = connectionMultiplexer.GetServer(endPoints);
                IDatabase db = connectionMultiplexer.GetDatabase();
                await server.FlushDatabaseAsync(db.Database);
            }
        }

        #endregion
    }
}