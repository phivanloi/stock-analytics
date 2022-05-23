using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using System.Text.Json;

namespace Pl.Sas.Infrastructure.Data
{
    public class KeyValueData : BaseData, IKeyValueData
    {
        public KeyValueData(IOptionsMonitor<ConnectionStrings> options) : base(options) { }

        public virtual async Task<T> SetAsync<T>(string key, T value)
        {
            var stringValue = JsonSerializer.Serialize(value);
            var query = "UPDATE KeyValues SET Value = @stringValue WHERE [Key] = @key";
            using SqlConnection connection = new(_connectionStrings.AnalyticsConnection);
            await connection.QueryAsync<KeyValue>(query, new { key, stringValue });
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
    }
}