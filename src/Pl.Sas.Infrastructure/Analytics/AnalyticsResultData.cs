using Dapper;
using Microsoft.Extensions.Options;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core;
using System.Data;
using Microsoft.Extensions.Logging;

namespace Pl.Sas.Infrastructure.Analytics
{
    public class AnalyticsResultData : BaseData, IAnalyticsResultData
    {
        private readonly ILogger<AnalyticsResultData> _logger;

        public AnalyticsResultData(ILogger<AnalyticsResultData> logger, IOptionsMonitor<ConnectionStrings> options) : base(options)
        {
            _logger = logger;
        }

        public virtual async Task<AnalyticsResult> FindAsync(string symbol)
        {
            string query = @"SELECT TOP(1) * FROM AnalyticsResults WHERE Symbol = @symbol ";
            using SqlConnection connection = new(_connectionStrings.AnalyticsConnection);
            return await connection.QueryFirstOrDefaultAsync<AnalyticsResult>(query, new { symbol });
        }

        public virtual async Task<bool> SaveSsaPerdictResultAsync(string symbol, decimal perdictResult)
        {
            var query = "UPDATE AnalyticsResults SET SsaPerdictPrice = @perdictResult, UpdatedTime = GETDATE() WHERE Symbol = @symbol ";
            using SqlConnection connection = new(_connectionStrings.AnalyticsConnection);
            return await connection.ExecuteAsync(query, new { symbol, perdictResult }) > 0;
        }

        public virtual async Task<bool> SaveFttPerdictResultAsync(string symbol, decimal perdictResult)
        {
            var query = "UPDATE AnalyticsResults SET FttPerdictPrice = @perdictResult, UpdatedTime = GETDATE() WHERE Symbol = @symbol ";
            using SqlConnection connection = new(_connectionStrings.AnalyticsConnection);
            return await connection.ExecuteAsync(query, new { symbol, perdictResult }) > 0;
        }

        public virtual async Task<bool> SaveSdcaTrendPerdictResultAsync(string symbol, int trendResult)
        {
            var query = "UPDATE AnalyticsResults SET SdcaPriceTrend = @trendResult, UpdatedTime = GETDATE() WHERE Symbol = @symbol ";
            using SqlConnection connection = new(_connectionStrings.AnalyticsConnection);
            return await connection.ExecuteAsync(query, new { symbol, trendResult }) > 0;
        }

        public virtual async Task<bool> SaveMacroeconomicsScoreAsync(string symbol, int marketScore, byte[] marketNotes)
        {
            var query = @"  UPDATE
                                AnalyticsResults
                            SET
                                MarketScore = @marketScore,
                                MarketNotes = @marketNotes,
                                UpdatedTime = GETDATE()
                            WHERE
                                Symbol = @symbol
                                ";
            using SqlConnection connection = new(_connectionStrings.AnalyticsConnection);
            return await connection.ExecuteAsync(query, new { symbol, marketScore, marketNotes }) > 0;
        }

        public virtual async Task<bool> SaveCompanyValueScoreAsync(string symbol, int companyValueScore, byte[] companyValueNote)
        {
            var query = @"  UPDATE
                                AnalyticsResults
                            SET
                                CompanyValueScore = @companyValueScore,
                                CompanyValueNote = @companyValueNote,
                                UpdatedTime = GETDATE()
                            WHERE
                                Symbol = @symbol
                                ";
            using SqlConnection connection = new(_connectionStrings.AnalyticsConnection);
            return await connection.ExecuteAsync(query, new { symbol, companyValueScore, companyValueNote }) > 0;
        }

        public virtual async Task<bool> SaveCompanyGrowthScoreAsync(string symbol, int companyGrowthScore, byte[] companyGrowthNote)
        {
            var query = @"  UPDATE
                                AnalyticsResults
                            SET
                                CompanyGrowthScore = @companyGrowthScore,
                                CompanyGrowthNote = @companyGrowthNote,
                                UpdatedTime = GETDATE()
                            WHERE
                                Symbol = @symbol
                                ";
            using SqlConnection connection = new(_connectionStrings.AnalyticsConnection);
            return await connection.ExecuteAsync(query, new { symbol, companyGrowthScore, companyGrowthNote }) > 0;
        }

        public virtual async Task<bool> SaveStockScoreAsync(string symbol, int stockScore, byte[] stockNote)
        {
            var query = @"  UPDATE
                                AnalyticsResults
                            SET
                                StockScore = @stockScore,
                                StockNote = @stockNote,
                                UpdatedTime = GETDATE()
                            WHERE
                                Symbol = @symbol
                                ";
            using SqlConnection connection = new(_connectionStrings.AnalyticsConnection);
            return await connection.ExecuteAsync(query, new { symbol, stockScore, stockNote }) > 0;
        }

        public virtual async Task<bool> SaveFiinScoreAsync(string symbol, int fiinScore, byte[] fiinNote)
        {
            var query = @"  UPDATE
                                AnalyticsResults
                            SET
                                FiinScore = @fiinScore,
                                FiinNote = @fiinNote,
                                UpdatedTime = GETDATE()
                            WHERE
                                Symbol = @symbol
                                ";
            using SqlConnection connection = new(_connectionStrings.AnalyticsConnection);
            return await connection.ExecuteAsync(query, new { symbol, fiinScore, fiinNote }) > 0;
        }

        public virtual async Task<bool> SaveVndScoreAsync(string symbol, int vndScore, byte[] vndNote)
        {
            var query = @"  UPDATE
                                AnalyticsResults
                            SET
                                VndScore = @vndScore,
                                VndNote = @vndNote,
                                UpdatedTime = GETDATE()
                            WHERE
                                Symbol = @symbol
                                ";
            using SqlConnection connection = new(_connectionStrings.AnalyticsConnection);
            return await connection.ExecuteAsync(query, new { symbol, vndScore, vndNote }) > 0;
        }

        public virtual async Task<bool> SaveTargetPriceAsync(string symbol, float targetPrice, byte[] targetPriceNote)
        {
            var query = @"  UPDATE
                                AnalyticsResults
                            SET
                                TargetPrice = @targetPrice,
                                TargetPriceNotes = @targetPriceNote,
                                UpdatedTime = GETDATE()
                            WHERE
                                Symbol = @symbol
                                ";
            using SqlConnection connection = new(_connectionStrings.AnalyticsConnection);
            return await connection.ExecuteAsync(query, new { symbol, targetPrice, targetPriceNote }) > 0;
        }

        public virtual async Task BulkInserAsync(IEnumerable<AnalyticsResult> analyticsResults)
        {
            if (analyticsResults is null || !analyticsResults.Any())
            {
                return;
            }

            var dataTableTemp = analyticsResults.ToDataTable();
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            connection.Open();
            using var tran = connection.BeginTransaction();
            try
            {
                await _dbAsyncRetry.ExecuteAsync(async () =>
                {
                    using (var sqlBulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, tran))
                    {
                        foreach (DataColumn column in dataTableTemp.Columns)
                        {
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(column.ColumnName, column.ColumnName));
                        }

                        sqlBulkCopy.BulkCopyTimeout = 300;
                        sqlBulkCopy.DestinationTableName = "AnalyticsResults";
                        await sqlBulkCopy.WriteToServerAsync(dataTableTemp);
                    }
                    tran.Commit();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AnalyticsResult BulkInserAsync error");
                tran.Rollback();
                throw;
            }
        }
    }
}