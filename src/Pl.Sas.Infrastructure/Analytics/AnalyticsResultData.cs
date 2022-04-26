using Dapper;
using Microsoft.Extensions.Options;
using Pl.Sas.Core;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Pl.Sas.Infrastructure.Analytics
{
    public class AnalyticsResultData : IAnalyticsResultData
    {
        private readonly Connections _connections;

        public AnalyticsResultData(
            IOptions<Connections> options)
        {
            _connections = options.Value;
        }

        public virtual async Task<List<AnalyticsResult>> FindAllAsync(string symbol, string datePath = null, int numberItem = 10)
        {
            string query = @"SELECT TOP(@numberItem) * FROM AnalyticsResults WHERE Symbol = @symbol";
            if (!string.IsNullOrEmpty(datePath))
            {
                query += $" AND DatePath = @datePath";
            }
            query += " ORDER BY TradingDate DESC";
            using SqlConnection connection = new(_connections.AnalyticsConnection);
            return (await connection.QueryAsync<AnalyticsResult>(query, new { datePath, symbol, numberItem })).AsList();
        }

        public virtual async Task<AnalyticsResult> FindAsync(string symbol, string datePath)
        {
            string query = @"SELECT TOP(1) * FROM AnalyticsResults WHERE Symbol = @symbol AND DatePath = @datePath";
            using SqlConnection connection = new(_connections.AnalyticsConnection);
            return await connection.QueryFirstOrDefaultAsync<AnalyticsResult>(query, new { datePath, symbol });
        }

        public virtual async Task<bool> SaveSsaPerdictResultAsync(string symbol, string datePath, decimal perdictResult)
        {
            await MakeSureReadyForUpdateAsync(symbol, datePath);
            var query = "UPDATE AnalyticsResults SET SsaPerdictPrice = @perdictResult, UpdatedTime = GETDATE() WHERE Symbol = @symbol AND DatePath = @datePath";
            using SqlConnection connection = new(_connections.AnalyticsConnection);
            return await connection.ExecuteAsync(query, new { symbol, datePath, perdictResult }) > 0;
        }

        public virtual async Task<bool> SaveFttPerdictResultAsync(string symbol, string datePath, decimal perdictResult)
        {
            await MakeSureReadyForUpdateAsync(symbol, datePath);
            var query = "UPDATE AnalyticsResults SET FttPerdictPrice = @perdictResult, UpdatedTime = GETDATE() WHERE Symbol = @symbol AND DatePath = @datePath";
            using SqlConnection connection = new(_connections.AnalyticsConnection);
            return await connection.ExecuteAsync(query, new { symbol, datePath, perdictResult }) > 0;
        }

        public virtual async Task<bool> SaveSdcaTrendPerdictResultAsync(string symbol, string datePath, int trendResult)
        {
            await MakeSureReadyForUpdateAsync(symbol, datePath);
            var query = "UPDATE AnalyticsResults SET SdcaPriceTrend = @trendResult, UpdatedTime = GETDATE() WHERE Symbol = @symbol AND DatePath = @datePath";
            using SqlConnection connection = new(_connections.AnalyticsConnection);
            return await connection.ExecuteAsync(query, new { symbol, datePath, trendResult }) > 0;
        }

        public virtual async Task<bool> SaveMacroeconomicsScoreAsync(string symbol, string datePath, int macroeconomicsScore, byte[] macroeconomicsNote)
        {
            await MakeSureReadyForUpdateAsync(symbol, datePath);
            var query = @"  UPDATE
                                AnalyticsResults
                            SET
                                MacroeconomicsScore = @macroeconomicsScore,
                                MacroeconomicsNote = @macroeconomicsNote,
                                UpdatedTime = GETDATE()
                            WHERE
                                Symbol = @symbol
                                AND DatePath = @datePath";
            using SqlConnection connection = new(_connections.AnalyticsConnection);
            return await connection.ExecuteAsync(query, new { symbol, datePath, macroeconomicsScore, macroeconomicsNote }) > 0;
        }

        public virtual async Task<bool> SaveCompanyValueScoreAsync(string symbol, string datePath, int companyValueScore, byte[] companyValueNote)
        {
            await MakeSureReadyForUpdateAsync(symbol, datePath);
            var query = @"  UPDATE
                                AnalyticsResults
                            SET
                                CompanyValueScore = @companyValueScore,
                                CompanyValueNote = @companyValueNote,
                                UpdatedTime = GETDATE()
                            WHERE
                                Symbol = @symbol
                                AND DatePath = @datePath";
            using SqlConnection connection = new(_connections.AnalyticsConnection);
            return await connection.ExecuteAsync(query, new { symbol, datePath, companyValueScore, companyValueNote }) > 0;
        }

        public virtual async Task<bool> SaveCompanyGrowthScoreAsync(string symbol, string datePath, int companyGrowthScore, byte[] companyGrowthNote)
        {
            await MakeSureReadyForUpdateAsync(symbol, datePath);
            var query = @"  UPDATE
                                AnalyticsResults
                            SET
                                CompanyGrowthScore = @companyGrowthScore,
                                CompanyGrowthNote = @companyGrowthNote,
                                UpdatedTime = GETDATE()
                            WHERE
                                Symbol = @symbol
                                AND DatePath = @datePath";
            using SqlConnection connection = new(_connections.AnalyticsConnection);
            return await connection.ExecuteAsync(query, new { symbol, datePath, companyGrowthScore, companyGrowthNote }) > 0;
        }

        public virtual async Task<bool> SaveStockScoreAsync(string symbol, string datePath, int stockScore, byte[] stockNote)
        {
            await MakeSureReadyForUpdateAsync(symbol, datePath);
            var query = @"  UPDATE
                                AnalyticsResults
                            SET
                                StockScore = @stockScore,
                                StockNote = @stockNote,
                                UpdatedTime = GETDATE()
                            WHERE
                                Symbol = @symbol
                                AND DatePath = @datePath";
            using SqlConnection connection = new(_connections.AnalyticsConnection);
            return await connection.ExecuteAsync(query, new { symbol, datePath, stockScore, stockNote }) > 0;
        }

        public virtual async Task<bool> SaveFiinScoreAsync(string symbol, string datePath, int fiinScore, byte[] fiinNote)
        {
            await MakeSureReadyForUpdateAsync(symbol, datePath);
            var query = @"  UPDATE
                                AnalyticsResults
                            SET
                                FiinScore = @fiinScore,
                                FiinNote = @fiinNote,
                                UpdatedTime = GETDATE()
                            WHERE
                                Symbol = @symbol
                                AND DatePath = @datePath";
            using SqlConnection connection = new(_connections.AnalyticsConnection);
            return await connection.ExecuteAsync(query, new { symbol, datePath, fiinScore, fiinNote }) > 0;
        }

        public virtual async Task<bool> SaveVndScoreAsync(string symbol, string datePath, int vndScore, byte[] vndNote)
        {
            await MakeSureReadyForUpdateAsync(symbol, datePath);
            var query = @"  UPDATE
                                AnalyticsResults
                            SET
                                VndScore = @vndScore,
                                VndNote = @vndNote,
                                UpdatedTime = GETDATE()
                            WHERE
                                Symbol = @symbol
                                AND DatePath = @datePath";
            using SqlConnection connection = new(_connections.AnalyticsConnection);
            return await connection.ExecuteAsync(query, new { symbol, datePath, vndScore, vndNote }) > 0;
        }

        public virtual async Task<bool> SaveTargetPriceAsync(string symbol, string datePath, decimal targetPrice, byte[] targetPriceNote)
        {
            await MakeSureReadyForUpdateAsync(symbol, datePath);
            var query = @"  UPDATE
                                AnalyticsResults
                            SET
                                TargetPrice = @targetPrice,
                                TargetPriceNotes = @targetPriceNote,
                                UpdatedTime = GETDATE()
                            WHERE
                                Symbol = @symbol
                                AND DatePath = @datePath";
            using SqlConnection connection = new(_connections.AnalyticsConnection);
            return await connection.ExecuteAsync(query, new { symbol, datePath, targetPrice, targetPriceNote }) > 0;
        }

        /// <summary>
        /// Đảm bảo đã có một bản ghi phân tích cho ngày được truyền vào
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <param name="datePath">Phiên đánh giá</param>
        /// <returns>bool</returns>
        private async Task MakeSureReadyForUpdateAsync(string symbol, string datePath)
        {
            var id = Utilities.GenerateShortGuid();
            var query = $@" SET TRANSACTION ISOLATION LEVEL REPEATABLE READ
                            BEGIN TRAN
                            DECLARE @analyticsId AS NVARCHAR(22);
                            SET @analyticsId = (SELECT TOP(1) Id FROM AnalyticsResults WHERE Symbol = @symbol AND DatePath = @datePath);
                            IF @analyticsId IS NULL
                            BEGIN
                                INSERT INTO AnalyticsResults
                                    (Id,
                                    Symbol,
                                    TradingDate,
                                    DatePath,
                                    SsaPerdictPrice,
                                    FttPerdictPrice,
                                    SdcaPriceTrend,
                                    MacroeconomicsScore,
                                    MacroeconomicsNote,
                                    CompanyValueScore,
                                    CompanyValueNote,
                                    CompanyGrowthScore,
                                    CompanyGrowthNote,
                                    StockScore,
                                    StockNote,
                                    FiinScore,
                                    FiinNote,
                                    VndScore,
                                    VndNote,
                                    TargetPrice,
                                    TargetPriceNotes,
                                    CreatedTime,
                                    UpdatedTime)
                                VALUES
                                    (@id,
                                    @symbol,
                                    @tradingDate,
                                    @datePath,
                                    0,
                                    0,
                                    0,
                                    0,
                                    NULL,
                                    -100,
                                    NULL,
                                    -100,
                                    NULL,
                                    -100,
                                    NULL,
                                    -100,
                                    NULL,
                                    -100,
                                    NULL,
                                    -100,
                                    NULL,
                                    GETDATE(),
                                    GETDATE());
                            END
                            COMMIT";
            using SqlConnection connection = new(_connections.AnalyticsConnection);
            await connection.ExecuteAsync(query, new { symbol, datePath, tradingDate = Utilities.GetTradingDate(), id });
        }
    }
}