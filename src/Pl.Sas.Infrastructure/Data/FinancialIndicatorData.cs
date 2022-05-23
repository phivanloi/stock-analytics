using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pl.Sas.Core.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core;
using Ardalis.GuardClauses;

namespace Pl.Sas.Infrastructure.Data
{
    public class FinancialIndicatorData : BaseData, IFinancialIndicatorData
    {
        private readonly ILogger<FinancialIndicatorData> _logger;
        private readonly IAsyncCacheService _asyncCacheService;

        public FinancialIndicatorData(
            IAsyncCacheService asyncCacheService,
            ILogger<FinancialIndicatorData> logger,
            IOptionsMonitor<ConnectionStrings> options) : base(options)
        {
            _asyncCacheService = asyncCacheService;
            _logger = logger;
        }

        public virtual async Task<IEnumerable<FinancialIndicator>> FindAllAsync(string symbol, int? yearReport = null, int? lengthReport = null)
        {
            var query = "SELECT * FROM FinancialIndicators WHERE Symbol = @symbol ";
            if (yearReport.HasValue)
            {
                query += " AND YearReport = @yearReport";
            }
            if (lengthReport.HasValue)
            {
                query += " AND LengthReport = @lengthReport";
            }

            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.QueryAsync<FinancialIndicator>($"{query} ORDER BY YearReport, LengthReport", new { symbol, yearReport, lengthReport });
        }

        public virtual async Task<List<FinancialIndicator>> GetTopYearlyAsync(string symbol, int top = 5)
        {
            var query = "SELECT TOP(@top) * FROM FinancialIndicators WHERE Symbol = @symbol AND LengthReport = 5 ";

            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return (await connection.QueryAsync<FinancialIndicator>($"{query} ORDER BY YearReport DESC", new { symbol, top })).AsList();
        }

        public virtual async Task<FinancialIndicator> GetLastYearAsync(string symbol)
        {
            var query = "SELECT TOP(1) * FROM FinancialIndicators WHERE Symbol = @symbol AND LengthReport != 5 ORDER BY YearReport DESC, LengthReport DESC";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.QueryFirstOrDefaultAsync<FinancialIndicator>(query, new { symbol });
        }

