using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;

namespace Pl.Sas.Infrastructure.Data
{
    public class TradingResultData : BaseData, ITradingResultData
    {
        public TradingResultData(IOptionsMonitor<ConnectionStrings> options) : base(options) { }

        public virtual async Task<List<TradingResult>> FindAllAsync()
        {
            string query = @"SELECT * FROM TradingResults";
            using SqlConnection connection = new(_connectionStrings.AnalyticsConnection);
            return (await connection.QueryAsync<TradingResult>(query)).AsList();
        }

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

        public virtual async Task<List<TradingResult>?> GetForViewAsync(string symbol)
        {
            string query = @"   SELECT
                                    Symbol,
                                    Principle,
                                    FixedCapital,
                                    WinNumber,
                                    LoseNumber,
                                    Profit,
                                    TotalTax,
                                    IsBuy,
                                    BuyPrice,
                                    IsSell,
                                    SellPrice,
                                    TradingNotes,
                                    AssetPosition
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
                                    Principle,
                                    FixedCapital,
                                    WinNumber,
                                    LoseNumber,
                                    Profit,
                                    TotalTax,
                                    IsBuy,
                                    BuyPrice,
                                    IsSell,
                                    SellPrice,
                                    AssetPosition,
                                    TradingNotes,
                                    CreatedTime,
                                    UpdatedTime)
                                VALUES
                                    (@Id,
                                    @Symbol,
                                    @Principle,
                                    @FixedCapital,
                                    @WinNumber,
                                    @LoseNumber,
                                    @Profit,
                                    @TotalTax,
                                    @IsBuy,
                                    @BuyPrice,
                                    @IsSell,
                                    @SellPrice,
                                    @AssetPosition,
                                    @TradingNotes,
                                    GETDATE(),
                                    GETDATE());
                            END
                            ELSE
                            BEGIN
                                UPDATE TradingResults SET
                                    FixedCapital = @FixedCapital,
                                    WinNumber = @WinNumber,
                                    LoseNumber = @LoseNumber,
                                    Profit = @Profit,
                                    TotalTax = @TotalTax,
                                    IsBuy = @IsBuy,
                                    BuyPrice = @BuyPrice,
                                    IsSell = @IsSell,
                                    SellPrice = @SellPrice,
                                    AssetPosition = @AssetPosition,
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