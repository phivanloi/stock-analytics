using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Pl.Sas.Logger.Data
{
    public class LoggerData
    {
        private readonly Connections _connections;

        public LoggerData(IOptions<Connections> optionsConnection)
        {
            _connections = optionsConnection.Value;
        }

        #region LogView
        public virtual async Task<IEnumerable<LogEntry>> FindAllAsync(int top = 1000, long? startTime = null, int? type = null)
        {
            var query = "SELECT TOP(@top) * FROM LogEntries ";
            var subQuery = string.Empty;
            if (startTime.HasValue)
            {
                subQuery += " WHERE CreatedTime > @startTime ";
            }
            if (type.HasValue)
            {
                if (string.IsNullOrEmpty(subQuery))
                {
                    subQuery += " WHERE Type = @type ";
                }
                else
                {
                    subQuery += " AND Type = @type ";
                }
            }
            using SqlConnection connection = new(_connections.LoggingConnection);
            return await connection.QueryAsync<LogEntry>($"{query} {subQuery} ORDER BY CreatedTime DESC", new { top, startTime, type });
        }

        public virtual async Task<LogEntry> FindAsync(string id)
        {
            var query = "SELECT * FROM LogEntries WHERE Id = @id";
            using SqlConnection connection = new(_connections.LoggingConnection);
            return await connection.QueryFirstOrDefaultAsync<LogEntry>(query, new { id });
        }

        public virtual async Task ClearLogAsync()
        {
            var query = "TRUNCATE TABLE LogEntries";
            using SqlConnection connection = new(_connections.LoggingConnection);
            await connection.ExecuteAsync(query);
        }

        public async Task<bool> WriteAsync(LogEntry logEntry)
        {
            var query = $@"   INSERT INTO LogEntries
                                        (Id,
                                        CreatedTime,
                                        Message,
                                        Content,
                                        Host,
                                        Type)
                                    VALUES
                                        (@Id,
                                        @CreatedTime,
                                        @Message,
                                        @Content,
                                        @Host,
                                        @Type)";
            using SqlConnection connection = new(_connections.LoggingConnection);
            return await connection.ExecuteAsync(query, logEntry) > 0;
        }
        #endregion

        #region Scheduler
        public virtual long RecurrentDelete(int skipSecond = 36000)
        {
            var queryCount = "SELECT COUNT(1) FROM LogEntries";
            using SqlConnection connection = new(_connections.LoggingConnection);
            var count = connection.QueryFirstOrDefault<long>(queryCount);
            if (count > skipSecond)
            {
                var getStartItemQuery = "SELECT CreatedTime FROM LogEntries ORDER BY CreatedTime DESC OFFSET @skipSecond ROWS FETCH NEXT 1 ROWS ONLY";
                var startTime = connection.QueryFirstOrDefault<long>(getStartItemQuery, new { skipSecond });
                var queryDelete = "DELETE LogEntries WHERE CreatedTime <= @startTime";
                return connection.Execute(queryDelete, new { startTime });
            }
            return 0;
        }
        #endregion
    }
}
