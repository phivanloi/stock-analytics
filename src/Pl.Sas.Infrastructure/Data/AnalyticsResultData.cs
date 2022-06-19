using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pl.Sas.Core;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using System.Data;

namespace Pl.Sas.Infrastructure.Data
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

        public virtual async Task<bool> SaveMarketScoreAsync(string symbol, int marketScore, byte[] marketNotes)
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

        public virtual async Task<bool> SaveCompanyValueScoreAsync(string symbol, int companyValueScore, byte[] companyValueNotes)
        {
            var query = @"  UPDATE
                                AnalyticsResults
                            SET
                                CompanyValueScore = @companyValueScore,
                                CompanyValueNotes = @companyValueNotes,
                                UpdatedTime = GETDATE()
                            WHERE
                                Symbol = @symbol
                                ";
            using SqlConnection connection = new(_connectionStrings.AnalyticsConnection);
            return await connection.ExecuteAsync(query, new { symbol, companyValueScore, companyValueNotes }) > 0;
        }

        public virtual async Task<bool> SaveCompanyGrowthScoreAsync(string symbol, int companyGrowthScore, byte[] companyGrowthNotes)
        {
            var query = @"  UPDATE
                                AnalyticsResults
                            SET
                                CompanyGrowthScore = @companyGrowthScore,
                                CompanyGrowthNotes = @companyGrowthNotes,
                                UpdatedTime = GETDATE()
                            WHERE
                                Symbol = @symbol
                                ";
            using SqlConnection connection = new(_connectionStrings.AnalyticsConnection);
            return await connection.ExecuteAsync(query, new { symbol, companyGrowthScore, companyGrowthNotes }) > 0;
        }

        public virtual async Task<bool> SaveStockScoreAsync(string symbol, int stockScore, byte[] stockNotes)
        {
            var query = @"  UPDATE
                                AnalyticsResults
                            SET
                                StockScore = @stockScore,
                                StockNotes = @stockNotes,
                                UpdatedTime = GETDATE()
                            WHERE
                                Symbol = @symbol
                                ";
            using SqlConnection connection = new(_connectionStrings.AnalyticsConnection);
            return await connection.ExecuteAsync(query, new { symbol, stockScore, stockNotes }) > 0;
        }

        public virtual async Task<bool> SaveFiinScoreAsync(string symbol, int fiinScore, byte[] fiinNotes)
        {
            var query = @"  UPDATE
                                AnalyticsResults
                            SET
                                FiinScore = @fiinScore,
                                FiinNotes = @fiinNotes,
                                UpdatedTime = GETDATE()
                            WHERE
                                Symbol = @symbol
                                ";
            using SqlConnection connection = new(_connectionStrings.AnalyticsConnection);
            return await connection.ExecuteAsync(query, new { symbol, fiinScore, fiinNotes }) > 0;
        }

        public virtual async Task<bool> SaveVndScoreAsync(string symbol, int vndScore, byte[] vndNotes)
        {
            var query = @"  UPDATE
                                AnalyticsResults
                            SET
                                VndScore = @vndScore,
                                VndNotes = @vndNotes,
                                UpdatedTime = GETDATE()
                            WHERE
                                Symbol = @symbol
                                ";
            using SqlConnection connection = new(_connectionStrings.AnalyticsConnection);
            return await connection.ExecuteAsync(query, new { symbol, vndScore, vndNotes }) > 0;
        }

        public virtual async Task<bool> SaveTargetPriceAsync(string symbol, float targetPrice, byte[] targetPriceNotes)
        {
            var query = @"  UPDATE
                                AnalyticsResults
                            SET
                                TargetPrice = @targetPrice,
                                TargetPriceNotes = @targetPriceNotes,
                                UpdatedTime = GETDATE()
                            WHERE
                                Symbol = @symbol
                                ";
            using SqlConnection connection = new(_connectionStrings.AnalyticsConnection);
            return await connection.ExecuteAsync(query, new { symbol, targetPrice, targetPriceNotes }) > 0;
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