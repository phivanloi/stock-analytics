using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pl.Sas.Core;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Pl.Sas.Infrastructure.Data
{
    public class StockData : BaseData, IStockData
    {
        private readonly ILogger<StockData> _logger;

        public StockData(ILogger<StockData> logger, IOptionsMonitor<ConnectionStrings> options) : base(options)
        {
            _logger = logger;
        }

        public virtual async Task BulkInserAsync(List<Stock> stocks)
        {
            if (stocks is null || stocks.Count <= 0)
            {
                return;
            }

            var dataTableTemp = stocks.ToDataTable();

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
                    sqlBulkCopy.DestinationTableName = "Stocks";
                    await sqlBulkCopy.WriteToServerAsync(dataTableTemp);
                }
                tran.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stock data error => BulkInserStockAsync");
                tran.Rollback();
                throw;
            }
        }

        public virtual async Task BulkUpdateAsync(List<Stock> stocks)
        {
            if (stocks is null || stocks.Count <= 0)
            {
                return;
            }

            var dataTableUpdate = new DataTable();
            dataTableUpdate.Columns.Add("Id");
            dataTableUpdate.Columns.Add("Name");
            dataTableUpdate.Columns.Add("FullName");
            dataTableUpdate.Columns.Add("Description");
            dataTableUpdate.Columns.Add("Exchange");
            dataTableUpdate.Columns.Add("Type");
            dataTableUpdate.Columns.Add("CompanyName");
            dataTableUpdate.Columns.Add("CompanyNameEn");
            foreach (var company in stocks)
            {
                var row = dataTableUpdate.NewRow();
                row["Id"] = company.Id;
                row["Name"] = company.Name;
                row["FullName"] = company.FullName;
                row["Exchange"] = company.Exchange;
                row["Type"] = company.Type;
                dataTableUpdate.Rows.Add(row);
            }

            var tableName = Utilities.RandomString(15);
            var createTempTableCommand = $@" CREATE TABLE {tableName}
	                                            (Id nvarchar(22) NOT NULL,
	                                            Name nvarchar(128) NULL,
	                                            FullName nvarchar(256) NULL,
                                                Exchange nvarchar(16) NULL,
                                                Type nvarchar(16) NULL";
            var updateAndDropTableCommand = $@"  UPDATE
                                                    Stocks
                                                SET
                                                    Stocks.Name = Ttb.Name,
                                                    Stocks.FullName = Ttb.FullName,
                                                    Stocks.Exchange = Ttb.Exchange,
                                                    Stocks.Type = Ttb.Type,
                                                    Stocks.UpdatedTime = GETDATE()
                                                FROM
                                                    Stocks Sto
                                                INNER JOIN
                                                    {tableName} Ttb
                                                ON
                                                    Sto.Id = Ttb.Id;
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
                _logger.LogError(ex, "Stock data error => BulkUpdateStockAsync");
                tran.Rollback();
                throw;
            }
        }

        public virtual async Task<IEnumerable<Stock>> FindAllAsync()
        {
            var query = "SELECT * FROM Stocks WHERE [Type] = 'stock'";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.QueryAsync<Stock>(query);
        }

        public virtual async Task<IEnumerable<string>> GetCodeForBuildTrainingDataAsync()
        {
            var query = "SELECT Code FROM Stocks WHERE Type = 'stock'";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.QueryAsync<string>(query);
        }

        public virtual async Task<Stock> GetByCodeAsync(string symbol)
        {
            var query = "SELECT * FROM Stocks WHERE Code = @symbol";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.QueryFirstOrDefaultAsync<Stock>(query, new { symbol });
        }

        public virtual async Task<bool> DeleteAsync(string id)
        {
            var query = "DELETE Stocks WHERE Id = @id";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.ExecuteAsync(query, new { id }) > 0;
        }

        public virtual async Task<List<string>> GetExchanges()
        {
            var query = "SELECT Exchange FROM Stocks WHERE [Type] = 'stock' GROUP BY Exchange";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return (await connection.QueryAsync<string>(query)).AsList();
        }
    }
}