        public virtual async Task BulkUpdateAsync(IEnumerable<FinancialIndicator> financialIndicators)
        {
            if (financialIndicators is null || !financialIndicators.Any())
            {
                return;
            }

            var dataTableUpdate = new DataTable();
            dataTableUpdate.Columns.Add("Id");
            dataTableUpdate.Columns.Add("Revenue", typeof(float));
            dataTableUpdate.Columns.Add("Profit", typeof(float));
            dataTableUpdate.Columns.Add("Eps", typeof(float));
            dataTableUpdate.Columns.Add("DilutedEps", typeof(float));
            dataTableUpdate.Columns.Add("Pe", typeof(float));
            dataTableUpdate.Columns.Add("DilutedPe", typeof(float));
            dataTableUpdate.Columns.Add("Roe", typeof(float));
            dataTableUpdate.Columns.Add("Roa", typeof(float));
            dataTableUpdate.Columns.Add("Roic", typeof(float));
            dataTableUpdate.Columns.Add("GrossProfitMargin", typeof(float));
            dataTableUpdate.Columns.Add("NetProfitMargin", typeof(float));
            dataTableUpdate.Columns.Add("DebtEquity", typeof(float));
            dataTableUpdate.Columns.Add("DebtAsset", typeof(float));
            dataTableUpdate.Columns.Add("QuickRatio", typeof(float));
            dataTableUpdate.Columns.Add("CurrentRatio", typeof(float));
            dataTableUpdate.Columns.Add("Pb", typeof(float));
            foreach (var financialIndicator in financialIndicators)
            {
                var row = dataTableUpdate.NewRow();
                row["Id"] = financialIndicator.Id;
                row["Revenue"] = financialIndicator.Revenue;
                row["Profit"] = financialIndicator.Profit;
                row["Eps"] = financialIndicator.Eps;
                row["DilutedEps"] = financialIndicator.DilutedEps;
                row["Pe"] = financialIndicator.Pe;
                row["DilutedPe"] = financialIndicator.DilutedPe;
                row["Roe"] = financialIndicator.Roe;
                row["Roa"] = financialIndicator.Roa;
                row["Roic"] = financialIndicator.Roic;
                row["GrossProfitMargin"] = financialIndicator.GrossProfitMargin;
                row["NetProfitMargin"] = financialIndicator.NetProfitMargin;
                row["DebtEquity"] = financialIndicator.DebtEquity;
                row["DebtAsset"] = financialIndicator.DebtAsset;
                row["QuickRatio"] = financialIndicator.QuickRatio;
                row["CurrentRatio"] = financialIndicator.CurrentRatio;
                row["Pb"] = financialIndicator.Pb;
                dataTableUpdate.Rows.Add(row);
            }

            var tableName = Utilities.RandomString(15);
            var createTempTableCommand = $@" CREATE TABLE {tableName}
	                                            (Id nvarchar(22) NOT NULL,
                                                Revenue decimal(35, 10) NOT NULL,
                                                Profit decimal(35, 10) NOT NULL,
                                                Eps decimal(35, 10) NOT NULL,
                                                DilutedEps decimal(35, 10) NOT NULL,
                                                Pe decimal(35, 10) NOT NULL,
                                                DilutedPe decimal(35, 10) NOT NULL,
                                                Roe decimal(35, 10) NOT NULL,
                                                Roa decimal(35, 10) NOT NULL,
                                                Roic decimal(35, 10) NOT NULL,
                                                GrossProfitMargin decimal(35, 10) NOT NULL,
                                                NetProfitMargin decimal(35, 10) NOT NULL,
                                                DebtEquity decimal(35, 10) NOT NULL,
                                                DebtAsset decimal(35, 10) NOT NULL,
                                                QuickRatio decimal(35, 10) NOT NULL,
                                                CurrentRatio decimal(35, 10) NOT NULL,
                                                Pb decimal(35, 10) NOT NULL)";
            var updateAndDropTableCommand = $@"  UPDATE
                                                    FinancialIndicators
                                                SET
                                                    FinancialIndicators.Revenue = Tmp.Revenue,
                                                    FinancialIndicators.Profit = Tmp.Profit,
                                                    FinancialIndicators.Eps = Tmp.Eps,
                                                    FinancialIndicators.DilutedEps = Tmp.DilutedEps,
                                                    FinancialIndicators.Pe = Tmp.Pe,
                                                    FinancialIndicators.DilutedPe = Tmp.DilutedPe,
                                                    FinancialIndicators.Roe = Tmp.Roe,
                                                    FinancialIndicators.Roa = Tmp.Roa,
                                                    FinancialIndicators.Roic = Tmp.Roic,
                                                    FinancialIndicators.GrossProfitMargin = Tmp.GrossProfitMargin,
                                                    FinancialIndicators.NetProfitMargin = Tmp.NetProfitMargin,
                                                    FinancialIndicators.DebtEquity = Tmp.DebtEquity,
                                                    FinancialIndicators.DebtAsset = Tmp.DebtAsset,
                                                    FinancialIndicators.QuickRatio = Tmp.QuickRatio,
                                                    FinancialIndicators.CurrentRatio = Tmp.CurrentRatio,
                                                    FinancialIndicators.Pb = Tmp.Pb,
                                                    FinancialIndicators.UpdatedTime = GETDATE()
                                                FROM
                                                    FinancialIndicators Fit
                                                INNER JOIN
                                                    {tableName} Tmp
                                                ON
                                                    Fit.Id = Tmp.Id;
                                                DROP TABLE {tableName};";

            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            connection.Open();
            using var tran = connection.BeginTransaction();
            try
            {
                using (SqlCommand command = new(createTempTableCommand, connection, tran))
                {
                    await command.ExecuteNonQueryAsync();

                    using (var bulk = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, tran))
                    {
                        bulk.DestinationTableName = tableName;
                        await bulk.WriteToServerAsync(dataTableUpdate);
                    }

                    command.CommandText = updateAndDropTableCommand;
                    await command.ExecuteNonQueryAsync();
                }
                tran.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BulkUpdateAsync error");
                tran.Rollback();
                throw;
            }
        }

        public virtual async Task BulkInserAsync(IEnumerable<FinancialIndicator> financialGrowths)
        {
            if (financialGrowths is null || !financialGrowths.Any())
            {
                return;
            }

            var dataTableTemp = financialGrowths.ToDataTable();
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            connection.Open();
            using var tran = connection.BeginTransaction();
            try
            {
                using (var sqlBulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, tran))
                {
                    foreach (DataColumn column in dataTableTemp.Columns)
                    {
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(column.ColumnName, column.ColumnName));
                    }

                    sqlBulkCopy.BulkCopyTimeout = 300;
                    sqlBulkCopy.DestinationTableName = "FinancialIndicators";
                    await sqlBulkCopy.WriteToServerAsync(dataTableTemp);
                }
                tran.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BulkInserAsync error");
                tran.Rollback();
                throw;
            }
        }

        public virtual async Task<bool> DeleteAsync(string id)
        {
            var query = "DELETE FinancialIndicators WHERE Id = @id";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.ExecuteAsync(query, new { id }) > 0;
        }

        public virtual async Task<List<FinancialIndicator>?> CacheGetBySymbolsAsync(string industryCode, string[] symbols, int yearRanger = 5)
        {
            Guard.Against.NullOrEmpty(industryCode, nameof(industryCode));
            var cacheKey = $"{Constants.FinancialIndicatorCachePrefix}-INC{industryCode}-YR{yearRanger}";
            return await _asyncCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                var query = @"  SELECT 
                                    * 
                                FROM 
                                    FinancialIndicators 
                                WHERE 
                                    Symbol = @symbol 
                                    AND YearReport >= @year
                                    AND Symbol IN @symbols
                                ORDER BY YearReport, LengthReport";
                using SqlConnection connection = new(_connectionStrings.MarketConnection);
                return (await connection.QueryAsync<FinancialIndicator>(query, new { symbols, year = DateTime.Now.Year - yearRanger })).AsList();
            }, Constants.DefaultCacheTime * 60 * 24);
        }
    }
}