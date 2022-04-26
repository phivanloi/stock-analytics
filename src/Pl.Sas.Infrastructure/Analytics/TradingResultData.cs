using Dapper;
using Microsoft.Extensions.Options;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Pl.Sas.Infrastructure.Analytics
{
    public class TradingResultData : ITradingResultData
    {
        private readonly Connections _connections;

        public TradingResultData(
            IOptions<Connections> options)
        {
            _connections = options.Value;
        }

        public virtual async Task<List<TradingResult>> FindAllAsync(string symbol, string datePath = null, int? principle = null, int numberItem = 10)
        {
            string query = @"SELECT TOP(@numberItem) * FROM TradingResults WHERE Symbol = @symbol";
            if (!string.IsNullOrEmpty(datePath))
            {
                query += $" AND DatePath = @datePath";
            }
            if (principle.HasValue)
            {
                query += $" AND Principle = @principle";
            }
            query += " ORDER BY TradingDate DESC";
            using SqlConnection connection = new(_connections.AnalyticsConnection);
            return (await connection.QueryAsync<TradingResult>(query, new { datePath, symbol, numberItem })).AsList();
        }

        public virtual async Task<List<TradingResult>> GetForViewAsync(string symbol, int numberItem = 10)
        {
            string query = @"   SELECT TOP(@numberItem)
                                    Principle,
                                    TradingDate,
                                    ProfitPercent,
                                    BuyPrice,
                                    SellPrice,
                                    IsBuy,
                                    IsSell
                                FROM
                                    TradingResults
                                WHERE
                                    Symbol = @symbol
                                ORDER BY
                                    TradingDate DESC";
            using SqlConnection connection = new(_connections.AnalyticsConnection);
            return (await connection.QueryAsync<TradingResult>(query, new { symbol, numberItem })).AsList();
        }

        public virtual async Task<bool> SaveTestTradingResultAsync(TradingResult tradingResult)
        {
            var query = $@" SET TRANSACTION ISOLATION LEVEL REPEATABLE READ
                            BEGIN TRAN
                            DECLARE @tradingResultId AS NVARCHAR(22);
                            SET @tradingResultId = (SELECT TOP(1) Id FROM TradingResults WHERE Symbol = @Symbol AND DatePath = @DatePath AND Principle = @Principle);
                            IF @tradingResultId IS NULL
                            BEGIN
                                INSERT INTO TradingResults
                                    (Id,
                                    Symbol,
                                    PrinCiple,
                                    TradingDate,
                                    DatePath,
                                    IsBuy,
                                    BuyPrice,
                                    IsSell,
                                    SellPrice,
                                    Profit,
                                    ProfitPercent,
                                    Capital,
                                    TotalTax,
                                    ZipExplainNotes,
                                    CreatedTime,
                                    UpdatedTime)
                                VALUES
                                    (@Id,
                                    @Symbol,
                                    @Principle,
                                    @TradingDate,
                                    @DatePath,
                                    @IsBuy,
                                    @BuyPrice,
                                    @IsSell,
                                    @SellPrice,
                                    @Profit,
                                    @ProfitPercent,
                                    @Capital,
                                    @TotalTax,
                                    @ZipExplainNotes,
                                    GETDATE(),
                                    GETDATE());
                            END
                            ELSE
                            BEGIN
                                UPDATE TradingResults SET
                                    IsBuy = @IsBuy,
                                    BuyPrice = @BuyPrice,
                                    IsSell = @IsSell,
                                    SellPrice = @SellPrice,
                                    Profit = @Profit,
                                    ProfitPercent = @ProfitPercent,
                                    Capital = @Capital,
                                    TotalTax = @TotalTax,
                                    ZipExplainNotes = @ZipExplainNotes,
                                    UpdatedTime = GETDATE()
                                WHERE
                                    Id = @tradingResultId
                            END
                            COMMIT";
            using SqlConnection connection = new(_connections.AnalyticsConnection);
            return await connection.ExecuteAsync(query, tradingResult) > 0;
        }
    }
}