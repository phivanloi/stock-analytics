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
    public class LeadershipData : BaseData, ILeadershipData
    {
        private readonly ILogger<LeadershipData> _logger;

        public LeadershipData(ILogger<LeadershipData> logger, IOptionsMonitor<ConnectionStrings> options) : base(options)
        {
            _logger = logger;
        }

        public virtual async Task BulkInserAsync(List<Leadership> leaderships)
        {
            if (leaderships is null || leaderships.Count <= 0)
            {
                return;
            }

            var dataTableTemp = leaderships.ToDataTable();

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
                    sqlBulkCopy.DestinationTableName = "Leaderships";
                    await sqlBulkCopy.WriteToServerAsync(dataTableTemp);
                }
                tran.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Leadership data error => BulkInserLeadershipAsync");
                tran.Rollback();
                throw;
            }
        }

        public virtual async Task BulkUpdateAsync(List<Leadership> leaderships)
        {
            if (leaderships is null || leaderships.Count <= 0)
            {
                return;
            }

            var dataTableUpdate = new DataTable();
            dataTableUpdate.Columns.Add("Id");
            dataTableUpdate.Columns.Add("Activated", typeof(bool));
            dataTableUpdate.Columns.Add("PositionLevel");
            foreach (var leadership in leaderships)
            {
                var row = dataTableUpdate.NewRow();
                row["Id"] = leadership.Id;
                row["PositionLevel"] = leadership.PositionLevel;
                dataTableUpdate.Rows.Add(row);
            }

            var tableName = Utilities.RandomString(15);
            var createTempTableCommand = $@" CREATE TABLE {tableName}
	                                            (Id nvarchar(22) NOT NULL,
                                                PositionLevel nvarchar(128) NULL)";

            var updateAndDropTableCommand = $@"  UPDATE
                                                    Leaderships
                                                SET
                                                    Leaderships.PositionLevel = Tmt.PositionLevel,
                                                    Leaderships.UpdatedTime = GETDATE()
                                                FROM
                                                    Leaderships Lea
                                                INNER JOIN
                                                    {tableName} Tmt
                                                ON
                                                    Lea.Id = Tmt.Id;
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
                _logger.LogError(ex, "Leadership data error => BulkUpdateLeadershipAsync");
                tran.Rollback();
                throw;
            }
        }

        public virtual async Task<IReadOnlyList<Leadership>> FindAllAsync(string symbol, bool? activated = null)
        {
            var query = "SELECT * FROM Leaderships WHERE Symbol = @symbol ";
            if (activated.HasValue)
            {
                query += " AND Activated = @activated ";
            }
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return (await connection.QueryAsync<Leadership>(query, new { symbol, activated })).ToList();
        }

        public virtual async Task<bool> DeleteAsync(string id)
        {
            var query = "DELETE Leaderships WHERE Id = @id";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.ExecuteAsync(query, new { id }) > 0;
        }
    }
}