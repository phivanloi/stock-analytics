using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Pl.Sas.Core;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using System.Text.Json;

namespace Pl.Sas.Infrastructure.Data
{
    public class KeyValueData : BaseData, IKeyValueData
    {
        private readonly IMemoryCacheService _memoryCacheService;
        public KeyValueData(IMemoryCacheService memoryCacheService, IOptionsMonitor<ConnectionStrings> options) : base(options)
        {
            _memoryCacheService = memoryCacheService;
        }

        public virtual async Task<T> SetAsync<T>(string key, T value)
        {
            var stringValue = JsonSerializer.Serialize(value);
            var query = @"  IF NOT EXISTS (SELECT * FROM KeyValues WHERE [Key] = @key)
                                INSERT INTO KeyValues(Id ,[Key], [Value], [CreatedTime], [UpdatedTime])
                                VALUES(@Id ,@key, @stringValue, GETDATE(), GETDATE())
                            ELSE
                                UPDATE KeyValues
                                SET [Value] = @stringValue, [UpdatedTime] = GETDATE()
                                WHERE [Key] = @key";
            using SqlConnection connection = new(_connectionStrings.AnalyticsConnection);
            await connection.QueryAsync<KeyValue>(query, new { key, stringValue, Id = Utilities.GenerateShortGuid() });
            var cacheKey = $"{Constants.KeyValueCachePrefix}-{key.ToUpper()}";
            _memoryCacheService.Remove(cacheKey);
            return value;
        }

        public virtual async Task<KeyValue> GetAsync(string key)
        {
            var query = "SELECT * FROM KeyValues WHERE [Key] = @key";
            using SqlConnection connection = new(_connectionStrings.AnalyticsConnection);
            return await _dbAsyncRetry.ExecuteAsync(async () =>
            {
                return await connection.QueryFirstOrDefaultAsync<KeyValue>(query, new { key });
            });
        }

        public virtual async Task<KeyValue> CacheGetAsync(string key)
        {
            var cacheKey = $"{Constants.KeyValueCachePrefix}-{key.ToUpper()}";
            return await _memoryCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                var query = "SELECT * FROM KeyValues WHERE [Key] = @key";
                using SqlConnection connection = new(_connectionStrings.AnalyticsConnection);
                return await _dbAsyncRetry.ExecuteAsync(async () =>
                {
                    return await connection.QueryFirstOrDefaultAsync<KeyValue>(query, new { key });
                });
            }, Constants.DefaultCacheTime * 60 * 24);
        }
    }
}