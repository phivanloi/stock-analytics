using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;
using Pl.Sas.Core;

namespace Pl.Sas.Infrastructure.Data
{
    public class FinancialGrowthData : BaseData, IFinancialGrowthData
    {
        private readonly ILogger<FinancialGrowthData> _logger;

        public FinancialGrowthData(ILogger<FinancialGrowthData> logger, IOptionsMonitor<ConnectionStrings> options) : base(options)
        {
            _logger = logger;
        }

        public virtual async Task<List<FinancialGrowth>> FindAllAsync(string symbol)
        {
            var query = "SELECT * FROM FinancialGrowths WHERE Symbol = @symbol ORDER BY Year";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return (await connection.QueryAsync<FinancialGrowth>(query, new { symbol })).AsList();
        }

        public virtual async Task<FinancialGrowth> GetLastAsync(string symbol)
        {
            var query = "SELECT TOP(1) * FROM FinancialGrowths WHERE Symbol = @symbol ORDER BY Year desc";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.QueryFirstOrDefaultAsync<FinancialGrowth>(query, new { symbol });
        }

        public virtual async Task BulkUpdateAsync(IEnumerable<FinancialGrowth> financialGrowths)
        {
            if (financialGrowths is null || !financialGrowths.Any())
            {
                return;
            }

            var dataTableUpdate = new DataTable();
            dataTableUpdate.Columns.Add("Id");
            dataTableUpdate.Columns.Add("Asset", typeof(decimal));
            dataTableUpdate.Columns.Add("ValuePershare", typeof(decimal));
            dataTableUpdate.Columns.Add("OwnerCapital", typeof(decimal));
            foreach (var financialGrowth in financialGrowths)
            {
                var row = dataTableUpdate.NewRow();
                row["Id"] = financialGrowth.Id;
                row["Asset"] = financialGrowth.Asset;
                row["ValuePershare"] = financialGrowth.ValuePershare;
                row["OwnerCapital"] = financialGrowth.OwnerCapital;
                dataTableUpdate.Rows.Add(row);
            }

            var tableName = Utilities.RandomString(15);
            var createTempTableCommand = $@" CREATE TABLE {tableName}
	                                            (Id nvarchar(22) NOT NULL,
                                                Asset decimal(35, 10) NOT NULL,
                                                ValuePershare decimal(35, 10) NOT NULL,
                                                OwnerCapital decimal(35, 10) NOT NULL)";
            var updateAndDropTableCommand = $@"  UPDATE
                                                    FinancialGrowths
                                                SET
                                                    FinancialGrowths.Asset = Tmp.Asset,
                                                    FinancialGrowths.ValuePershare = Tmp.ValuePershare,
                                                    FinancialGrowths.OwnerCapital = Tmp.OwnerCapital,
                                                    FinancialGrowths.UpdatedTime = GETDATE()
                                                FROM
                                                    FinancialGrowths Fgt
                                                INNER JOIN
                                                    {tableName} Tmp
                                                ON
                                                    Fgt.Id = Tmp.Id;
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

        public virtual async Task BulkInserAsync(IEnumerable<FinancialGrowth> financialGrowths)
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
                    sqlBulkCopy.DestinationTableName = "FinancialGrowths";
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
            var query = "DELETE FinancialGrowths WHERE Id = @id";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.ExecuteAsync(query, new { id }) > 0;
        }
    }
}