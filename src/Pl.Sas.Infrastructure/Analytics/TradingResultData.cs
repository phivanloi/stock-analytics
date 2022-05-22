using Dapper;
using Microsoft.Extensions.Options;
using Pl.Sas.Core.Interfaces;
using Microsoft.Data.SqlClient;
using Pl.Sas.Core.Entities;

namespace Pl.Sas.Infrastructure.Analytics
{
    public class TradingResultData : BaseData, ITradingResultData
    {
        public TradingResultData(IOptionsMonitor<ConnectionStrings> options) : base(options) { }

        public virtual async Task<List<TradingResult>> FindAllAsync(string symbol, int? principle = null)
        {
            string query = @"SELECT * FROM TradingResults WHERE Symbol = @symbol";
            if (principle.HasValue)
            {
                query += $" AND Principle = @principle";
            }
            using SqlConnection connection = new(_connectionStrings.AnalyticsConnection);
            return (await connection.QueryAsync<TradingResult>(query, new { symbol, principle })).AsList();
        }

        public virtual async Task<List<TradingResult>> GetForViewAsync(string symbol)
        {
            string query = @"   SELECT
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
                                    Symbol = @symbol";
            using SqlConnection connection = new(_connectionStrings.AnalyticsConnection);
            return (await connection.QueryAsync<TradingResult>(query, new { symbol })).AsList();
        }

        public virtual async Task<bool> SaveTestTradingResultAsync(TradingResult tradingResult)
        {
            var query = $@" SET TRANSACTION ISOLATION LEVEL REPEATABLE READ
                            BEGIN TRAN
                            DECLARE @tradingResultId AS NVARCHAR(22);
                            SET @tradingResultId = (SELECT TOP(1) Id FROM TradingResults WHERE Symbol = @Symbol AND Principle = @Principle);
                            IF @tradingResultId IS NULL
                            BEGIN
                                INSERT INTO TradingResults
                                    (Id,
                                    Symbol,
                                    PrinCiple,
                                    IsBuy,
                                    BuyPrice,
                                    IsSell,
                                    SellPrice,
                                    Profit,
                                    ProfitPercent,
                                    Capital,
                                    TotalTax,
                                    TradingNotes,
                                    CreatedTime,
                                    UpdatedTime)
                                VALUES
                                    (@Id,
                                    @Symbol,
                                    @Principle,
                                    @IsBuy,
                                    @BuyPrice,
                                    @IsSell,
                                    @SellPrice,
                                    @Profit,
                                    @ProfitPercent,
                                    @Capital,
                                    @TotalTax,
                                    @TradingNotes,
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
                                    TradingNotes = @TradingNotes,
                                    UpdatedTime = GETDATE()
                                WHERE
                                    Id = @tradingResultId
                            END
                            COMMIT";
            using SqlConnection connection = new(_connectionStrings.AnalyticsConnection);
            return await connection.ExecuteAsync(query, tradingResult) > 0;
        }
    }
}