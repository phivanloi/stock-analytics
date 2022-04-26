using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pl.Sas.Core;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pl.Sas.Infrastructure.Analytics
{
    public class ScheduleData : IScheduleData
    {
        private readonly Connections _connections;
        private readonly ILogger<ScheduleData> _logger;

        public ScheduleData(
            ILogger<ScheduleData> logger,
            IOptions<Connections> options)
        {
            _connections = options.Value;
            _logger = logger;
        }

        public virtual async Task<IReadOnlyList<Schedule>> GetScheduleIdForActiveEventAsync(DateTime selectTime, int top)
        {
            var query = "SELECT TOP(@top) Id, Type FROM Schedules WHERE ActiveTime <= @selectTime";
            using SqlConnection connection = new(_connections.AnalyticsConnection);
            return (await connection.QueryAsync<Schedule>(query, new { top, selectTime })).AsList();
        }

        public virtual async Task<bool> SetActiveTimeAsync(string id, DateTime setTime)
        {
            var query = "UPDATE Schedules SET ActiveTime = @setTime, UpdatedTime = GETDATE() WHERE Id = @id";
            using SqlConnection connection = new(_connections.AnalyticsConnection);
            return await connection.ExecuteAsync(query, new { id, setTime }) > 0;
        }

        public virtual async Task BulkSetActiveTimeAsync(IEnumerable<Schedule> schedules)
        {
            if (schedules?.Count() <= 0)
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

            using SqlConnection connection = new(_connections.AnalyticsConnection);
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
                _logger.LogError(ex, "BulkSetActiveTimeAsync error");
                tran.Rollback();
                throw;
            }
        }

        public virtual async Task<Schedule> GetByIdAsync(string id)
        {
            var query = "SELECT * FROM Schedules WHERE Id = @id";
            using SqlConnection connection = new(_connections.AnalyticsConnection);
            return await connection.QueryFirstOrDefaultAsync<Schedule>(query, new { id });
        }

        public virtual async Task<Schedule> FindAsync(int type, string dataKey)
        {
            var query = "SELECT * FROM Schedules WHERE DataKey = @dataKey AND Type = @type";
            using SqlConnection connection = new(_connections.AnalyticsConnection);
            return await connection.QueryFirstOrDefaultAsync<Schedule>(query, new { type, dataKey });
        }

        public virtual async Task<bool> UpdateAsync(Schedule schedule)
        {
            string query = @"    UPDATE Schedules SET
			                            Name = @Name,
			                            DataKey = @DataKey,
			                            Type = @Type,
			                            Priority = @Priority,
			                            Activated = @Activated,
			                            ActiveTime = @ActiveTime,
		                                IsError = @IsError,
		                                OptionsJson = @OptionsJson,
                                        UpdatedTime = GETDATE()
		                            WHERE
			                            Id = @Id";
            using SqlConnection connection = new(_connections.AnalyticsConnection);
            return await connection.ExecuteAsync(query, schedule) > 0;
        }

        public virtual async Task BulkInserAsync(IEnumerable<Schedule> schedules)
        {
            if (schedules?.Count() <= 0)
            {
                return;
            }

            var dataTableTemp = schedules.ToDataTable();
            using SqlConnection connection = new(_connections.AnalyticsConnection);
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
                    sqlBulkCopy.DestinationTableName = "Schedules";
                    await sqlBulkCopy.WriteToServerAsync(dataTableTemp);
                }
                tran.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BulkInserScheduleAsync error");
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
                                        Priority,
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
                                        @Priority,
                                        @Activated,
                                        @ActiveTime,
                                        @IsError,
                                        @OptionsJson,
                                        @Type,
                                        @CreatedTime,
                                        GETDATE())";
            using SqlConnection connection = new(_connections.AnalyticsConnection);
            return await connection.ExecuteAsync(query, schedule) > 0;
        }

        public virtual async Task<bool> DeleteAsync(string id)
        {
            var query = "DELETE Schedules WHERE Id = @id";
            using SqlConnection connection = new(_connections.AnalyticsConnection);
            return await connection.ExecuteAsync(query, new { id }) > 0;
        }

        public virtual async Task<bool> DeleteByDataKeyAsync(string key)
        {
            var query = "DELETE Schedules WHERE DataKey = @key";
            using SqlConnection connection = new(_connections.AnalyticsConnection);
            return await connection.ExecuteAsync(query, new { key }) > 0;
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
            using SqlConnection connection = new(_connections.AnalyticsConnection);
            return await connection.ExecuteAsync(query, new { type, code }) > 0;
        }

        public virtual async Task<List<Schedule>> InitialAsync()
        {
            var queryCount = "SELECT COUNT(1) FROM Schedules";
            using SqlConnection connection = new(_connections.AnalyticsConnection);
            var count = await connection.QueryFirstOrDefaultAsync<int>(queryCount);
            if (count <= 0)
            {
                var schedules = new List<Schedule>
                {
                    new Schedule()
                    {
                        Name = "Tìm kiếm và bổ sung mã chứng khoáng từ ssi api",
                        Type = 0,
                        Priority = 50
                    },
                    new Schedule()
                    {
                        Name = "Lấy lãi suất ngân hàng cao nhất.",
                        Type = 10,
                        Priority = 80,
                        OptionsJson = JsonSerializer.Serialize(new Dictionary<string, string>() {
                            { "Length", $"3,6,12,24" }
                        }),
                        ActiveTime = DateTime.Now.Date.AddDays(1).AddHours(4).AddMinutes(5)
                    },
                    new Schedule()
                    {
                        Name = "Thu thập định giá của thị trường.",
                        Type = 11,
                        Priority = 80,
                        OptionsJson = JsonSerializer.Serialize(new Dictionary<string, string>() {
                            { "Indexs", string.Join(',', Constants.ShareIndex) }
                        }),
                        ActiveTime = DateTime.Now.Date.AddDays(1).AddHours(4).AddMinutes(2)
                    },
                    new Schedule()
                    {
                        Name = "Thu thập giá nguyên vật liệu, tài nguyên.",
                        Type = 12,
                        Priority = 80,
                        ActiveTime = DateTime.Now.Date.AddDays(1).AddHours(4).AddMinutes(1)
                    },
                    new Schedule()
                    {
                        Name = "Thu thập dữ liệu cho chỉ số chiên tranh.",
                        Type = 13,
                        Priority = 80,
                        ActiveTime = DateTime.Now.Date.AddDays(1).AddHours(4).AddMinutes(1)
                    },
                    new Schedule()
                    {
                        Name = "Thu thập dữ liệu cho chỉ số dịch bệnh.",
                        Type = 14,
                        Priority = 80,
                        ActiveTime = DateTime.Now.Date.AddDays(1).AddHours(4).AddMinutes(1)
                    },
                    new Schedule()
                    {
                        Name = "Thu thập dữ liệu cho chỉ số thiên tai.",
                        Type = 15,
                        Priority = 80,
                        ActiveTime = DateTime.Now.Date.AddDays(1).AddHours(4).AddMinutes(1)
                    },
                    new Schedule()
                    {
                        Name = "Thu thập dữ liệu cho chỉ số thể chế, chính sách.",
                        Type = 16,
                        Priority = 80,
                        ActiveTime = DateTime.Now.Date.AddDays(1).AddHours(4).AddMinutes(1)
                    },

                    new Schedule()
                    {
                        Name = "Dự đoán giá cổ phiểu bằng mô hình ftt",
                        Type = 100,
                        Priority = 95,
                        ActiveTime = DateTime.Now.Date.AddDays(1).AddHours(4).AddMinutes(45)
                    },
                    new Schedule()
                    {
                        Name = "Dự đoán giá cố phiếu bằng thuật toán ssa.",
                        Type = 101,
                        Priority = 95,
                        ActiveTime = DateTime.Now.Date.AddDays(1).AddHours(4).AddMinutes(50)
                    },
                    new Schedule()
                    {
                        Name = "Phân loại hành động giá bằng thuật toán SdcaMaximumEntropy.",
                        Type = 102,
                        Priority = 95,
                        ActiveTime = DateTime.Now.Date.AddDays(1).AddHours(4).AddMinutes(55)
                    },

                    new Schedule()
                    {
                        Name = "Đánh giá mã ngành tự động.",
                        Type = 204,
                        Priority = 95,
                        ActiveTime = DateTime.Now.Date.AddDays(1).AddHours(5).AddMinutes(1)
                    },
                    new Schedule()
                    {
                        Name = "Xử lý ngày giao dịch không hưởng quyền chi trả cổ tức thì xóa và import lại lịch sử giá.",
                        Type = 205,
                        Priority = 95,
                        ActiveTime = DateTime.Now.Date.AddDays(1).AddHours(2).AddMinutes(50)
                    },
                    new Schedule()
                    {
                        Name = "Phân tích tâm lý thị trường.",
                        Type = 207,
                        Priority = 95,
                        ActiveTime = DateTime.Now.Date.AddDays(5).AddHours(10).AddMinutes(1)
                    },

                    new Schedule()
                    {
                        Name = "Xử lý hiển thị dữ liệu chứng khoán cho hiển thị.",
                        Type = 300,
                        Priority = 30,
                        ActiveTime = DateTime.Now.AddMinutes(60)
                    }
                };
                foreach (var item in Constants.ShareIndex)
                {
                    schedules.Add(new Schedule()
                    {
                        Name = $"Bổ sung lịch sử dữ liệu cho chỉ số {item}.",
                        Type = 9,
                        Priority = 50,
                        DataKey = item,
                        OptionsJson = JsonSerializer.Serialize(new Dictionary<string, string>() {
                            { "StartTime", $"{new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.FromMilliseconds(0)).ToUnixTimeSeconds()}" }
                        }),
                        ActiveTime = DateTime.Now.Date.AddDays(1).AddHours(4).AddMinutes(1)
                    });
                }
                await BulkInserAsync(schedules);
                return schedules;
            }
            return new();
        }
    }
}