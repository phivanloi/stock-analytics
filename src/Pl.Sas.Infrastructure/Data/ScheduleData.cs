using Ardalis.GuardClauses;
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
    public class ScheduleData : BaseData, IScheduleData
    {
        private readonly ILogger<ScheduleData> _logger;

        public ScheduleData(ILogger<ScheduleData> logger, IOptionsMonitor<ConnectionStrings> options) : base(options)
        {
            _logger = logger;
        }

        public virtual async Task<IReadOnlyList<Schedule>> GetForActiveEventAsync(DateTime selectTime, int top)
        {
            var query = "SELECT TOP(@top) Id, Type, OptionsJson FROM Schedules WHERE ActiveTime <= @selectTime";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return (await connection.QueryAsync<Schedule>(query, new { top, selectTime })).AsList();
        }

        public virtual async Task<bool> SetActiveTimeAsync(string id, DateTime setTime)
        {
            var query = "UPDATE Schedules SET ActiveTime = @setTime, UpdatedTime = GETDATE() WHERE Id = @id";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await _dbAsyncRetry.ExecuteAsync(async () =>
            {
                return await connection.ExecuteAsync(query, new { id, setTime }) > 0;
            });
        }

        public virtual async Task BulkSetActiveTimeAsync(IEnumerable<Schedule> schedules)
        {
            if (schedules is null || !schedules.Any())
            {
                return;
            }

            var dataTableUpdate = new DataTable();
            dataTableUpdate.Columns.Add("Id");
            dataTableUpdate.Columns.Add("ActiveTime", typeof(DateTime));
            foreach (var schedule in schedules)
            {
                var row = dataTableUpdate.NewRow();
                row["Id"] = schedule.Id;
                row["ActiveTime"] = schedule.ActiveTime;
                dataTableUpdate.Rows.Add(row);
            }

            var tableName = Utilities.RandomString(15);
            var createTempTableCommand = $@" CREATE TABLE {tableName}
	                                            (Id nvarchar(22) NOT NULL,
	                                            ActiveTime datetime NOT NULL)";
            var updateAndDropTableCommand = $@"  UPDATE
                                                    Schedules
                                                SET
                                                    Schedules.ActiveTime = Tmt.ActiveTime,
                                                    Schedules.UpdatedTime = GETDATE()
                                                FROM
                                                    Schedules Sch
                                                INNER JOIN
                                                    {tableName} Tmt
                                                ON
                                                    Sch.Id = Tmt.Id;
                                                DROP TABLE {tableName};";

            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            connection.Open();
            using var tran = connection.BeginTransaction();
            try
            {
                await _dbAsyncRetry.ExecuteAsync(async () =>
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
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BulkSetActiveTimeAsync error");
                tran.Rollback();
                throw;
            }
        }

        public virtual async Task<Schedule> GetByIdAsync(string id)
        {
            var query = "SELECT * FROM Schedules WHERE Id = @id";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await _dbAsyncRetry.ExecuteAsync(async () =>
            {
                return await connection.QueryFirstOrDefaultAsync<Schedule>(query, new { id });
            });
        }

        public virtual async Task<Schedule?> FindAsync(int type, string dataKey)
        {
            Guard.Against.NullOrEmpty(dataKey, nameof(dataKey));
            var query = "SELECT * FROM Schedules WHERE DataKey = @dataKey AND Type = @type";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.QueryFirstOrDefaultAsync<Schedule>(query, new { type, dataKey });
        }

        public virtual async Task<bool> UpdateAsync(Schedule schedule)
        {
            string query = @"    UPDATE Schedules SET
			                            Name = @Name,
			                            DataKey = @DataKey,
			                            Type = @Type,
			                            Activated = @Activated,
			                            ActiveTime = @ActiveTime,
		                                IsError = @IsError,
		                                OptionsJson = @OptionsJson,
                                        UpdatedTime = GETDATE()
		                            WHERE
			                            Id = @Id";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.ExecuteAsync(query, schedule) > 0;
        }

        public virtual async Task BulkInserAsync(IEnumerable<Schedule> schedules)
        {
            if (schedules is null || !schedules.Any())
            {
                return;
            }

            var dataTableTemp = schedules.ToDataTable();
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
                        sqlBulkCopy.DestinationTableName = "Schedules";
                        await sqlBulkCopy.WriteToServerAsync(dataTableTemp);
                    }
                    tran.Commit();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BulkInserAsync error");
                tran.Rollback();
                throw;
            }
        }

        public virtual async Task<bool> InsertAsync(Schedule schedule)
        {
            var query = $@"   INSERT INTO Schedules
                                        (Id,
                                        Name,
                                        DataKey,
                                        Activated,
                                        ActiveTime,
                                        IsError,
                                        OptionsJson,
                                        Type,
                                        CreatedTime,
                                        UpdatedTime)
                                    VALUES
                                        (@Id,
                                        @Name,
                                        @DataKey,
                                        @Activated,
                                        @ActiveTime,
                                        @IsError,
                                        @OptionsJson,
                                        @Type,
                                        @CreatedTime,
                                        GETDATE())";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.ExecuteAsync(query, schedule) > 0;
        }

        public virtual async Task<bool> DeleteAsync(string id)
        {
            var query = "DELETE Schedules WHERE Id = @id";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.ExecuteAsync(query, new { id }) > 0;
        }

        public virtual async Task<bool> UtilityUpdateAsync(int type, string code)
        {
            string query = @"   UPDATE Schedules SET
			                        ActiveTime = GETDATE(),
                                    UpdatedTime = GETDATE()
		                        WHERE
			                        Type = @type ";
            if (!string.IsNullOrEmpty(code))
            {
                query += " AND DataKey = @code";
            }
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.ExecuteAsync(query, new { type, code }) > 0;
        }
    }
}