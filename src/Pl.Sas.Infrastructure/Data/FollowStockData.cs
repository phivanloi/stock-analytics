using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Entities.Identity;
using Pl.Sas.Core.Interfaces;

namespace Pl.Sas.Infrastructure.Data
{
    public class FollowStockData : BaseData, IFollowStockData
    {
        public FollowStockData(IOptionsMonitor<ConnectionStrings> options) : base(options) { }

        public virtual async Task<List<FollowStock>> FindAllAsync(string userId, string? symbol = null)
        {
            var query = "SELECT * FROM FollowStocks WHERE UserId = @userId ";
            if (!string.IsNullOrEmpty(symbol))
            {
                query += " AND Symbol = @symbol ";
            }
            using SqlConnection connection = new(_connectionStrings.IdentityConnection);
            return (await connection.QueryAsync<FollowStock>(query, new { userId, symbol })).AsList();
        }

        public virtual async Task<FollowStock> FindByIdAsync(string id)
        {
            var query = "SELECT * FROM FollowStocks WHERE Id = @id";
            using SqlConnection connection = new(_connectionStrings.IdentityConnection);
            return await connection.QueryFirstOrDefaultAsync<FollowStock>(query, new { id });
        }

        public virtual async Task<FollowStock> FindAsync(string userId, string symbol)
        {
            var query = "SELECT * FROM FollowStocks WHERE UserId = @userId AND Symbol = @symbol";
            using SqlConnection connection = new(_connectionStrings.IdentityConnection);
            return await connection.QueryFirstOrDefaultAsync<FollowStock>(query, new { userId, symbol });
        }

        public virtual async Task<bool> InsertAsync(FollowStock userStockNote)
        {
            var query = $@"   INSERT INTO FollowStocks
                                        (Id,
                                        UserId,
                                        Symbol,
                                        CreatedTime,
                                        UpdatedTime)
                                    VALUES
                                        (@Id,
                                        @UserId,
                                        @Symbol,
                                        @CreatedTime,
                                        @UpdatedTime)";
            using SqlConnection connection = new(_connectionStrings.IdentityConnection);
            return await connection.ExecuteAsync(query, userStockNote) > 0;
        }

        public virtual async Task<bool> DeleteAsync(string id)
        {
            var query = "DELETE FollowStocks WHERE Id = @id";
            using SqlConnection connection = new(_connectionStrings.IdentityConnection);
            return await connection.ExecuteAsync(query, new { id }) > 0;
        }

        public virtual async Task<bool> IsUserHasFollowAsync(string userId)
        {
            var query = "SELECT COUNT(1) FROM FollowStocks WHERE UserId = @userId";
            using SqlConnection connection = new(_connectionStrings.IdentityConnection);
            return await connection.QueryFirstOrDefaultAsync<int>(query, new { userId }) > 0;
        }
    }
}