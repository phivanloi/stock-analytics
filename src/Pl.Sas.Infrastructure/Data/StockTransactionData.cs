using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;

namespace Pl.Sas.Infrastructure.Data
{
    public class StockTransactionData : BaseData, IStockTransactionData
    {
        public StockTransactionData(IOptionsMonitor<ConnectionStrings> options) : base(options) { }

        public virtual async Task<List<StockTransaction>> FindAllAsync(string symbol, DateTime? tradingDate = null)
        {
            string query = @"SELECT * FROM StockTransactions WHERE Symbol = @symbol";
            if (tradingDate.HasValue)
            {
                query += $" AND TradingDate = @tradingDate";
            }
            query += " ORDER BY TradingDate DESC";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return (await connection.QueryAsync<StockTransaction>(query, new { tradingDate, symbol })).AsList();
        }

        public virtual async Task<StockTransaction> FindAsync(string symbol, DateTime? tradingDate = null)
        {
            string query = @"SELECT TOP(1) * FROM StockTransactions WHERE Symbol = @symbol AND TradingDate = @tradingDate";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.QueryFirstOrDefaultAsync<StockTransaction>(query, new { tradingDate, symbol });
        }

        public virtual async Task<List<StockTransaction>> GetForTradingAsync(string symbol, DateTime? startDate = null, DateTime? endDate = null)
        {
            string query = @"SELECT DatePath, ZipDetails FROM StockTransactions WHERE Symbol = @symbol";
            if (startDate.HasValue)
            {
                query += $" AND TradingDate >= @startDate";
            }
            if (endDate.HasValue)
            {
                query += $" AND TradingDate <= @endDate";
            }
            query += " ORDER BY TradingDate";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return (await connection.QueryAsync<StockTransaction>(query, new { symbol, startDate, endDate })).AsList();
        }

        public virtual async Task<bool> SaveStockTransactionAsync(StockTransaction stockTransaction)
        {
            var query = $@" SET TRANSACTION ISOLATION LEVEL REPEATABLE READ
                            BEGIN TRAN
                            DECLARE @stockTransactionId AS NVARCHAR(22);
                            SET @stockTransactionId = (SELECT TOP(1) Id FROM StockTransactions WHERE Symbol = @Symbol AND DatePath = @DatePath);
                            IF @stockTransactionId IS NULL
                            BEGIN
                                INSERT INTO StockTransactions
                                    (Id,
                                    Symbol,
                                    TradingDate,
                                    DatePath,
                                    ZipDetails,
                                    CreatedTime,
                                    UpdatedTime)
                                VALUES
                                    (@Id,
                                    @Symbol,
                                    @TradingDate,
                                    @DatePath,
                                    @ZipDetails,
                                    GETDATE(),
                                    GETDATE());
                            END
                            ELSE
                            BEGIN
                                UPDATE StockTransactions SET
                                    ZipDetails = @ZipDetails,
                                    UpdatedTime = GETDATE()
                                WHERE
                                    Id = @stockTransactionId
                            END
                            COMMIT";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.ExecuteAsync(query, stockTransaction) > 0;
        }
    }
}